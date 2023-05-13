using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

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
    [SerializeField] Transform transitionParent;
    public float floorOffset, transitionDur, unitDropDur;
    [HideInInspector] public bool transitioning, peeking;
    [SerializeField] public GameObject upButton, downButton;
    [SerializeField] ParallaxImageScroll parallax;
    
    [Header("Grid Viz")]
    public bool gridHighlights;
    public bool gridContextuals;
    [SerializeField] private GameObject descentPreview;
    [SerializeField] private Material previewMaterial;
    [SerializeField] private Dictionary<GridElement, LineRenderer> lineRenderers;
    public Color playerColor, enemyColor, equipmentColor;
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
        lineRenderers = new Dictionary<GridElement, LineRenderer>();
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
        }
        return c;
    }

    public IEnumerator PreviewFloor(bool down, bool draw) {
        transitioning = true;
        if (scenario.currentTurn == ScenarioManager.Turn.Player)
            uiManager.endTurnButton.enabled = !down;
        if (down) {
            if (draw) {
                StartCoroutine(ToggleDescentPreview(true));
                SetButtonActive(downButton, false); SetButtonActive(upButton, true);
                scenario.player.transform.parent = transitionParent;
                scenario.player.nail.transform.parent = currentFloor.transform;
                scenario.currentEnemy.transform.parent = transitionParent;
            }
            
            yield return StartCoroutine(TransitionFloors(down, true));
            transitioning = false;
        }
        else {
            StartCoroutine(ToggleDescentPreview(false));
            currentFloor.DisableGridHighlight();
            if (draw) {
                SetButtonActive(downButton, true); SetButtonActive(upButton, false);
            }
            yield return StartCoroutine(TransitionFloors(down, true));
            scenario.player.transform.parent = currentFloor.transform;
            scenario.player.nail.transform.parent = scenario.player.transform;
            scenario.currentEnemy.transform.parent = currentFloor.transform;
            
            transitioning = false;
        }
    }

    public IEnumerator CancelPreview() {
        while (transitioning) {
            yield return null;
        }
        if (peeking) {
            if (uiManager.gameObject.activeSelf)
                uiManager.PlaySound(uiManager.peekAboveSFX.Get());
            yield return PreviewFloor(false, true);
        }
        yield return new WaitForSecondsRealtime(0.125f);
    }

    public IEnumerator ToggleDescentPreview(bool active) {
        
        if (active) {
            lineRenderers = new Dictionary<GridElement, LineRenderer>();
            foreach (GridElement ge in currentFloor.gridElements) {
                if (ge is Unit && ge is not Nail) {
                    if (!lineRenderers.ContainsKey(currentFloor.sqrs.Find(sqr => sqr.coord == ge.coord))) {
                        LineRenderer lr = new GameObject().AddComponent<LineRenderer>();
                        lr.gameObject.transform.parent = ge.transform;
                        lr.startWidth = 0.15f; lr.endWidth = 0.15f;
                        lr.sortingLayerName = "Floor";
                        lr.material = previewMaterial;
                        lr.positionCount = 2;
                        lr.useWorldSpace = false;
                        lr.SetPosition(0, ge.transform.position); lr.SetPosition(1, ge.transform.position);
                        if (ge is PlayerUnit) {
                            lr.startColor = playerColor; lr.endColor = playerColor;
                        } else if (ge is EnemyUnit) {
                            lr.startColor = enemyColor; lr.endColor = enemyColor;
                        } else {
                            lr.startColor = equipmentColor; lr.endColor = equipmentColor;
                        }

                        lineRenderers.Add(currentFloor.sqrs.Find(sqr => sqr.coord == ge.coord), lr);
                        ge.ElementDestroyed += DestroyPreview;

                        Color c = equipmentColor;
                        if (ge is PlayerUnit) c = playerColor;
                        else if (ge is EnemyUnit) c = enemyColor;
                        else c = equipmentColor;
                        floors[currentFloor.index+1].sqrs.Find(sqr => sqr.coord == ge.coord).ToggleValidCoord(true, c, false);
                    }
                }
            }
        }

        float alpha = 255; float timer = 0;
        while (transitioning) {
            foreach (KeyValuePair<GridElement, LineRenderer> lr in lineRenderers) {
                lr.Value.SetPosition(0, lr.Key.transform.position);
            }
            if (!active) {
                foreach (KeyValuePair<GridElement, LineRenderer> lr in lineRenderers) {
                    lr.Value.startColor = new Color(lr.Value.endColor.r, lr.Value.endColor.g, lr.Value.endColor.b, alpha);
                    lr.Value.endColor = new Color(lr.Value.endColor.r, lr.Value.endColor.g, lr.Value.endColor.b, alpha);
                    alpha = Mathf.Lerp(255, 0, timer / 1);
                    timer += Time.deltaTime;
                    yield return null;
                }
            }
            yield return null;
        }
        if (!active) {
            foreach (KeyValuePair<GridElement, LineRenderer> lr in lineRenderers)
                DestroyImmediate(lr.Value.gameObject);
            lineRenderers = new Dictionary<GridElement, LineRenderer>();
            if (currentFloor.index-1 >= 0)
                floors[currentFloor.index-1].DisableGridHighlight();
        }        
    }

    public void DestroyPreview(GridElement ge) {
        if (lineRenderers.ContainsKey(ge)) {
            GameObject go = lineRenderers[ge].gameObject;
            lineRenderers.Remove(ge);
            DestroyImmediate(go);
        }
    }

    public void PreviewButton(bool down) {
        scenario.player.DeselectUnit();
        if (!transitioning)
            StartCoroutine(PreviewFloor(down, true));
        if (uiManager.gameObject.activeSelf)
            uiManager.PlaySound(down ? uiManager.peekBelowSFX.Get() : uiManager.peekAboveSFX.Get());
    }

    public void ChessNotationToggle() {
        notation = !notation;
        foreach (Grid floor in floors)
            floor.ToggleChessNotation(notation);
    }

    public void GridHighlightToggle() {
        gridHightlightOverride = !gridHightlightOverride;
        foreach (Grid floor in floors) {
            floor.overrideHighlight = gridHightlightOverride;
        }
    }

    void SetButtonActive(GameObject button, bool state) {
        button.SetActive(state);
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
        float partialA = down? 0.25f : 1;

// All this code should be refactored into the stencil buffer alpha, NestedFadeGroup should be removed from project
// Store references to which groups of grid objects will be faded with the transition contextually by preview
        List<NestedFadeGroup.NestedFadeGroup> currentFade = new List<NestedFadeGroup.NestedFadeGroup> {
            currentFloor.gridContainer.GetComponent<NestedFadeGroup.NestedFadeGroup>(),
            currentFloor.neutralGEContainer.GetComponent<NestedFadeGroup.NestedFadeGroup>(),
        };
        List<NestedFadeGroup.NestedFadeGroup> partialFade = null;

// Assign addiitonal groups by preview
        if (currentFloor == scenario.player.currentGrid) {
            if (!preview) {
                currentFade.Add(scenario.player.GetComponent<NestedFadeGroup.NestedFadeGroup>());
                currentFade.Add(scenario.currentEnemy.GetComponent<NestedFadeGroup.NestedFadeGroup>());
            } else {
                currentFade.Add(scenario.player.nail.GetComponent<NestedFadeGroup.NestedFadeGroup>());
                partialFade = new List<NestedFadeGroup.NestedFadeGroup>() {
                    scenario.player.GetComponent<NestedFadeGroup.NestedFadeGroup>(),
                    scenario.currentEnemy.GetComponent<NestedFadeGroup.NestedFadeGroup>()
                };
            }
        }
// Same as above and boy is it ugly
        List<NestedFadeGroup.NestedFadeGroup> toFade = null;

        if (toFloor && !down) {
            toFade = new List<NestedFadeGroup.NestedFadeGroup> {
                toFloor.gridContainer.GetComponent<NestedFadeGroup.NestedFadeGroup>(),
                toFloor.neutralGEContainer.GetComponent<NestedFadeGroup.NestedFadeGroup>(),
            };
            if (toFloor == scenario.player.currentGrid) {
                if (!preview) {
                    toFade.Add(scenario.player.GetComponent<NestedFadeGroup.NestedFadeGroup>());
                    toFade.Add(scenario.currentEnemy.GetComponent<NestedFadeGroup.NestedFadeGroup>());
                } else {
                    toFade.Add(scenario.player.nail.GetComponent<NestedFadeGroup.NestedFadeGroup>());
                    partialFade = new List<NestedFadeGroup.NestedFadeGroup>() {
                        scenario.player.GetComponent<NestedFadeGroup.NestedFadeGroup>(),
                        scenario.currentEnemy.GetComponent<NestedFadeGroup.NestedFadeGroup>()
                    };
                }
            }
        }

        peeking = down && preview;
        HideUnits(down);

// And the actual animation. Has become cluttered with NestedFadeGroup logic too.
        float timer = 0;
        while (timer <= transitionDur) {
// Lerp position of floor contatiner
            floorParent.transform.position = Vector3.Lerp(from, to, timer/transitionDur);
            currentFloor.transform.localScale = Vector3.Lerp(fromScale, toScale, timer/transitionDur);
            if (toFloor)
                toFloor.transform.localScale = Vector3.Lerp(toFromScale, Vector3.one, timer/transitionDur);
// Fade in destination floor if present
            if (toFade != null) {
                foreach(NestedFadeGroup.NestedFadeGroup fade in toFade) 
                   fade.AlphaSelf = Mathf.Lerp(0, 1, timer/transitionDur);
            }
// Fade previewed GridElements like player units
            if (partialFade != null) {
                foreach(NestedFadeGroup.NestedFadeGroup fade in partialFade) 
                   fade.AlphaSelf = Mathf.Lerp(partialFrom, partialA, timer/transitionDur);
            }
            if (parallax)
                parallax.ScrollParallax(down ? -1 : 1);

// Coroutine/animation lerp yield
            yield return null;
            timer += Time.deltaTime;
// Fade out currentfloor
            foreach(NestedFadeGroup.NestedFadeGroup fade in currentFade) 
                fade.AlphaSelf = Mathf.Lerp(currFromA, currToA, timer/transitionDur);
            
        }
// Hard set lerped variables
        floorParent.transform.position = to;
        currentFloor.transform.localScale = toScale;
        if (toFloor) toFloor.transform.localScale = Vector3.one;
        foreach(NestedFadeGroup.NestedFadeGroup fade in currentFade) {
            fade.AlphaSelf = currToA;
        }
        if (toFade != null) {
            foreach(NestedFadeGroup.NestedFadeGroup fade in toFade) 
                fade.AlphaSelf = 1;
        }
// Update floor manager current floor... preview next floor untis stats?
        if (down && currentFloor.index-1 >= 0) floors[currentFloor.index-1].gameObject.SetActive(false);
        if (toFloor) currentFloor = toFloor;
        if (uiManager.gameObject.activeSelf)
            uiManager.metaDisplay.UpdateCurrentFloor(currentFloor.index);

    }
    
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
        yield return new WaitForSecondsRealtime(.2f);
        yield return StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Cascade));
        while (scenario.currentTurn == ScenarioManager.Turn.Cascade)
            yield return null;
        StartCoroutine(ToggleDescentPreview(false));
        currentFloor.DisableGridHighlight();

    }

    public void Descend(bool cascade = false, bool tut = false) {
        StartCoroutine(DescendFloors(cascade, tut));
        
    }

    public IEnumerator DescendFloors(bool cascade = false, bool tut = false) {

        if (uiManager.gameObject.activeSelf)
            uiManager.LockFloorButtons(true);
        EnemyManager enemy = (EnemyManager)currentFloor.enemy;
        
        currentFloor.DisableGridHighlight();
        currentFloor.LockGrid(true);
        yield return StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Descent));
        
        if (cascade) {
            yield return StartCoroutine(PreviewFloor(true, true));
            currentFloor.LockGrid(false);
        }
        else
            yield return StartCoroutine(TransitionFloors(true, false));

        scenario.player.nail.ToggleNailState(Nail.NailState.Primed);

        yield return new WaitForSecondsRealtime(0.25f);
        if (betweenFloor.InbetweenTrigger(currentFloor.index-1)) {
            yield return StartCoroutine(betweenFloor.BetweenFloorSegment(currentFloor.index-1));
            
            yield return new WaitForSecondsRealtime(0.25f);
        }

        if (cascade) {
            yield return StartCoroutine(ChooseLandingPositions());
            yield return new WaitForSecondsRealtime(1.25f);
            SetButtonActive(downButton, true); SetButtonActive(upButton, false);
        }

        Coroutine drop = StartCoroutine(DropUnits(floors[currentFloor.index-1], currentFloor));
        scenario.currentEnemy = (EnemyManager)currentFloor.enemy;
        yield return drop;
        if (enemy)
            enemy.SeedUnits(currentFloor);


        scenario.player.DescendGrids(currentFloor);
        currentFloor.LockGrid(false);
        
        yield return new WaitForSecondsRealtime(0.75f);
        
        yield return StartCoroutine(scenario.player.DropNail());       

        if (uiManager.gameObject.activeSelf)    
            uiManager.metaDisplay.UpdateEnemiesRemaining(scenario.currentEnemy.units.Count);

        yield return new WaitForSecondsRealtime(1.5f);
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

    public IEnumerator GenerateNextFloor(GameObject floorPrefab = null, GameObject enemyPrefab = null) {
// Check if player wins
        if (currentFloor.index >= floorDefinitions.Count - 2) {

            StartCoroutine(scenario.Win());

        } else {

            yield return StartCoroutine(TransitionFloors(true, false));

            yield return StartCoroutine(GenerateFloor(floorPrefab, enemyPrefab));

            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(PreviewFloor(false, false));
        }
        if (uiManager.gameObject.activeSelf)
            uiManager.LockFloorButtons(false);
    }


    public IEnumerator DropUnits(Grid fromFloor, Grid toFloor) {
        
        scenario.player.transform.parent = transitionParent;
        scenario.player.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = 1;
        scenario.player.nail.transform.parent = currentFloor.transform;
        scenario.currentEnemy.transform.parent = transitionParent;
        scenario.currentEnemy.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = 1;

        foreach (GridElement ge in fromFloor.gridElements) {
            if (ge is Unit)
                ge.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = 0;
        }

        List<Coroutine> descents = new List<Coroutine>();
        for (int i = fromFloor.gridElements.Count - 1; i >= 0; i--) {
            if (fromFloor.gridElements[i] is Unit u) {
                u.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = 0;
                if (fromFloor.gridElements[i] is not Nail) {
                    GridElement subElement = null;
                    foreach (GridElement ge in toFloor.CoordContents(u.coord)) subElement = ge;
                    descents.Add(StartCoroutine(DropUnit(u, fromFloor.PosFromCoord(u.coord), toFloor.PosFromCoord(u.coord), subElement)));
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }
        for (int i = descents.Count - 1; i >= 0; i--) {
            if (descents[i] != null) 
                yield return descents[i];
            else
                descents.RemoveAt(i);
        }
    }

    public IEnumerator DropUnit(Unit unit, Vector3 from, Vector3 to, GridElement subElement = null) {
        float timer = 0;
        NestedFadeGroup.NestedFadeGroup fade = unit.GetComponent<NestedFadeGroup.NestedFadeGroup>();
        while (timer <= unitDropDur) {
            unit.transform.position = Vector3.Lerp(from, to, timer/unitDropDur);
            fade.AlphaSelf = Mathf.Lerp(0, 1, timer/(unitDropDur/3));
            yield return null;
            timer += Time.deltaTime;
        }
        unit.transform.position = to;
        fade.AlphaSelf = 1;

        if (unit.landingSFX)
            unit.PlaySound(unit.landingSFX.Get());

        if (subElement) {
            StartCoroutine(subElement.CollideFromBelow(unit));
            if (subElement is not GroundElement)
                yield return StartCoroutine(unit.CollideFromAbove(subElement));
        } else if (currentFloor.sqrs.Find(sqr => sqr.coord == unit.coord).tileType == GridSquare.TileType.Bile) {
            yield return new WaitForSecondsRealtime(0.25f);
            StartCoroutine(unit.DestroyElement());
        }   
    }

}
