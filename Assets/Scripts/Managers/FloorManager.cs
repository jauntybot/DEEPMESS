using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class FloorManager : MonoBehaviour
{

    ScenarioManager scenario;
    
    [Header("Floor Serialization")]
    [SerializeField] GameObject floorPrefab;
    [SerializeField] Transform floorParent;
    [SerializeField] List<FloorDefinition> floorDefinitions;

    public Grid currentFloor;
    public List<Grid> floors;

    [SerializeField] int _gridSize;
    public static int gridSize;
    [SerializeField] float _sqrSize;
    public static float sqrSize;

    [Header("Floor Transitioning")]
    [SerializeField] Transform transitionParent;
    public float floorOffset, transitionDur;
    private bool transitioning;
    [SerializeField] public GameObject upButton, downButton;
    
    [Header("Grid Viz")]
    [SerializeField] private GameObject descentPreview;
    [SerializeField] private Material previewMaterial;
    [SerializeField] private Dictionary<GridElement, LineRenderer> lineRenderers;
    public Color moveColor, attackColor, hammerColor;
    [SerializeField] private Color playerColor, enemyColor;
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
        lineRenderers = new Dictionary<GridElement, LineRenderer>();
    }

    public IEnumerator GenerateFloor() {
        int index = floors.Count;

        Grid newFloor = Instantiate(floorPrefab, this.transform).GetComponent<Grid>();
        currentFloor = newFloor;
        FloorDefinition floorDef = floorDefinitions[index];
        newFloor.lvlDef = floorDef;
    
        Coroutine co = StartCoroutine(newFloor.GenerateGrid(index));
        yield return co;
        newFloor.ToggleChessNotation(notation);
    
        newFloor.gameObject.name = "Floor" + newFloor.index;
        newFloor.transform.SetParent(floorParent);
        floors.Add(newFloor);

    }

    public IEnumerator PreviewFloor(bool down, bool draw) {
        transitioning = true;
        scenario.endTurnButton.enabled = !down;
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

    public IEnumerator ToggleDescentPreview(bool active) {
        
        if (active) {
            lineRenderers = new Dictionary<GridElement, LineRenderer>();
            foreach (GridElement ge in currentFloor.gridElements) {
                if (ge is Unit && ge is not Nail) {
                    if (!lineRenderers.ContainsKey(currentFloor.sqrs.Find(sqr => sqr.coord == ge.coord))) {
                        LineRenderer lr = new GameObject().AddComponent<LineRenderer>();
                        lr.gameObject.transform.parent = descentPreview.transform;
                        lr.startWidth = 0.15f; lr.endWidth = 0.15f;
                        lr.sortingLayerName = "UI";
                        lr.material = previewMaterial;
                        lr.positionCount = 2;
                        lr.SetPosition(0, ge.transform.position); lr.SetPosition(1, ge.transform.position);
                        lr.startColor = enemyColor; lr.endColor = enemyColor;
                        if (ge is PlayerUnit) {
                            lr.startColor = playerColor; lr.endColor = playerColor;
                        }

                        lineRenderers.Add(currentFloor.sqrs.Find(sqr => sqr.coord == ge.coord), lr);
                        ge.ElementDestroyed += DestroyPreview;

                        floors[currentFloor.index+1].sqrs.Find(sqr => sqr.coord == ge.coord).ToggleValidCoord(true,
                        ge is PlayerUnit ? playerColor : enemyColor);
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
            floors[currentFloor.index+1].DisableGridHighlight();
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
        if (!transitioning)
            StartCoroutine(PreviewFloor(down, true));
    }

    public void ChessNotationToggle() {
        notation = !notation;
        foreach (Grid floor in floors)
            floor.ToggleChessNotation(notation);
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

        if (floors.Count - 1 >= currentFloor.index + dir) // Checks if there is a floor in the direction transitioning
            toFloor = floors[currentFloor.index + dir];
// Adjust sorting orders contextually
        Vector3 toFromScale = Vector3.one;
        if (toFloor) {
            toFloor.GetComponent<SortingGroup>().sortingOrder = 0;
            toFromScale = toFloor.transform.localScale;
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

// All this code should be refactored into the stencil buffer alpha, NestedFadeGroup should go
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
// Coroutine/animation lerp yield
            yield return null;
            timer += Time.deltaTime;
// Fade out currentfloor
            foreach(NestedFadeGroup.NestedFadeGroup fade in currentFade) 
                fade.AlphaSelf = Mathf.Lerp(currFromA, currToA, timer/transitionDur);
            
        }
// Hard set lerped variables
        floorParent.transform.position = to;
        foreach(NestedFadeGroup.NestedFadeGroup fade in currentFade) {
            fade.AlphaSelf = currToA;
        }
        if (toFade != null) {
            foreach(NestedFadeGroup.NestedFadeGroup fade in toFade) 
                fade.AlphaSelf = 1;
        }
// Update floor manager current floor... preview next floor untis stats?
        if (toFloor) currentFloor = toFloor;
        UIManager.instance.metaDisplay.UpdateCurrentFloor(currentFloor.index);
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

        Coroutine finalCoroutine = null;

        for (int i = fromFloor.gridElements.Count - 1; i >= 0; i--) {
            if (fromFloor.gridElements[i] is Unit u) {
                u.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = 0;
                if (fromFloor.gridElements[i] is not Nail) {
                    GridElement subElement = null;
                    foreach (GridElement ge in toFloor.CoordContents(u.coord)) subElement = ge;
                    yield return StartCoroutine(DropUnit(u, fromFloor.PosFromCoord(u.coord), toFloor.PosFromCoord(u.coord), subElement));
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }
        //yield return finalCoroutine;
        //Debug.Log("final coroutine");
    }

    public IEnumerator DropUnit(Unit unit, Vector3 from, Vector3 to, GridElement subElement = null) {
        float timer = 0;
        NestedFadeGroup.NestedFadeGroup fade = unit.GetComponent<NestedFadeGroup.NestedFadeGroup>();
        while (timer <= transitionDur) {
            unit.transform.position = Vector3.Lerp(from, to, timer/transitionDur);
            fade.AlphaSelf = Mathf.Lerp(0, 1, timer/transitionDur/3);
            yield return null;
            timer += Time.deltaTime;
        }
        unit.transform.position = to;
        fade.AlphaSelf = 1;

        if (subElement) {
            yield return StartCoroutine(subElement.CollideFromBelow(unit));
            if (subElement is not GroundElement)
                yield return StartCoroutine(unit.CollideFromAbove(subElement));
        } else if (currentFloor.sqrs.Find(sqr => sqr.coord == unit.coord).tileType == GridSquare.TileType.Bile)
            StartCoroutine(unit.DestroyElement());
    }

    public void Descend() {
        StartCoroutine(DescendFloors());
        
    }

    public IEnumerator DescendFloors() {


        EnemyManager enemy = (EnemyManager)currentFloor.enemy;
        
        currentFloor.DisableGridHighlight();
        currentFloor.LockGrid(true);
        yield return StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Descent));
        yield return StartCoroutine(TransitionFloors(true, false));

        yield return new WaitForSecondsRealtime(0.25f);

        yield return StartCoroutine(DropUnits(floors[currentFloor.index-1], currentFloor));

        scenario.player.DescendGrids(currentFloor);
        enemy.SeedUnits(currentFloor);
        scenario.currentEnemy = (EnemyManager)currentFloor.enemy;
        
        currentFloor.LockGrid(false);
        
        yield return new WaitForSecondsRealtime(0.75f);
        print("dropping nail");
        yield return StartCoroutine(scenario.player.DropNail());        
        UIManager.instance.metaDisplay.UpdateTurnsToDescend(scenario.currentEnemy.units.Count);


        yield return new WaitForSecondsRealtime(.75f);
        // for (int i = currentFloor.gridElements.Count - 1; i >= 0; i--) {
        //     if (currentFloor.gridElements[i] is LandingBuff b)
        //         StartCoroutine(b.DestroyElement()); 
        // }
        yield return new WaitForSecondsRealtime(.75f);

// Check if player wins
        if (currentFloor.index >= floorDefinitions.Count - 2) {

            StartCoroutine(scenario.Win());

        } else {

            yield return StartCoroutine(TransitionFloors(true, false));

            yield return StartCoroutine(GenerateFloor());

            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(PreviewFloor(false, false));
            yield return new WaitForSeconds(0.5f);

            StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Enemy));
        }
    }

}
