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
    [SerializeField] GameObject floorPrefab;
    [SerializeField] Transform floorParent;
    [SerializeField] public List<FloorDefinition> floorDefinitions;
    public bool gridHightlightOverride;
    public Grid currentFloor;
    public List<Grid> floors;

    [SerializeField] int _gridSize;
    public static int gridSize;
    [SerializeField] float _sqrSize;
    public static float sqrSize;


    [Header("Floor Transitioning")]
    [HideInInspector] public BetweenFloorManager betweenFloor;
    public Transform transitionParent;
    public float floorOffset, transitionDur, unitDropDur;
    public AnimationCurve dropCurve;
    [HideInInspector] public bool transitioning, peeking;
    [SerializeField] public GameObject upButton, downButton;
    [SerializeField] ParallaxImageScroll parallax;
    public DescentPreviewManager previewManager;


    public List<Vector2> nailSpawnOverrides = new List<Vector2>(); // MOVE TO GRID CLASS
    public List<Vector2> playerDropOverrides = new List<Vector2>();

    
    [Header("Grid Viz")]
    public Color playerColor;
    public Color enemyColor, equipmentColor, movePreview, enemyMovePreview;
    private bool notation = false;


    #region Singleton (and Awake)

    public static FloorManager instance;
    private void Awake() {
        if (FloorManager.instance) {
            Debug.Log("Warning! More than one instance of Grid found!");
            return;
        }
        FloorManager.instance = this;
        gridSize = _gridSize; sqrSize = _sqrSize;
    }
    #endregion

    void Start() {
        if (ScenarioManager.instance) scenario = ScenarioManager.instance;
        if (UIManager.instance) uiManager = UIManager.instance;
        betweenFloor = GetComponent<BetweenFloorManager>();
    }

    public IEnumerator GenerateFloor(GameObject floorOverride = null, GameObject enemyOverride = null) {
        int index = floors.Count;

        Grid newFloor = Instantiate(floorOverride == null ? floorPrefab : floorOverride, this.transform).GetComponent<Grid>();
        currentFloor = newFloor;
        FloorDefinition floorDef = floorDefinitions[index];
        newFloor.lvlDef = floorDef;
    
        Coroutine co = StartCoroutine(newFloor.GenerateGrid(index, enemyOverride));
        yield return co;
        newFloor.ToggleChessNotation(notation);
        newFloor.overrideHighlight = gridHightlightOverride;
    
        newFloor.gameObject.name = "Floor" + newFloor.index;
        newFloor.transform.SetParent(floorParent);
        floors.Add(newFloor);

    }

    public IEnumerator GenerateNextFloor(GameObject floorPrefab = null, GameObject enemyPrefab = null) {
// Check if player wins
        if (currentFloor.index >= floorDefinitions.Count - 2) {

            StartCoroutine(scenario.Win());

        } else {

            yield return StartCoroutine(TransitionFloors(true, false));

            yield return StartCoroutine(GenerateFloor(floorPrefab, enemyPrefab));
            previewManager.UpdateFloors(floors[currentFloor.index]);

            yield return new WaitForSeconds(0.5f);
            if (TutorialSequence.instance)
                yield return (TutorialSequence.instance.GetComponent<LaterTutorials>().CheckFloorDef(floors[currentFloor.index].lvlDef));

            yield return StartCoroutine(previewManager.PreviewFloor(false, false));
        }
    }

    
    public Color GetFloorColor(int i) {
        Color c = equipmentColor;
        
        switch (i) {
            case 0:
            c = playerColor;
            break;
            case 1:
            c = enemyColor;
            break;
            case 2:
            c = equipmentColor;
             break;
            case 3:
            c = movePreview;
            break;
            case 4:
            c = enemyMovePreview;
            break;
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
        Grid toFloor = null; // After a descent the next floor hasn't generated before panning to it

// Block player from selecting units
        scenario.player.ToggleUnitSelectability(dir == -1);

        if (floors.Count - 1 >= currentFloor.index + dir) // Checks if there is a floor in the direction transitioning
            toFloor = floors[currentFloor.index + dir];
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
        float partialFrom = scenario.player.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf;
        float partialA = down? 0 : 1;

        peeking = down && preview;
        HideUnits(down);

// And the actual animation. Has become cluttered with NestedFadeGroup logic too.
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
            uiManager.metaDisplay.UpdateCurrentFloor(currentFloor.index);

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

    public void Descend(bool cascade = false, bool tut = false) {
        StartCoroutine(DescendFloors(cascade, tut));
        
    }

    public IEnumerator DescendFloors(bool cascade = false, bool tut = false) {
// Lock up current floor
        if (uiManager.gameObject.activeSelf)
            uiManager.LockFloorButtons(true);
        EnemyManager enemy = (EnemyManager)currentFloor.enemy;
        
        currentFloor.DisableGridHighlight();
        currentFloor.LockGrid(true);
        yield return StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Descent));

// Pan to next floor        
        // if (cascade) {
        //     yield return StartCoroutine(previewManager.PreviewFloor(true, true));
        //     currentFloor.LockGrid(false);
        // }
        // else
        yield return StartCoroutine(TransitionFloors(true, false));

        scenario.player.nail.ToggleNailState(Nail.NailState.Primed);

// Yield for Slots sequence
        yield return new WaitForSecondsRealtime(0.25f);
        if (betweenFloor.InbetweenTrigger(currentFloor.index-1)) {
            yield return StartCoroutine(betweenFloor.BetweenFloorSegment(currentFloor.index-1));
            
            yield return new WaitForSecondsRealtime(0.25f);
        }

// Yield for cascade sequence
        if (cascade) {
            //yield return StartCoroutine(ChooseLandingPositions());
            //yield return new WaitForSecondsRealtime(1.25f);
            downButton.SetActive(true); upButton.SetActive(false);
        }

// Descend units from previous floor
        yield return StartCoroutine(DescendUnits(floors[currentFloor.index -1].gridElements, enemy));

// Tutorial bloat
        if (tut) {
            if (currentFloor.index < 2)
                yield return StartCoroutine(GenerateNextFloor(TutorialSequence.instance.tutorialFloorPrefab, TutorialSequence.instance.tutorialEnemyPrefab));
            else
                yield return StartCoroutine(GenerateNextFloor());
            yield return new WaitForSeconds(0.5f);
            scenario.currentTurn = ScenarioManager.Turn.Null;
        } else {
            yield return StartCoroutine(GenerateNextFloor());
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Enemy));
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

        yield return new WaitForSecondsRealtime(1.5f);
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
                if (units[i] is not Nail) {
                    if (units[i] is PlayerUnit && playerDropOverrides.Count > 0) {
                        units[i].coord = playerDropOverrides[0];
                        playerDropOverrides.RemoveAt(0);
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
        
        yield return StartCoroutine(DropNail(nail));
    }

// Coroutine that houses the logic to descend a single unit
    public IEnumerator DropUnit(Unit unit, Vector3 from, Vector3 to, GridElement subElement = null) {
        float timer = 0;
        NestedFadeGroup.NestedFadeGroup fade = unit.GetComponent<NestedFadeGroup.NestedFadeGroup>();
        while (timer <= unitDropDur) {
            unit.transform.position = Vector3.Lerp(from, to, dropCurve.Evaluate(timer/unitDropDur));
            fade.AlphaSelf = Mathf.Lerp(0, 1, timer/(unitDropDur/3));
            yield return null;
            timer += Time.deltaTime;
        }
        unit.DescentVFX(currentFloor.sqrs.Find(sqr => sqr.coord == unit.coord), subElement);
        unit.transform.position = to;
        unit.StoreInGrid(currentFloor);
        fade.AlphaSelf = 1;

        unit.PlaySound(unit.landingSFX);

        if (subElement) {
            StartCoroutine(subElement.CollideFromBelow(unit));
            if (subElement is not GroundElement)
                yield return StartCoroutine(unit.CollideFromAbove(subElement));
        }
    }

// Coroutine for descending the nail at a regulated random position
    public IEnumerator DropNail(Nail nail) {
        if (nail.nailState == Nail.NailState.Buried)
            nail.ToggleNailState(Nail.NailState.Primed);

        bool validCoord = false;
        Vector2 spawn = Vector2.zero;

// Find a valid coord that a player unit is not in
        while (!validCoord) {
            validCoord = true;
            if (nailSpawnOverrides.Count > 0) {
                int i = Random.Range(0,nailSpawnOverrides.Count);
                spawn = nailSpawnOverrides[i];
                nailSpawnOverrides.RemoveAt(i);
            }
            else
                spawn = new Vector2(Random.Range(1,6), Random.Range(1,6));
                
            foreach(Unit u in scenario.player.units) {
                if (u.coord == spawn) validCoord = false;
            }

            if (currentFloor.sqrs.Find(sqr => sqr.coord == spawn).tileType == GridSquare.TileType.Bile) validCoord = false;
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
        while (timer <= unitDropDur) {
            nail.transform.localPosition = Vector3.Lerp(from, to, dropCurve.Evaluate(timer/unitDropDur));
            fade.AlphaSelf = Mathf.Lerp(0, 1, timer/(unitDropDur/3));
            yield return null;
            timer += Time.deltaTime;
        }
        nail.DescentVFX(currentFloor.sqrs.Find(sqr => sqr.coord == nail.coord), subElement);
        nail.transform.position = to;
        nail.StoreInGrid(currentFloor);
        nail.ToggleNailState(Nail.NailState.Buried);
        fade.AlphaSelf = 1;

        nail.PlaySound(nail.landingSFX);

        if (subElement) 
            StartCoroutine(subElement.CollideFromBelow(nail));
    }
}
