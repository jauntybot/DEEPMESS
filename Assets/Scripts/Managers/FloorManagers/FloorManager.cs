using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Linq;


using UnityEditor;

// This class generates and sequences Floor prefabs (Grid class), as well as manages the descending of units

public class FloorManager : MonoBehaviour {

    UIManager uiManager;
    ScenarioManager scenario;
    MusicController musicController;

    [Header("Floor Serialization")]
    [SerializeField] public GameObject floorPrefab;
    [SerializeField] Transform floorParent;
    [SerializeField] public FloorSequence floorSequence;
    public TutorialSequence tutorial;
    public bool gridHightlightOverride;
    public Grid currentFloor;
    public List<Grid> floors;

    [SerializeField] int _gridSize;
    public static int gridSize;
    [SerializeField] float _sqrSize;
    public static float sqrSize;
    [SerializeField] GameObject godParticlePrefab;
    
    [Header("Floor Transitioning")]
    public Transform transitionParent;
    public float floorOffset, transitionDur, unitDropDur;
    public AnimationCurve dropCurve;
    public bool transitioning, peeking, descending;
    int floorCount;
    bool cavityWait = false;
    public Coroutine floating;
    [SerializeField] ParallaxImageScroll parallax;
    public DescentPreviewManager previewManager;
    [SerializeField] SFX cavityTransition;

    public delegate void OnFloorAction();
    public virtual event OnFloorAction DescendingUnits;
    public virtual event OnFloorAction DescendingFloors;
    public virtual event OnFloorAction FloorDescended;
   
    [Header("Grid Viz")]
    public Color playerColor;
    public Color enemyColor, equipmentColor, movePreview, enemyMovePreview;
    private bool notation = false;


    #region Singleton (and Awake)

    public static FloorManager instance;
    private void Awake() {
        if (FloorManager.instance) {
            Debug.Log("Warning! More than one instance of FloorManager found!");
            return;
        }
        FloorManager.instance = this;
        gridSize = _gridSize; sqrSize = _sqrSize;
    }
    #endregion

    public IEnumerator Init(int startIndex) {
        if (ScenarioManager.instance) scenario = ScenarioManager.instance;
        if (UIManager.instance) uiManager = UIManager.instance;
        if (PersistentMenu.instance) musicController = PersistentMenu.instance.musicController;

        floorSequence.Init(startIndex);
        if (startIndex == 0) {
            tutorial.gameObject.SetActive(true);
            tutorial.Initialize(scenario);
            previewManager.tut = true;
        } else {
            tutorial.gameObject.SetActive(false);
        }
        
        floorCount = 1;
        nailTries = 0;

        yield return null;
    }

    public void GenerateFloor(FloorDefinition definitionOverride = null, bool first = false) {
        int index = floorSequence.floorsGot;

        Grid newFloor = Instantiate(floorPrefab, floorParent).GetComponent<Grid>();
        newFloor.transform.localPosition = new Vector3(0, floors.Count * -floorOffset);
        
        FloorDefinition floorDef;
        if (definitionOverride)
            floorDef = definitionOverride;
        else 
            floorDef = floorSequence.GetFloor(first);

        newFloor.lvlDef = floorDef;

        if (first) 
            currentFloor = newFloor;
         
        newFloor.GenerateGrid(index);
        newFloor.gameObject.name = "Floor" + newFloor.index;
        
        newFloor.ToggleChessNotation(notation);
        newFloor.overrideHighlight = gridHightlightOverride;
    
        floors.Add(newFloor);
        
        if (!first) {
            newFloor.transform.localScale = Vector3.one * 0.75f;
            newFloor.GetComponent<SortingGroup>().sortingOrder = -2;
            previewManager.UpdateFloors(floors[floors.Count - 2], newFloor);            
        }
    }

    
    public Color GetFloorColor(int i) {
        Color c = equipmentColor;
        
        switch (i) {
            case 0: c = playerColor; break; 
            case 1: c = enemyColor; break;
            case 2: c = equipmentColor; break;
            case 3: c = movePreview; break;
            case 4: c = enemyMovePreview; break;
        }
        return c;
    }

    // public void ChessNotationToggle() {
    //     notation = !notation;
    //     foreach (Grid floor in floors)
    //         floor.ToggleChessNotation(notation);
    // }

