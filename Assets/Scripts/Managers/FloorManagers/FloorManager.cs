using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Linq;

// This class generates and sequences Floor prefabs (Grid class), as well as manages the descending of units

public class FloorManager : MonoBehaviour {

    UIManager uiManager;
    ScenarioManager scenario;
    
    [Header("Floor Serialization")]
    [SerializeField] public GameObject floorPrefab;
    [SerializeField] Transform floorParent;
    [SerializeField] public FloorSequence floorSequence;
    public TutorialSequence tutorial;
    public bool gridHightlightOverride;
    public Grid currentFloor;
    int currentGridIndex;
    public List<Grid> floors;
    [HideInInspector] public bool bossSpawn = false;

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
    bool cavityWait = false;
    [SerializeField] ParallaxImageScroll parallax;
    public DescentPreviewManager previewManager;
    [SerializeField] Animator cavityText;

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

        floorSequence.Init(startIndex);
        if (startIndex == 0) {
            tutorial.gameObject.SetActive(true);
            tutorial.Initialize(scenario);
            previewManager.tut = true;
        } else {
            tutorial.gameObject.SetActive(false);
        }
        floorSequence.StartPacket(floorSequence.currentThreshold);
        
        yield return null;
    }

    public void GenerateFloor(FloorDefinition definitionOverride = null, bool first = false) {
        int index = floors.Count;

        Grid newFloor = Instantiate(floorPrefab, floorParent).GetComponent<Grid>();
        newFloor.transform.localPosition = new Vector3(0, index * -floorOffset);
        
        FloorDefinition floorDef;
        if (definitionOverride)
            floorDef = definitionOverride;
        else 
            floorDef = floorSequence.GetFloor();

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
            previewManager.UpdateFloors(floors[newFloor.index - 1], newFloor);            
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

        if (floors.Count - 1 >= currentFloor.index + dir) // Checks if there is a floor in the direction transitioning
            toFloor = floors[currentFloor.index + dir];
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
                toFloor.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = Mathf.Lerp(0, 1, timer/transitionDur);
            }
            
