using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

// This class generates and sequences Floor prefabs (Grid class), as well as manages the descending of units

[RequireComponent(typeof(BetweenFloorManager))]
public class FloorManager : MonoBehaviour
{

    UIManager uiManager;
    ScenarioManager scenario;
    
    [Header("Floor Serialization")]
    [SerializeField] public GameObject floorPrefab;
    [SerializeField] Transform floorParent;
    [SerializeField] public FloorSequence floorSequence;
    public bool gridHightlightOverride;
    public Grid currentFloor;
    int currentGridIndex;
    public List<Grid> floors;
    [HideInInspector] public bool bossSpawn = false;

    [SerializeField] int _gridSize;
    public static int gridSize;
    [SerializeField] float _sqrSize;
    public static float sqrSize;


    [Header("Floor Transitioning")]
    [HideInInspector] public BetweenFloorManager betweenFloor;
    public Transform transitionParent;
    public float floorOffset, transitionDur, unitDropDur;
    public AnimationCurve dropCurve;
    public bool transitioning, peeking;
    [SerializeField] ParallaxImageScroll parallax;
    public DescentPreviewManager previewManager;
    [SerializeField] Animator cavityText;
   
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

    public IEnumerator Init() {
        if (ScenarioManager.instance) scenario = ScenarioManager.instance;
        if (UIManager.instance) uiManager = UIManager.instance;
        betweenFloor = GetComponent<BetweenFloorManager>();
        floorSequence.Init();
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
        Vector3 to = new Vector3(from.x, from.y + floorOffset * dir, from.z);
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
        if (uiManager.gameObject.activeSelf)
            uiManager.metaDisplay.UpdateCurrentFloor(currentFloor.index + 1 - (TutorialSequence.instance != null ? 3 : 0));

    }
    

    // Function that hides units when previewing the next floor -- MOVE TO UNIT FUNCTIONALITY
    Dictionary<Unit, bool> targetedDict = new Dictionary<Unit,bool>();
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
        yield return StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Cascade));
        while (scenario.currentTurn == ScenarioManager.Turn.Cascade)
            yield return null;
        //StartCoroutine(ToggleDescentPreview(false));
        currentFloor.DisableGridHighlight();

    }

    public void Descend(bool cascade = false, bool nail = true, Vector2 pos = default) {
        bool tut = floorSequence.activePacket.packetType == FloorPacket.PacketType.Tutorial;
        StartCoroutine(DescendFloors(cascade, tut, nail, pos));
        
    }

    public IEnumerator DescendFloors(bool cascade = false, bool tut = false, bool nail = true, Vector2 pos = default) {

// Lock up current floor
        if (uiManager.gameObject.activeSelf)
            uiManager.LockFloorButtons(true);
        EnemyManager enemy = (EnemyManager)currentFloor.enemy;
        
        currentFloor.DisableGridHighlight();
        currentFloor.LockGrid(true);

        if (nail)
            yield return StartCoroutine(currentFloor.ShockwaveCollapse(pos));

        ScenarioManager.Scenario scen = ScenarioManager.Scenario.Null;
        if (floorSequence.currentThreshold == FloorPacket.PacketType.BOSS) scen = ScenarioManager.Scenario.Boss;
        if (tut) {
            yield return StartCoroutine(TutorialSequence.instance.TutorialDescend());
        }
        else {

            // if (cascade) {
            //     yield return StartCoroutine(previewManager.PreviewFloor(true, true));
            //     currentFloor.LockGrid(false);
            // }
            // else

// Check if at the end of packet
            if (floors.Count - 1 > currentFloor.index) {
// Generate next floor if still mid packet
                if (!floorSequence.ThresholdCheck() || floorSequence.currentThreshold == FloorPacket.PacketType.BOSS) {
                    GenerateFloor(); 
                    
                }

                scenario.player.nail.ToggleNailState(Nail.NailState.Falling);   
                yield return StartCoroutine(TransitionFloors(true, false));
                
                yield return StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Descent, scen));
                yield return new WaitForSecondsRealtime(0.25f);

// Yield for cascade sequence
                // if (cascade) {
                //     yield return StartCoroutine(ChooseLandingPositions());
                //     yield return new WaitForSecondsRealtime(1.25f);
                //     downButton.SetActive(true); upButton.SetActive(false);
                // }
                
// Descend units from previous floor
                yield return StartCoroutine(DescendUnits(floors[currentFloor.index -1].gridElements, enemy));

// Check for boss spawn
                if (floorSequence.activePacket.packetType == FloorPacket.PacketType.BOSS && !bossSpawn) 
                    yield return StartCoroutine(SpawnBoss());

// Check for tutorial tooltip triggers
                if (scenario.tutorial != null && floorSequence.activePacket.packetType != FloorPacket.PacketType.Tutorial) {
                    if (currentFloor.lvlDef.initSpawns.Find(spawn => spawn.asset.prefab.GetComponent<GridElement>() is TileBulb) != null && !scenario.tutorial.bulbEncountered)
                        scenario.tutorial.StartCoroutine(scenario.tutorial.TileBulb());
                    if (currentFloor.lvlDef.initSpawns.Find(spawn => spawn.asset.prefab.GetComponent<GridElement>() is EnemyDetonateUnit) != null && !scenario.tutorial.basophicEncountered)
                        scenario.tutorial.StartCoroutine(scenario.tutorial.Basophic());
                }


                 
                StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Enemy));
            } else {
    // NODE LOGIC
                yield return StartCoroutine(TransitionPackets());
            }
        }
    }

    public IEnumerator DescendUnits(List<GridElement> units, EnemyManager enemy = null) {
        Coroutine drop = StartCoroutine(DropUnits(units, currentFloor));

        scenario.currentEnemy = (EnemyManager)currentFloor.enemy;
        yield return drop;

        if (enemy)
            enemy.SeedUnits(currentFloor);

        scenario.player.DescendGrids(currentFloor);
        currentFloor.LockGrid(false);

        if (uiManager.gameObject.activeSelf)    
            uiManager.metaDisplay.UpdateEnemiesRemaining(scenario.currentEnemy.units.Count);
    }