    public void GridHighlightToggle() {
        gridHightlightOverride = !gridHightlightOverride;
        foreach (Grid floor in floors) {
            floor.overrideHighlight = gridHightlightOverride;
        }
    }


// Huge function, animation should be seperated
// Handles all calls to transition the floor view, from upper to lower, lower to upper, preview or descent
    public IEnumerator TransitionFloors(bool down, bool preview) {        
// Orients the animation
        int dir = down? 1 : -1;
        Grid toFloor = null;
        floorCount += dir;
        UpdateFloorCounter();

        if (floors.Count - 1 >= currentFloor.transform.GetSiblingIndex() + dir && currentFloor.transform.GetSiblingIndex() + dir >= 0) // Checks if there is a floor in the direction transitioning
            toFloor = floors[currentFloor.transform.GetSiblingIndex() + dir];
// Block player from selecting units
        scenario.player.ToggleUnitSelectability(dir == -1);

// Adjust sorting orders contextually
        Vector3 toFromScale = Vector3.one;
        if (toFloor) {
            toFloor.GetComponent<SortingGroup>().sortingOrder = 0;
            toFromScale = toFloor.transform.localScale;
            toFloor.gameObject.SetActive(true);
        }
        if (!preview) currentFloor.GetComponent<SortingGroup>().sortingOrder = -1;
        else if (!down) currentFloor.GetComponent<SortingGroup>().sortingOrder = -1;
        else currentFloor.GetComponent<SortingGroup>().sortingOrder = 1;
// Local params for animation
        Vector3 from = floorParent.transform.position;
        Vector3 to = new(from.x, from.y + floorOffset * dir, from.z);
        Vector3 fromScale = currentFloor.transform.localScale;
        Vector3 toScale = down? Vector3.one : Vector3.one * 0.75f;

        float currFromA = 1;
        float currToA = down? 0 : 1;

        peeking = down && preview;
        HideUnits(down);

// And the actual animation. Has become cluttered with NestedFadeGroup logic too.
        transitioning = true;
        float timer = 0;
        while (timer <= transitionDur) {
// Lerp position of floor contatiner
            floorParent.transform.position = Vector3.Lerp(from, to, timer/transitionDur);
            currentFloor.transform.localScale = Vector3.Lerp(fromScale, toScale, timer/transitionDur);
            if (toFloor) {
                toFloor.transform.localScale = Vector3.Lerp(toFromScale, Vector3.one, timer/transitionDur);
// Fade in destination floor if present
                if (!down)
                    toFloor.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = Mathf.Lerp(0, 1, timer/transitionDur);
            }
            
// Fade previewed GridElements like player units
            currentFloor.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = Mathf.Lerp(currFromA, currToA, timer/transitionDur);
            
            if (parallax)
                parallax.ScrollParallax(Time.deltaTime * (down ? -1 : 1));
// Coroutine/animation lerp yield
            yield return null;
            timer += Time.deltaTime;
        }
        transitioning = false;
// Hard set lerped variables
        floorParent.transform.position = to;
        currentFloor.transform.localScale = toScale;
        currentFloor.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = currToA;
        if (toFloor) {
            toFloor.transform.localScale = Vector3.one;
            toFloor.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = 1;
        }
// Update floor manager current floor... preview next floor untis stats?
        if (down && currentFloor.transform.GetSiblingIndex()-1 >= 0) floors[currentFloor.transform.GetSiblingIndex()-1].gameObject.SetActive(false);
        if (toFloor) {
            toFloor.LockGrid(false);
            currentFloor.LockGrid(true);
            currentFloor = toFloor;
        }           
    }
    

    public IEnumerator TransitionToSlimeHub(bool up) {
        //yield return StartCoroutine(TransitionFloors(!up, false));
        musicController.SwitchMusicState(up ? MusicController.MusicState.Ginos : musicController.currentState, true, true);
        float timer = 0f;
        Vector3 origin = floorParent.transform.position;
        NestedFadeGroup.NestedFadeGroup fade = floorParent.GetComponent<NestedFadeGroup.NestedFadeGroup>();
        scenario.player.GridMouseOver(new Vector2(-32, -32), false);
        
        EnemyManager enemy = (EnemyManager)currentFloor.enemy;
        if (!up) {
            parallax.gameObject.SetActive(true);
        } else {
            foreach(Transform spawn in enemy.spawnParent) 
                spawn.gameObject.SetActive(false);
        }

        int sign = up ? 1 : -1;
        bool toggle = false;
        while (timer <= 2f) {
            parallax.ScrollParallax(Time.deltaTime * Mathf.Lerp(0, 5 * sign, timer/2f));
            
            parallax.GetComponent<SpriteRenderer>().material.SetFloat("_Alpha", Mathf.Lerp(up? 1 : 0, up? 0 : 1, up? (timer-1.75f)/.25f : timer/0.25f));
            parallax.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = Mathf.Lerp(up? 1 : 0, up? 0 : 1, up? (timer-1.75f)/.25f : timer/0.25f);
            parallax.slimeHub.transform.position = new Vector3(0, Mathf.Lerp(up? 1.5f : -0.5f, up? -0.5f : 1.5f, up? (timer-1.75f)/.25f : timer/0.25f));

            floorParent.transform.position = new Vector3(origin.x, Mathf.Lerp(origin.y, up? origin.y - 18 : origin.y + 18, up? timer/0.5f : (timer-1.5f)/.5f));
            fade.AlphaSelf = Mathf.Lerp(up? 1 : 0, up? 0 : 1, up? timer/0.25f : (timer-1.5f)/.5f);
            
            if (!toggle && timer >= 1.0f) {
                toggle = true;
                parallax.slimeHub.SetActive(up);
            }

            yield return null;
            timer += Time.deltaTime;
        }
        parallax.GetComponent<SpriteRenderer>().material.SetFloat("_Alpha", up? 0 : 1);
        parallax.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = up? 0 : 1;

        floorParent.transform.position = new Vector3(origin.x, origin.y - sign * 18);
        fade.AlphaSelf = up? 0 : 1;

        parallax.gameObject.SetActive(!up);
        if (!up) {
            foreach(Transform spawn in enemy.spawnParent) 
                spawn.gameObject.SetActive(true);
        }
    }