// Fade previewed GridElements like player units
            currentFloor.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = Mathf.Lerp(currFromA, currToA, timer/transitionDur);
            
            if (parallax)
                parallax.ScrollParallax(down ? -1 : 1);
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
        if (down && currentFloor.index-1 >= 0) floors[currentFloor.index-1].gameObject.SetActive(false);
        if (toFloor) currentFloor = toFloor;           
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
            foreach (Unit u in scenario.player.units) 
                u.elementCanvas.ToggleStatsDisplay(targetedDict[u]);
            foreach (Unit u in scenario.currentEnemy.units) 
                u.elementCanvas.ToggleStatsDisplay(targetedDict[u]);
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
        previewManager.UpdateFloors(currentFloor, floors[currentFloor.index + 1]);            
        currentFloor.DisableGridHighlight();

    }

    public void Descend(bool cascade = false, bool nail = true, Vector2 pos = default) {
        bool tut = floorSequence.activePacket.packetType == FloorPacket.PacketType.Tutorial;
        Debug.Log("Tutorial Descent: " + tut);
        StartCoroutine(DescendFloors(cascade, tut, nail, pos));
        
    }

    public IEnumerator DescendFloors(bool cascade = false, bool tut = false, bool nail = true, Vector2 pos = default) {
// Lock up current floor
        descending = true;
        
        EnemyManager enemy = (EnemyManager)currentFloor.enemy;
        enemy.InterruptReinforcements();
        List<Unit> enemyUnits = new();
        foreach (GridElement ge in currentFloor.gridElements) {
            if (ge is EnemyUnit u)
                enemyUnits.Add(u);
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
        if (floorSequence.currentThreshold == FloorPacket.PacketType.BOSS) scen = ScenarioManager.Scenario.Boss;
        if (tut) {
            yield return StartCoroutine(TutorialSequence.instance.TutorialDescend());
        } else {

            // if (cascade) {
            //     yield return StartCoroutine(previewManager.PreviewFloor(true, true));
            //     currentFloor.LockGrid(false);
            // }
            // else

// Check if at the end of packet / if there was a sub floor generated, if not packet is done
            if (floors.Count - 1 > currentFloor.index) {
// Generate next floor if still mid packet
                if (!floorSequence.ThresholdCheck()) { //|| floorSequence.currentThreshold == FloorPacket.PacketType.BOSS
                    GenerateFloor();       
                }

                scenario.player.nail.ToggleNailState(Nail.NailState.Falling);   
                yield return StartCoroutine(TransitionFloors(true, false));
                
                yield return StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Descent, scen));
                yield return new WaitForSecondsRealtime(0.25f);
// Check for tutorial tooltip triggers
                if (floorSequence.activePacket.packetType != FloorPacket.PacketType.Tutorial) {
                    if (currentFloor.lvlDef.initSpawns.Find(spawn => spawn.asset.prefab.GetComponent<GridElement>() is TileBulb) != null && !scenario.gpOptional.bulbEncountered)
                        scenario.gpOptional.StartCoroutine(scenario.gpOptional.TileBulb());
                    if (currentFloor.lvlDef.initSpawns.Find(spawn => spawn.asset.prefab.GetComponent<GridElement>() is EnemyDetonateUnit) != null && !scenario.gpOptional.basophicEncountered)
                        scenario.gpOptional.StartCoroutine(scenario.gpOptional.Basophic());
                    // if (floorSequence.currentThreshold == FloorPacket.PacketType.BOSS && !scenario.gpOptional.prebossEncountered) { 
                    //     scenario.gpOptional.StartCoroutine(scenario.gpOptional.Preboss());
                    // }
                }

// Yield for cascade sequence
                // if (cascade) {
                //     yield return StartCoroutine(ChooseLandingPositions());
                //     yield return new WaitForSecondsRealtime(1.25f);
                //     downButton.SetActive(true); upButton.SetActive(false);
                // }
                
// Descend units from previous floor
                yield return StartCoroutine(DescendUnits(floors[currentFloor.index -1].gridElements, enemy));
                 
                StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Enemy));
            } else {
    // NODE LOGIC
                yield return StartCoroutine(TransitionPackets((EnemyManager)currentFloor.enemy));
            }
        }
        FloorDescended?.Invoke();

        descending = false;
    }

    void UpdateFloorCounter() {
        int cavityOffset = -3;
        if (scenario.startCavity >= 1) cavityOffset += 3;
        if (scenario.startCavity >= 2) cavityOffset += 3;
        if (scenario.startCavity >= 3) cavityOffset += 4;
        uiManager.metaDisplay.UpdateCurrentFloor(currentFloor.index + 1 + cavityOffset);
    }

    public IEnumerator DescendUnits(List<GridElement> units, EnemyManager enemy = null, bool hardLand = false) {
        UpdateFloorCounter();
        Coroutine drop = StartCoroutine(DropUnits(units, hardLand));

        scenario.currentEnemy = (EnemyManager)currentFloor.enemy;
        
        yield return drop;

        if (enemy) {
            enemy.SeedUnits(currentFloor, enemy == currentFloor.enemy);
        }
        scenario.player.DescendGrids(currentFloor);
        currentFloor.LockGrid(false);


        if (uiManager.gameObject.activeSelf)    
            uiManager.metaDisplay.UpdateEnemiesRemaining(scenario.currentEnemy.units.Count);
    }