// Coroutine that sequences the descent of all valid units
    public IEnumerator DropUnits(List<GridElement> units, Grid toFloor) {

        scenario.player.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = 1;
        scenario.currentEnemy.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = 1;
        
        scenario.player.nail.transform.parent = currentFloor.transform;

        foreach (GridElement ge in units) {
            if (ge is Unit u) {
                u.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = 0;
                u.manager.transform.parent = transitionParent;
            }
        }
        Nail nail = null;
        List<Coroutine> descents = new List<Coroutine>();
        for (int i = units.Count - 1; i >= 0; i--) {
            if (units[i] is Unit u) {
                if (u is not Nail) {
                    if (u is PlayerUnit && toFloor.slagSpawns.Count > 0) {
                        u.coord = toFloor.slagSpawns[0];
                        toFloor.slagSpawns.RemoveAt(0);
                    } 
                    GridElement subElement = null;
                    foreach (GridElement ge in toFloor.CoordContents(u.coord)) subElement = ge;
                    descents.Add(StartCoroutine(DropUnit(u, toFloor.PosFromCoord(u.coord) + new Vector3 (0, floorOffset*2, 0), toFloor.PosFromCoord(u.coord), subElement)));
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
        
        if (nail)
            yield return StartCoroutine(DropNail(nail, toFloor));
    }

// Coroutine that houses the logic to descend a single unit
    public IEnumerator DropUnit(Unit unit, Vector3 from, Vector3 to, GridElement subElement = null) {
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

            yield return StartCoroutine(unit.CollideFromAbove(subElement));
        }
    }

// Coroutine for descending the nail at a regulated random position
    public IEnumerator DropNail(Nail nail, Grid toFloor) {
        nail.ToggleNailState(Nail.NailState.Falling);

        bool validCoord = false;
        Vector2 spawn = Vector2.zero;

// Find a valid coord that a player unit is not in
        while (!validCoord) {
            validCoord = true;
            if (toFloor.nailSpawns.Count > 0) {
                int i = Random.Range(0, toFloor.nailSpawns.Count);
                spawn = toFloor.nailSpawns[i];
                toFloor.nailSpawns.RemoveAt(i);
            }
            else
                spawn = new Vector2(Random.Range(1,6), Random.Range(1,6));
                
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

    public IEnumerator SpawnBoss() {
        bossSpawn = true;
        bool validCoord = false;
        Vector2 spawn = Vector2.zero;
        while (!validCoord) {
            validCoord = true;

            spawn = new Vector2(Random.Range(1,6), Random.Range(1,6));

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
        fade.AlphaSelf = 1;


    }


    public IEnumerator TransitionPackets() {
        yield return StartCoroutine(TransitionFloors(true, false));
        yield return StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Descent));
        yield return new WaitForSecondsRealtime(0.25f);
        List<Coroutine> cos = new();
        for (int i = scenario.player.units.Count - 1; i >= 0; i-- ) {
            Unit u = scenario.player.units[i];
            if (u is not PlayerUnit && u is not Nail)
                cos.Add(StartCoroutine(u.DestroyElement()));
        }
        
        for (int i = cos.Count - 1; i >= 0; i--) {
            if (cos[i] != null) 
                yield return cos[i];
            else
                cos.RemoveAt(i);
        }
        
        List<Unit> units = new List<Unit> { scenario.player.units[0], scenario.player.units[1], scenario.player.units[2], scenario.player.units[3] };
        List<Vector2> to = new List<Vector2> { currentFloor.PosFromCoord(new Vector2(3, 3)), currentFloor.PosFromCoord(new Vector2(4, 3)), currentFloor.PosFromCoord(new Vector2(3, 2)), currentFloor.PosFromCoord(new Vector2(5, 4)) };
        units[0].manager.transform.parent = transitionParent;
        units[3].transform.parent = transitionParent;
        scenario.player.nail.ToggleNailState(Nail.NailState.Falling);   
        float timer = 0;
        while (timer <= unitDropDur) {
            parallax.ScrollParallax(-1);
            for (int i = 0; i <= units.Count - 1; i++) {
                NestedFadeGroup.NestedFadeGroup fade = units[i].GetComponent<NestedFadeGroup.NestedFadeGroup>();
                units[i].transform.position = Vector3.Lerp(to[i] + new Vector2(0, floorOffset*2), to[i] - new Vector2(0, floorOffset), dropCurve.Evaluate(timer/unitDropDur));
                fade.AlphaSelf = Mathf.Lerp(0, 1, timer/(unitDropDur/3));
            }
            yield return null;
            timer += Time.deltaTime;
        }
        
        
        cavityText.gameObject.SetActive(true);
        cavityText.SetBool("Active", true);

        timer = 0;
        while(true) {
            yield return null;
            parallax.ScrollParallax(-1);
            transitionParent.transform.position = new Vector3(0, Mathf.Sin(timer), 0);

            timer += Time.deltaTime;
            if (Input.GetMouseButtonDown(0)) break;
        }

        cavityText.SetBool("Active", false);

        if (floorSequence.activePacket.packetType != FloorPacket.PacketType.BOSS) {
            timer = 0;
            while (timer <= unitDropDur) {
                parallax.ScrollParallax(-1);
                for (int i = 0; i <= units.Count - 1; i++) {
                    NestedFadeGroup.NestedFadeGroup fade = units[i].GetComponent<NestedFadeGroup.NestedFadeGroup>();
                    units[i].transform.position = Vector3.Lerp(to[i] - new Vector2(0, floorOffset), to[i] + new Vector2(0, floorOffset*2), dropCurve.Evaluate(timer/unitDropDur));
                    fade.AlphaSelf = Mathf.Lerp(1, 0, (timer - (unitDropDur*2/3))/(unitDropDur/3));
                    yield return null;
                    timer += Time.deltaTime;
                }
            }
            
            units[0].manager.transform.parent = floors[currentFloor.index - 1].transform;
            units[3].transform.parent = currentFloor.transform;

            timer = 0;
            while (timer <= 0.5f) {
                parallax.ScrollParallax(-1);
                timer += Time.deltaTime;
            }
            
            GenerateFloor(null, true);
            GenerateFloor();
            yield return scenario.StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Descent, ScenarioManager.Scenario.Combat));
            yield return StartCoroutine(DescendUnits(new List<GridElement>{ units[0], units[1], units[2], units[3]} ));
            yield return scenario.StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Enemy));
        } else {
            yield return StartCoroutine(scenario.Win());
            while (scenario.scenario == ScenarioManager.Scenario.EndState) {
                if (parallax)
                    parallax.ScrollParallax(1);
                yield return null;
            }
        }
    }

    public IEnumerator EndSequenceAnimation(GameObject arm) {

        // Local params for animation
        Vector3 from = floorParent.transform.position;
        Vector3 to = new Vector3(from.x, from.y - floorOffset * 5, from.z);
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