    // Function that hides units when previewing the next floor -- MOVE TO UNIT FUNCTIONALITY
    Dictionary<Unit, bool> targetedDict = new();
    public void HideUnits(bool state) {
        if (state) {
            targetedDict = new Dictionary<Unit, bool>();
            if (targetedDict.Count == 0) {
                foreach (Unit u in scenario.player.units) {
                    targetedDict.Add(u, u.targeted);
                    u.elementCanvas.ToggleStatsDisplay(false);
                }
                foreach (Unit u in scenario.currentEnemy.units) {
                    targetedDict.Add(u, u.targeted);
                    u.elementCanvas.ToggleStatsDisplay(false);
                    u.ElementDestroyed += RemoveFromDict;
                }
            }
        } else {
            foreach (KeyValuePair<Unit, bool> entry in targetedDict)
                entry.Key.elementCanvas.ToggleStatsDisplay(entry.Value);
        }
    }

    void RemoveFromDict(GridElement ge) {
        if (targetedDict.ContainsKey((Unit)ge))
            targetedDict.Remove((Unit)ge);
    }


    public IEnumerator ChooseLandingPositions() {
        while (scenario.currentTurn == ScenarioManager.Turn.Cascade)
            yield return null;
        //StartCoroutine(ToggleDescentPreview(false));
        previewManager.UpdateFloors(floors[floors.Count - 2], floors[floors.Count - 1]);        
        currentFloor.DisableGridHighlight();

    }

    public void Descend(bool cascade = false, bool nail = true, Vector2 pos = default) {
        bool tut = floorSequence.activePacket.packetType == FloorChunk.PacketType.Tutorial;
        StartCoroutine(DescendFloors(cascade, tut, nail, pos));
        
    }