// Coroutine that sequences the descent of all valid units
    public IEnumerator DropUnits(List<GridElement> units, bool hardLand) {

        scenario.player.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = 1;
        scenario.player.transform.parent = transitionParent;
        scenario.player.nail.transform.parent = currentFloor.transform;
        if (scenario.currentEnemy) {
            scenario.currentEnemy.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = 1;
            scenario.currentEnemy.transform.parent = transitionParent;
        }

        DescendingUnits?.Invoke();

        foreach (GridElement ge in units) {
            if (ge is Unit u) {
                u.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = 0;
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
        
        if (nail && floorSequence.currentThreshold != FloorPacket.PacketType.BOSS) {
            yield return StartCoroutine(DropNail(nail));
            //yield return StartCoroutine(DropParticle());
        }
    }

// Coroutine that houses the logic to descend a single unit
    public IEnumerator DropUnit(Unit unit, Vector3 from, Vector3 to, GridElement subElement = null, bool hardLand = false) {
        float timer = 0;
        NestedFadeGroup.NestedFadeGroup fade = unit.GetComponent<NestedFadeGroup.NestedFadeGroup>();
        unit.airTraillVFX.SetActive(true);
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
        unit.DescentVFX(currentFloor.tiles.Find(sqr => sqr.coord == unit.coord), subElement);
        unit.transform.position = to;
        unit.StoreInGrid(currentFloor);
        unit.UpdateElement(unit.coord);
        fade.AlphaSelf = 1;

        unit.PlaySound(unit.landingSFX);

        if (subElement) {
            StartCoroutine(subElement.CollideFromBelow(unit));
            yield return StartCoroutine(unit.CollideFromAbove(subElement, hardLand?1:0));
        } else if (hardLand && currentFloor.tiles.Find(t => t.coord == unit.coord).tileType != Tile.TileType.Bile) {
            yield return StartCoroutine(unit.TakeDamage(1, GridElement.DamageType.Fall));
        }

    }

// Coroutine for descending the nail at a regulated random position
    public IEnumerator DropNail(Nail nail) {
        nail.ToggleNailState(Nail.NailState.Falling);

        bool validCoord = false;
        Vector2 spawn = Vector2.zero;

// Find a valid coord that a player unit is not in
        while (!validCoord) {
            validCoord = true;
            if (currentFloor.nailSpawns.Count > 0) {
                int i = UnityEngine.Random.Range(0, currentFloor.nailSpawns.Count);
                spawn = currentFloor.nailSpawns[i];
                currentFloor.nailSpawns.RemoveAt(i);
            }
            else
                spawn = new Vector2(UnityEngine.Random.Range(1,6), UnityEngine.Random.Range(1,6));
                
            foreach(Unit u in scenario.player.units) {
                if (u.coord == spawn && u is not Nail) validCoord = false;
            }

            if (currentFloor.tiles.Find(sqr => sqr.coord == spawn).tileType == Tile.TileType.Bile) validCoord = false;
            if (currentFloor.tiles.Find(sqr => sqr.coord == spawn) is TileBulb) validCoord = false;
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

        if (subElement) 
            StartCoroutine(subElement.CollideFromBelow(nail));
    }

    public IEnumerator DropParticle() {

        GodParticleGE particle = Instantiate(godParticlePrefab).GetComponent<GodParticleGE>();
        particle.Init();
        particle.transform.parent = currentFloor.neutralGEContainer.transform;

        particle.StoreInGrid(currentFloor);
        
        bool validCoord = false;
        Vector2 spawn = Vector2.zero;

// Find a valid coord that a player unit is not in
        while (!validCoord) {
            validCoord = true;
            spawn = new Vector2(UnityEngine.Random.Range(1,6), UnityEngine.Random.Range(1,6));
                
            foreach(Unit u in scenario.player.units) {
                if (u.coord == spawn) validCoord = false;
            }

            if (currentFloor.tiles.Find(sqr => sqr.coord == spawn).tileType == Tile.TileType.Bile) validCoord = false;
            if (currentFloor.tiles.Find(sqr => sqr.coord == spawn) is TileBulb) validCoord = false;
        }
        
        particle.transform.position = particle.grid.PosFromCoord(spawn);
        particle.UpdateSortOrder(spawn);
        particle.coord = spawn;

        GridElement subElement = null;
        foreach (GridElement ge in currentFloor.CoordContents(particle.coord)) subElement = ge;

        Vector3 to = currentFloor.PosFromCoord(spawn);
        Vector3 from = to + new Vector3(0, floorOffset*2, 0);
        float timer = 0;
        NestedFadeGroup.NestedFadeGroup fade = particle.GetComponent<NestedFadeGroup.NestedFadeGroup>();
        //particle.airTraillVFX.SetActive(true);

        while (timer <= unitDropDur) {
            particle.transform.localPosition = Vector3.Lerp(from, to, dropCurve.Evaluate(timer/unitDropDur));
            fade.AlphaSelf = Mathf.Lerp(0, 1, timer/(unitDropDur/3));
            yield return null;
            timer += Time.deltaTime;
        }

        //particle.DescentVFX(currentFloor.tiles.Find(sqr => sqr.coord == particle.coord), subElement);
        particle.transform.position = to;
        particle.StoreInGrid(currentFloor);
        fade.AlphaSelf = 1;
    }

    public IEnumerator SpawnBoss() {
        bossSpawn = true;
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
    
        Unit boss = scenario.currentEnemy.SpawnBossUnit(spawn, floorSequence.bossPrefab.GetComponent<Unit>());
        Vector3 to = currentFloor.PosFromCoord(spawn);
        Vector3 from = to + new Vector3(0, floorOffset*2, 0);

        float timer = 0;
        NestedFadeGroup.NestedFadeGroup fade = boss.GetComponent<NestedFadeGroup.NestedFadeGroup>();
        boss.airTraillVFX.SetActive(true);
        while (timer <= unitDropDur) {
            boss.transform.localPosition = Vector3.Lerp(from, to, dropCurve.Evaluate(timer/unitDropDur));
            fade.AlphaSelf = Mathf.Lerp(0, 1, timer/(unitDropDur/3));
            yield return null;
            timer += Time.deltaTime;
        }
        boss.DescentVFX(currentFloor.tiles.Find(sqr => sqr.coord == boss.coord));
        boss.transform.position = to;
        boss.StoreInGrid(currentFloor);
        boss.UpdateElement(boss.coord);
        fade.AlphaSelf = 1;

        boss.PlaySound(boss.landingSFX);
        scenario.gpOptional.StartCoroutine(scenario.gpOptional.Boss());        
    }


    public IEnumerator TransitionPackets(EnemyManager lastFloorEnemey = null) {
        if (currentFloor) {
            yield return StartCoroutine(TransitionFloors(true, false));
            
            yield return StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Descent));
        }
        yield return new WaitForSecondsRealtime(0.25f);
// Destroy Anvils
        // List<Coroutine> cos = new();
        // for (int i = scenario.player.units.Count - 1; i >= 0; i-- ) {
        //     Unit u = scenario.player.units[i];
        //     if (u is not PlayerUnit && u is not Nail)
        //         cos.Add(StartCoroutine(u.DestroySequence()));
        // }
        // for (int i = cos.Count - 1; i >= 0; i--) {
        //     if (cos[i] != null) 
        //         yield return cos[i];
        //     else
        //         cos.RemoveAt(i);
        // }
        
// Lerp units into screen
        List<Unit> units = new() { scenario.player.units[0], scenario.player.units[1], scenario.player.units[2], scenario.player.units[3] };
        List<Vector2> to = new() {new Vector2(-1.182819f, 0.5243183f), new Vector2(-2.862819f, 0.04704558f), new Vector2(-0.5108191f, -0.5781816f), new Vector2(1.841181f, -1.203409f) };
        units[0].manager.transform.parent = transitionParent;
        units[3].transform.parent = transitionParent;
        scenario.player.nail.ToggleNailState(Nail.NailState.Falling);   
        float timer = 0;
        while (timer <= unitDropDur) {
            parallax.ScrollParallax(-1);
            for (int i = 0; i <= units.Count - 1; i++) {
                NestedFadeGroup.NestedFadeGroup fade = units[i].GetComponent<NestedFadeGroup.NestedFadeGroup>();
                units[i].transform.position = Vector3.Lerp(to[i] + new Vector2(0, floorOffset*2), to[i], dropCurve.Evaluate(timer/unitDropDur));
                fade.AlphaSelf = Mathf.Lerp(0, 1, timer/(unitDropDur/3));
            }
            yield return null;
            timer += Time.deltaTime;
        }
        
// Endlessly falling
        cavityWait = true;
        Coroutine floating = StartCoroutine(FloatingUnits());


// Objective award + Upgrade sequence
        if (floorSequence.activePacket.packetType != FloorPacket.PacketType.BOSS) {
            uiManager.ToggleBattleCanvas(false);
            if (floorSequence.activePacket.packetType != FloorPacket.PacketType.Tutorial && currentFloor != null) {
                yield return scenario.objectiveManager.RewardSequence();
                yield return scenario.player.upgradeManager.StartCoroutine(scenario.player.upgradeManager.UpgradeSequence());
            }

            yield return scenario.objectiveManager.AssignSequence();
        }
        

        if (floorSequence.activePacket.packetType != FloorPacket.PacketType.BARRIER) {
            StopCoroutine(floating);
            timer = 0;
            Vector3 startPos = transitionParent.transform.position;
            while (timer <= unitDropDur*2) {
                parallax.ScrollParallax(-1);
                transitionParent.transform.position = Vector3.Lerp(startPos, Vector3.zero, timer/unitDropDur);
                timer += Time.deltaTime;
                yield return null;
            }

            transitionParent.transform.position = Vector3.zero;
            cavityWait = false;

            timer = 0;
            while (timer <= unitDropDur) {
                parallax.ScrollParallax(-1);
                for (int i = 0; i <= units.Count - 1; i++) {
                    NestedFadeGroup.NestedFadeGroup fade = units[i].GetComponent<NestedFadeGroup.NestedFadeGroup>();
                    units[i].transform.position = Vector3.Lerp(to[i], to[i] + new Vector2(0, floorOffset*2), dropCurve.Evaluate(timer/unitDropDur));
                    fade.AlphaSelf = Mathf.Lerp(1, 0, (timer - (unitDropDur*2/3))/(unitDropDur/3));
                }
                yield return null;
                timer += Time.deltaTime;
            }
            
            // units[0].manager.transform.parent = floors[currentFloor.index - 1].transform;
            // units[3].transform.parent = currentFloor.transform;

            timer = 0;
            while (timer <= 0.5f) {
                parallax.ScrollParallax(-1);
                timer += Time.deltaTime;
            }
            uiManager.ToggleBattleCanvas(true);
            
            GenerateFloor(null, true);
            scenario.player.transform.parent = currentFloor.transform;
            GenerateFloor();
            descending = false;
            yield return scenario.StartCoroutine(scenario.FirstTurn(lastFloorEnemey));
        } else {
            yield return StartCoroutine(scenario.Win());
            while (scenario.scenario == ScenarioManager.Scenario.EndState) {
                if (parallax)
                    parallax.ScrollParallax(-1);
                yield return null;
            }
        }
    }

    IEnumerator FloatingUnits() {
        float timer = 0;
        while(cavityWait) {
            yield return null;
            parallax.ScrollParallax(-1);
            transitionParent.transform.position = new Vector3(0, Mathf.Sin(timer), 0);

            timer += Time.deltaTime;
        }
    }

    public IEnumerator FinalDescent() {
        descending = true;
        currentFloor.DisableGridHighlight();
        currentFloor.LockGrid(true);

        currentFloor.StartCoroutine(currentFloor.ShockwaveCollapse(scenario.player.nail.coord));
        floors[currentFloor.index++].StartCoroutine(floors[currentFloor.index++].ShockwaveCollapse(scenario.player.nail.coord));
        yield return new WaitForSecondsRealtime(1f);
        Coroutine co = StartCoroutine(TransitionPackets());
        floorSequence.currentThreshold = FloorPacket.PacketType.BARRIER;
        yield return co;
        //yield return StartCoroutine(TransitionFloors(true, false));
    }

    public IEnumerator EndSequenceAnimation(GameObject arm) {

        // Local params for animation
        Vector3 from = floorParent.transform.position;
        Vector3 to = new(from.x, from.y - floorOffset * 5, from.z);
        Vector3 fromScale = currentFloor.transform.localScale;
        Vector3 toScale = Vector3.one * 0.75f;

        float timer = 0;
        while (timer < transitionDur) {
            floorParent.transform.position = Vector3.Lerp(from, from - new Vector3(0, floorOffset), timer/transitionDur);
            currentFloor.transform.localScale = Vector3.Lerp(fromScale, toScale, timer/transitionDur);

            parallax.ScrollParallax(1);
            yield return null;
            timer += Time.deltaTime;
        }
        currentFloor.GetComponent<SortingGroup>().sortingOrder = -1;
        while (timer < transitionDur * 5) {
            floorParent.transform.position = Vector3.Lerp(from, to, timer/(transitionDur*5));
            currentFloor.transform.localScale = Vector3.Lerp(fromScale, toScale, timer/(transitionDur*5));

            parallax.ScrollParallax(1);
            yield return null;
            timer += Time.deltaTime;
        }
        
        timer = 0;
        Vector3 prevPos = scenario.player.nail.transform.position;
        while (timer < transitionDur * 10) {
            scenario.player.nail.transform.localPosition = Vector3.Lerp(prevPos, Vector3.zero, timer/(transitionDur*10));
            arm.transform.localPosition = Vector3.Lerp(prevPos, Vector3.zero, timer/(transitionDur*10));
            
            transitionParent.transform.position = Vector3.Lerp(Vector3.zero, new Vector3(6, 0, 0), timer/(transitionDur*10));
            parallax.ScrollParallax(1);

            yield return null;
            timer += Time.deltaTime;
        }
        scenario.player.nail.transform.localPosition = Vector3.zero;
        arm.transform.localPosition = Vector3.zero;
        
        while (scenario.scenario == ScenarioManager.Scenario.EndState) {
            if (parallax)
                parallax.ScrollParallax(1);
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