    public IEnumerator DescendFloors(bool cascade = false, bool tut = false, bool nail = true, Vector2 pos = default) {
// Lock up current floor
        descending = true;
        
        EnemyManager enemy = (EnemyManager)currentFloor.enemy;
        enemy.InterruptReinforcements();
        List<Unit> enemyUnits = new();
        foreach (GridElement ge in currentFloor.gridElements) {
            if (ge is EnemyUnit eu)
                enemyUnits.Add(eu);
        }

        currentFloor.DisableGridHighlight();
        currentFloor.LockGrid(true);

        FloorDescentEvent evt = ObjectiveEvents.FloorDescentEvent;
        evt.floorIndex = currentFloor.index;
        evt.enemyDescentsCount = enemyUnits.Count;
        ObjectiveEventManager.Broadcast(evt);

        if (nail)
            yield return StartCoroutine(currentFloor.ShockwaveCollapse(pos));

        DescendingFloors?.Invoke();

        ScenarioManager.Scenario scen = ScenarioManager.Scenario.Null;
        if (tut) {
            yield return StartCoroutine(TutorialSequence.instance.TutorialDescend());
        } else {

// Check if at the end of packet / if there was a sub floor generated, if not packet is done
            if (floors.Count - 1 > currentFloor.transform.GetSiblingIndex()) {
// Generate next floor if still mid packet
                if (!floorSequence.ThresholdCheck()) { //|| floorSequence.currentThreshold == FloorChunk.PacketType.BOSS
                    GenerateFloor();       
                }
                if (floorSequence.currentThreshold == FloorChunk.PacketType.BOSS) scen = ScenarioManager.Scenario.Boss;

                scenario.player.nail.ToggleNailState(Nail.NailState.Falling);   
                yield return StartCoroutine(TransitionFloors(true, false));
                
                yield return StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Descent, scen));
                yield return new WaitForSecondsRealtime(0.25f);
                
// Descend units from previous floor
                yield return StartCoroutine(DescendUnits(floors[currentFloor.transform.GetSiblingIndex() -1].gridElements, enemy, true));
                
                if (currentFloor.index+1 == floorSequence.activePacket.packetLength) scenario.player.nail.barkBox.Bark(BarkBox.BarkType.FinalFloor);

                StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Enemy));
            } else {
    // NODE LOGIC
                yield return StartCoroutine(TransitionPackets((EnemyManager)currentFloor.enemy));
            }
        }
        FloorDescended?.Invoke();

        descending = false;
    }

    void UpdateFloorCounter(int max = -1) {
        if (floorSequence.activePacket.packetType != FloorChunk.PacketType.BOSS)
            uiManager.metaDisplay.UpdateCurrentFloor(floorCount, max);
        else 
            uiManager.metaDisplay.UpdateCurrentFloor(floorCount);
    }

    public IEnumerator DescendUnits(List<GridElement> units, EnemyManager enemy = null, bool hardLand = false) {
        Beacon b = (Beacon)units.Find(ge => ge is Beacon);
        if (b) units.Remove(b);

        UpdateFloorCounter();
        Coroutine drop = StartCoroutine(DropUnits(units, hardLand));

        scenario.currentEnemy = (EnemyManager)currentFloor.enemy;
        
        yield return drop;

        if (enemy) {
            enemy.SeedUnits(currentFloor, enemy == currentFloor.enemy);
        }

// Spawns elite
        if (currentFloor.lvlDef.spawnElite) {
            yield return new WaitForSecondsRealtime(0.75f);
            yield return StartCoroutine(SpawnBoss(floorSequence.elitePrefab, true));
            currentFloor.lvlDef.spawnElite = false;
        }

        scenario.player.DescendGrids(currentFloor);
        currentFloor.LockGrid(false);

        if (uiManager.gameObject.activeSelf)    
            uiManager.metaDisplay.UpdateEnemiesRemaining(scenario.currentEnemy.units.Count);

// Check for tutorial tooltip triggers
        if (floorSequence.activePacket.packetType != FloorChunk.PacketType.Tutorial) {
            if (currentFloor.lvlDef.initSpawns.Find(spawn => spawn.asset.prefab.GetComponent<GridElement>() is TileBulb) != null && !scenario.gpOptional.bulbEncountered)
                scenario.gpOptional.StartCoroutine(scenario.gpOptional.TileBulb());
            if (currentFloor.gridElements.Find(ge => ge is Beacon) && !scenario.gpOptional.beaconEncountered)
                scenario.gpOptional.StartCoroutine(scenario.gpOptional.Beacon());
            if (currentFloor.gridElements.Find(ge => ge is BloatedBulb) && !scenario.gpOptional.bloatedBulbEncountered)
                scenario.gpOptional.StartCoroutine(scenario.gpOptional.BloatedBulb());
        }
    }


// Coroutine that sequences the descent of all valid units
    public IEnumerator DropUnits(List<GridElement> units, bool hardLand) {

        scenario.player.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = 1;
        scenario.player.transform.parent = currentFloor.transform;
        
        if (scenario.currentEnemy) {
            scenario.currentEnemy.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = 1;
            scenario.currentEnemy.transform.parent = currentFloor.transform;    
        }

        DescendingUnits?.Invoke();

        foreach (GridElement ge in units) {
            if (ge is Unit u) {
                u.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = 0;
                u.transform.parent = transitionParent;
            }
        }

// Sort list of descending units in order of faction, controls order of descent
        GridElement[] units1 = units.ToArray();
        Array.Sort<GridElement>(units1, (a,b) => {
            int x, y;
            if (a is EnemyUnit) x = 2;
            else if (a is PlayerUnit) x = 1;
            else x = 0;
            if (b is EnemyUnit) y = 2;
            else if (b is PlayerUnit) y = 1;
            else y = 0;
            return x.CompareTo(y);
        });
        units = units1.ToList();

        Nail nail = null;
        List<Coroutine> descents = new();

        for (int i = units1.Length - 1; i >= 0; i--) {
            if (units1[i] is Unit u) {
                if (u is not Nail) {
                    // if (u is PlayerUnit && currentFloor.slagSpawns.Count > 0) {
                    //     u.coord = currentFloor.slagSpawns[0];
                    //     currentFloor.slagSpawns.RemoveAt(0);
                    // } 
                    GridElement subElement = null;
                    foreach (GridElement ge in currentFloor.CoordContents(u.coord)) subElement = ge;
                    descents.Add(StartCoroutine(DropUnit(u, currentFloor.PosFromCoord(u.coord) + new Vector3 (0, floorOffset*2, 0), currentFloor.PosFromCoord(u.coord), subElement, hardLand)));
                    yield return new WaitForSeconds(unitDropDur*1.5f);
                    
                } else 
                    nail = (Nail)u;
                
            }
        }
        for (int i = descents.Count - 1; i >= 0; i--) {
            if (descents[i] != null) 
                yield return descents[i];
            else
                descents.RemoveAt(i);
        }
        yield return new WaitForSecondsRealtime(0.75f);
        
        if (nail && floorSequence.activePacket.packetType != FloorChunk.PacketType.BOSS) {
            yield return StartCoroutine(DropNail(nail));
            //yield return StartCoroutine(DropParticle());
        }
    }

// Coroutine that houses the logic to descend a single unit
    public IEnumerator DropUnit(Unit unit, Vector3 from, Vector3 to, GridElement subElement = null, bool hardLand = false) {
        float timer = 0;
        NestedFadeGroup.NestedFadeGroup fade = unit.GetComponent<NestedFadeGroup.NestedFadeGroup>();
        unit.airTraillVFX.SetActive(true);
        if (unit.gfxAnim)
            unit.gfxAnim.SetBool("Falling", true);
        // float slowLeft = 0;
        while (timer <= unitDropDur) {
            unit.transform.position = Vector3.Lerp(from, to, dropCurve.Evaluate(timer/unitDropDur));
            fade.AlphaSelf = Mathf.Lerp(0, 1, timer/(unitDropDur/3));
            
            yield return null;
            timer += Time.deltaTime;

            // if (subElement) {
            //     if (Time.timeScale != 0.025f && timer > unitDropDur*(9f/10f)) {
            //         Time.timeScale = 0.025f;
            //         Debug.Log("Slow at " + timer + "/ " + unitDropDur*(9f/10f));
            //         slowLeft = timer;
            //     }
            // }
        }
        // Time.timeScale = 1;
        unit.transform.parent = unit.manager.unitParent.transform;
        unit.DescentVFX(currentFloor.tiles.Find(sqr => sqr.coord == unit.coord), subElement);
        unit.transform.position = to;
        unit.StoreInGrid(currentFloor);
        unit.UpdateElement(unit.coord);
        fade.AlphaSelf = 1;
        

        unit.PlaySound(unit.landingSFX);
        if (unit.gfxAnim)
            unit.gfxAnim.SetBool("Falling", false);
        if (unit is PlayerUnit pu && pu.equipment.Find(e => e is HammerData)) {
            scenario.player.hammerActions[0].hammer.SetActive(false);
            scenario.player.hammerActions[0].hammer.GetComponentInChildren<Animator>().SetBool("Falling", false);
        }

        List<Coroutine> cos = new();

        if (!unit.fragile) {
            hardLand = false;
        }

        if (subElement) {
            cos.Add(StartCoroutine(subElement.CollideFromBelow(unit)));
            cos.Add(StartCoroutine(unit.CollideFromAbove(subElement, hardLand?1:0)));
        } else if (hardLand && currentFloor.tiles.Find(t => t.coord == unit.coord).tileType != Tile.TileType.Bile) {
            yield return StartCoroutine(unit.TakeDamage(1, GridElement.DamageType.Fall));
        }

        if (cos.Count > 0) {
             for (int i = cos.Count - 1; i >= 0; i--) {
                if (cos[i] != null) 
                    yield return cos[i];
                else
                    cos.RemoveAt(i);
            }
        }
    }

// Coroutine for descending the nail at a regulated random position
         public int nailTries;
    public IEnumerator DropNail(Nail nail) {
        nail.ToggleNailState(Nail.NailState.Falling);

        bool validCoord = false;
        Vector2 spawn = Vector2.zero;

        ShuffleBag<Vector2> spawns = new();
        for (int x = 1; x <= 6; x++) { for (int y = 1; y <= 6; y++) { spawns.Add(new Vector2(x,y)); } }

        int tries = nailTries;
// Find a valid coord that a player unit is not in
        while (!validCoord) {
            validCoord = true;
            if (currentFloor.nailSpawns.Count > 0) {
                int i = UnityEngine.Random.Range(0, currentFloor.nailSpawns.Count);
                spawn = currentFloor.nailSpawns[i];
                currentFloor.nailSpawns.RemoveAt(i);
            }
            else
                spawn = spawns.Next();
                
            foreach(Unit u in scenario.player.units) {
                if (u.coord == spawn && u is not Nail) validCoord = false;
            }

            if (currentFloor.gridElements.Find(ge => ge.coord == spawn) is Beacon) validCoord = false;
            if (currentFloor.tiles.Find(sqr => sqr.coord == spawn).tileType == Tile.TileType.Bile) validCoord = false;
            if (currentFloor.tiles.Find(sqr => sqr.coord == spawn) is TileBulb) validCoord = false;
            if (!currentFloor.enemy.units.Find(u => u.coord == spawn) && tries > 0) {
                validCoord = false;
                tries--;
            } 
        }
        
        nail.transform.position = nail.grid.PosFromCoord(spawn);
        nail.UpdateSortOrder(spawn);
        nail.coord = spawn;

        GridElement subElement = null;
        foreach (GridElement ge in currentFloor.CoordContents(nail.coord)) subElement = ge;

        Vector3 to = currentFloor.PosFromCoord(spawn);
        Vector3 from = to + new Vector3(0, floorOffset*2, 0);


        nail.PlaySound(nail.selectedSFX);

        float timer = 0;
        NestedFadeGroup.NestedFadeGroup fade = nail.GetComponent<NestedFadeGroup.NestedFadeGroup>();
        nail.airTraillVFX.SetActive(true);
        while (timer <= unitDropDur) {
            nail.transform.localPosition = Vector3.Lerp(from, to, dropCurve.Evaluate(timer/unitDropDur));
            fade.AlphaSelf = Mathf.Lerp(0, 1, timer/(unitDropDur/3));
            yield return null;
            timer += Time.deltaTime;
        }
        nail.DescentVFX(currentFloor.tiles.Find(sqr => sqr.coord == nail.coord), subElement);
        nail.transform.position = to;
        nail.StoreInGrid(currentFloor);
        nail.ToggleNailState(Nail.NailState.Buried);
        fade.AlphaSelf = 1;

        nail.PlaySound(nail.landingSFX);

        if (subElement) {
            StartCoroutine(subElement.CollideFromBelow(nail));
            StartCoroutine(nail.CollideFromAbove(subElement, 0));
        }
        nail.transform.parent = nail.manager.transform;
    }

    public IEnumerator SpawnBoss(Unit u, bool relic = false) {
        bool validCoord = false;
        Vector2 spawn = Vector2.zero;
        while (!validCoord) {
            validCoord = true;

            spawn = new Vector2(UnityEngine.Random.Range(1,6), UnityEngine.Random.Range(1,6));

            foreach (GridElement ge in currentFloor.gridElements) {
                if (ge.coord == spawn) validCoord = false;
            }

            if (currentFloor.tiles.Find(sqr => sqr.coord == spawn).tileType == Tile.TileType.Bile) validCoord = false; 
        }
    
        Unit unit = scenario.currentEnemy.SpawnBossUnit(spawn, u, relic);
        Vector3 to = currentFloor.PosFromCoord(spawn);
        Vector3 from = to + new Vector3(0, floorOffset*2, 0);

        float timer = 0;
        NestedFadeGroup.NestedFadeGroup fade = unit.GetComponent<NestedFadeGroup.NestedFadeGroup>();
        unit.airTraillVFX.SetActive(true);
        while (timer <= unitDropDur) {
            unit.transform.localPosition = Vector3.Lerp(from, to, dropCurve.Evaluate(timer/unitDropDur));
            fade.AlphaSelf = Mathf.Lerp(0, 1, timer/(unitDropDur/3));
            yield return null;
            timer += Time.deltaTime;
        }
        unit.DescentVFX(currentFloor.tiles.Find(sqr => sqr.coord == unit.coord));
        unit.transform.position = to;
        unit.StoreInGrid(currentFloor);
        unit.UpdateElement(unit.coord);
        fade.AlphaSelf = 1;

        unit.PlaySound(unit.landingSFX);
    
        scenario.currentEnemy.UpdateTurnOrder();
        if (floorSequence.currentThreshold == FloorChunk.PacketType.BOSS) 
            scenario.gpOptional.StartCoroutine(scenario.gpOptional.Boss());        
    }


    public IEnumerator TransitionPackets(EnemyManager lastFloorEnemey = null) {
        if (currentFloor) {
            yield return StartCoroutine(TransitionFloors(true, false));
            
            yield return StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Descent));
        }
        yield return new WaitForSecondsRealtime(0.25f);

// Lerp units into screen
        List<Unit> units = new() { scenario.player.units[0], scenario.player.units[1], scenario.player.units[2], scenario.player.units[3] };
        List<Vector2> to = new() {new Vector2(7.5f, 1f), new Vector2(6.2f, 2), new Vector2(8.2f, 2.4f), new Vector2(8.4f, -1f) };
        foreach (Unit u in units) u.gfxAnim.SetBool("Falling", true);
        scenario.player.hammerActions[0].hammer.SetActive(true);
        scenario.player.hammerActions[0].hammer.GetComponentInChildren<Animator>().SetBool("Falling", true);
        foreach (Unit u in units) 
            u.transform.parent = transitionParent;
        
        scenario.player.nail.ToggleNailState(Nail.NailState.Falling);   
        float timer = 0;
        while (timer <= unitDropDur) {
            parallax.ScrollParallax(Time.deltaTime * -1);
            for (int i = 0; i <= units.Count - 1; i++) {
                units[i].transform.localPosition = Vector3.Lerp(to[i] + new Vector2(0, floorOffset*2), to[i], dropCurve.Evaluate(timer/unitDropDur));
                NestedFadeGroup.NestedFadeGroup fade = units[i].GetComponent<NestedFadeGroup.NestedFadeGroup>();
                fade.AlphaSelf = Mathf.Lerp(0, 1, timer/(unitDropDur/3));
            }
            yield return null;
            timer += Time.deltaTime;
        }
        for (int i = 0; i <= units.Count - 1; i++) {
            units[i].transform.localPosition = to[i];
            NestedFadeGroup.NestedFadeGroup fade = units[i].GetComponent<NestedFadeGroup.NestedFadeGroup>();
            fade.AlphaSelf = 1;
        }
        
// Endlessly falling
        cavityWait = true;
        floating = StartCoroutine(FloatingUnits());


        uiManager.ToggleBattleCanvas(false);

// Path sequence and contextual tooltips
        if (floorSequence.currentThreshold != FloorChunk.PacketType.BARRIER) {
            if (floorSequence.currentThreshold != FloorChunk.PacketType.Tutorial && tutorial.isActiveAndEnabled && !tutorial.sequenceEnd) yield return tutorial.StartCoroutine(tutorial.TutorialEnd());            
                if (!scenario.gpOptional.pathsEncountered) 
                    yield return StartCoroutine(scenario.gpOptional.Paths());
                
                yield return scenario.pathManager.PathSequence(floorSequence.currentThreshold != FloorChunk.PacketType.I && floorSequence.currentThreshold != FloorChunk.PacketType.Tutorial); 

                if (floorSequence.currentThreshold == FloorChunk.PacketType.BOSS) {
                    if (!scenario.gpOptional.prebossEncountered)
                        yield return StartCoroutine(scenario.gpOptional.Preboss());
                    uiManager.ToggleObjectiveTracker(false);
            }

            MusicController.MusicState state;
            int s;
            switch(floorSequence.currentThreshold) {
                default:
                case FloorChunk.PacketType.I: s = 1; state = MusicController.MusicState.Chunk1; break;
                case FloorChunk.PacketType.II: s = 2; state = MusicController.MusicState.Chunk2; break;
                case FloorChunk.PacketType.BOSS: s = 3; state = MusicController.MusicState.Chunk3; break;
            }
            if (s != scenario.startCavity) {
                Debug.Log(s + ", " + scenario.startCavity);
                musicController.SwitchMusicState(state, true);
            }

// Lerp units offscreen
            scenario.player.nail.PlaySound(cavityTransition);
            StopCoroutine(floating);
            timer = 0;
            Vector3 startPos = transitionParent.transform.position;
            while (timer <= unitDropDur*2) {
                parallax.ScrollParallax(Time.deltaTime * -1);
                transitionParent.transform.position = Vector3.Lerp(startPos, Vector3.zero, timer/unitDropDur);
                timer += Time.deltaTime;
                yield return null;
            }

            transitionParent.transform.position = Vector3.zero;
            cavityWait = false;

            timer = 0;
            while (timer <= unitDropDur) {
                parallax.ScrollParallax(Time.deltaTime * -1);
                for (int i = 0; i <= units.Count - 1; i++) {
                    NestedFadeGroup.NestedFadeGroup fade = units[i].GetComponent<NestedFadeGroup.NestedFadeGroup>();
                    units[i].transform.position = Vector3.Lerp(to[i], to[i] + new Vector2(0, floorOffset*2), dropCurve.Evaluate(timer/unitDropDur));
                    fade.AlphaSelf = Mathf.Lerp(1, 0, (timer - (unitDropDur*2/3))/(unitDropDur/3));
                }
                timer += Time.deltaTime;
                yield return null;
            }
            for (int i = 0; i <= units.Count - 1; i++) {
                NestedFadeGroup.NestedFadeGroup fade = units[i].GetComponent<NestedFadeGroup.NestedFadeGroup>();
                units[i].transform.position = to[i] + new Vector2(0, floorOffset*2);
                fade.AlphaSelf = 0;
            }
            
// Enter new cavity
            timer = 0;
            float scroll;
            startPos = new Vector3(0, -15, 0);
            Vector3 endPos = floorParent.transform.position;
            floorParent.transform.position = startPos;

            GenerateFloor(null, true);
            scenario.player.transform.parent = currentFloor.transform;
            foreach (Unit u in scenario.player.units)
                u.transform.parent = currentFloor.transform;
            GenerateFloor();
            floorCount = 1;
            UpdateFloorCounter(floorSequence.activePacket.packetLength);

            while (timer <= 0.5f) {
                scroll = Mathf.Lerp(-1, 0, dropCurve.Evaluate(timer/0.5f));
                floorParent.transform.position = Vector3.Lerp(startPos, endPos, dropCurve.Evaluate(timer/0.5f));
                parallax.ScrollParallax(Time.deltaTime * scroll);
                timer += Time.deltaTime;
                yield return null;
            }
            floorParent.transform.position = endPos;
            
            uiManager.ToggleBattleCanvas(true);


            descending = false;
            yield return scenario.StartCoroutine(scenario.FirstTurn());
        } else {
            StartCoroutine(scenario.Win());
            while (scenario.scenario == ScenarioManager.Scenario.EndState) {
                yield return null;
            }
        }
    }

    IEnumerator FloatingUnits() {
        float timer = 0;
        while(cavityWait) {
            yield return null;
            parallax.ScrollParallax(Time.deltaTime * -1);
            transitionParent.transform.position = new Vector3(0, Mathf.Sin(timer), 0);

            timer += Time.deltaTime;
        }
    }

    public IEnumerator FinalDescent() {
        scenario.player.StartEndTurn(false);
        yield return StartCoroutine(scenario.gpOptional.BossSlain());
        yield return StartCoroutine(DropNail(scenario.player.nail));
        descending = true;
        currentFloor.DisableGridHighlight();
        currentFloor.LockGrid(true);

        currentFloor.StartCoroutine(currentFloor.ShockwaveCollapse(scenario.player.nail.coord));
        floors[currentFloor.transform.GetSiblingIndex()+1].StartCoroutine(floors[currentFloor.transform.GetSiblingIndex()+1].ShockwaveCollapse(scenario.player.nail.coord));
        yield return new WaitForSecondsRealtime(1f);
        Coroutine co = StartCoroutine(TransitionPackets());
        floorSequence.currentThreshold = FloorChunk.PacketType.BARRIER;
        yield return co;
        //yield return StartCoroutine(TransitionFloors(true, false));
    }

    public IEnumerator EndSequenceAnimation(GameObject arm) {
// Local params for animation
        float timer = 0;
        Vector3 from = floorParent.transform.position;
        Vector3 to = new(from.x, from.y - floorOffset * 5, from.z);
        Vector3 fromScale = new();
        if (currentFloor)
            fromScale = currentFloor.transform.localScale;
        Vector3 toScale = Vector3.one * 0.75f;
        if (!currentFloor)
            scenario.player.transform.parent = floorParent;

        for (int i = transitionParent.childCount - 1; i >= 0; i--) {
            if (transitionParent.GetChild(i).gameObject == arm) continue;
            GridElement ge = transitionParent.GetChild(i).GetComponent<GridElement>();
            if (ge is Nail) continue;
            
            transitionParent.GetChild(i).SetParent(currentFloor != null ? currentFloor.transform : floorParent);
        }

        while (timer < transitionDur) {
            floorParent.transform.position = Vector3.Lerp(from, from - new Vector3(0, floorOffset), timer/transitionDur);
            if (currentFloor) 
                currentFloor.transform.localScale = Vector3.Lerp(fromScale, toScale, timer/transitionDur);

            parallax.ScrollParallax(Time.deltaTime * 1);
            yield return null;
            timer += Time.deltaTime;
        }
        if (currentFloor)
            currentFloor.GetComponent<SortingGroup>().sortingOrder = -1;
        while (timer < transitionDur * 5) {
            floorParent.transform.position = Vector3.Lerp(from, to, timer/(transitionDur*5));
            if (currentFloor)
                currentFloor.transform.localScale = Vector3.Lerp(fromScale, toScale, timer/(transitionDur*5));

            parallax.ScrollParallax(Time.deltaTime * 1);
            yield return null;
            timer += Time.deltaTime;
        }
        if (floating != null)
            StopCoroutine(floating);
        timer = 0;
        Vector3 prevPos = scenario.player.nail.transform.position;
        while (timer < transitionDur * 10) {
            scenario.player.nail.transform.localPosition = Vector3.Lerp(prevPos, Vector3.zero, timer/(transitionDur*10));
            arm.transform.localPosition = Vector3.Lerp(prevPos, Vector3.zero, timer/(transitionDur*10));
            
            transitionParent.transform.position = Vector3.Lerp(Vector3.zero, new Vector3(6, 0, 0), timer/(transitionDur*10));
            parallax.ScrollParallax(Time.deltaTime * 1);

            yield return null;
            timer += Time.deltaTime;
        }
        transitionParent.transform.position = new Vector3(6, 0, 0);
        scenario.player.nail.transform.localPosition = Vector3.zero;
        arm.transform.localPosition = Vector3.zero;
        while (scenario.scenario == ScenarioManager.Scenario.EndState) {
            if (parallax)
                parallax.ScrollParallax(Time.deltaTime * 1);
            yield return null;
        }

        from = transitionParent.transform.position;
        to = from + new Vector3(0, floorOffset * 5);

        timer = 0;
        scenario.runDataTracker.panel.SetActive(false);
        while (timer < transitionDur * 10) {
            transitionParent.transform.position = Vector3.Lerp(from, to, timer/(transitionDur*10));

            yield return null;
            timer += Time.deltaTime;
        }
    }

}
