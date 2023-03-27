using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class FloorManager : MonoBehaviour
{

    ScenarioManager scenario;
    [SerializeField] GameObject floorPrefab;
    
    public Color moveColor, attackColor, hammerColor;
    [SerializeField] List<LevelDefinition> floorDefinitions;

    public Grid currentFloor;
    [SerializeField] Transform floorParent;
    public List<Grid> floors;

    [SerializeField] int _gridSize;
    public static int gridSize;
    [SerializeField] float _sqrSize;
    public static float sqrSize;

    [SerializeField] Transform transitionParent;
    public float floorOffset, transitionDur;
    private bool transitioning;
    private bool notation = false;
    [SerializeField] private GameObject descentPreview;
    [SerializeField] private Dictionary<GridElement, LineRenderer> lineRenderers;
    [SerializeField] private Material previewMaterial;
    [SerializeField] private Color playerColor, enemyColor;
    [SerializeField] public GameObject upButton, downButton;

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
        LevelDefinition floorDef = floorDefinitions[index];
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

    public IEnumerator TransitionFloors(bool down, bool preview) {
        int dir = down? 1 : -1;
        Grid toFloor = null;
        if (floors.Count - 1 >= currentFloor.index + dir)
            toFloor = floors[currentFloor.index + dir];
        
        if (toFloor) toFloor.GetComponent<SortingGroup>().sortingOrder = 0;
        if (!preview) currentFloor.GetComponent<SortingGroup>().sortingOrder = -1;
        else if (!down) currentFloor.GetComponent<SortingGroup>().sortingOrder = -1;
        else currentFloor.GetComponent<SortingGroup>().sortingOrder = 1;
        
        Vector3 from = floorParent.transform.position;
        Vector3 to = new Vector3(from.x, from.y + floorOffset * dir, from.z);

        float currFromA = 1;
        float currToA = down? 0 : 1;
        float partialFrom = scenario.player.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf;
        float partialA = down? 0.25f : 1;

        List<NestedFadeGroup.NestedFadeGroup> currentFade = new List<NestedFadeGroup.NestedFadeGroup> {
            currentFloor.gridContainer.GetComponent<NestedFadeGroup.NestedFadeGroup>(),
            currentFloor.neutralGEContainer.GetComponent<NestedFadeGroup.NestedFadeGroup>(),
        };
        List<NestedFadeGroup.NestedFadeGroup> partialFade = null;

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

        float timer = 0;
        while (timer <= transitionDur) {
            floorParent.transform.position = Vector3.Lerp(from, to, timer/transitionDur);
            if (toFade != null) {
                foreach(NestedFadeGroup.NestedFadeGroup fade in toFade) 
                   fade.AlphaSelf = Mathf.Lerp(0, 1, timer/transitionDur);
            }
            if (partialFade != null) {
                foreach(NestedFadeGroup.NestedFadeGroup fade in partialFade) 
                   fade.AlphaSelf = Mathf.Lerp(partialFrom, partialA, timer/transitionDur);
            }
            yield return null;
            timer += Time.deltaTime;
            foreach(NestedFadeGroup.NestedFadeGroup fade in currentFade) 
                fade.AlphaSelf = Mathf.Lerp(currFromA, currToA, timer/transitionDur);
            
        }
        floorParent.transform.position = to;
        foreach(NestedFadeGroup.NestedFadeGroup fade in currentFade) {
            fade.AlphaSelf = currToA;
        }
        if (toFade != null) {
            foreach(NestedFadeGroup.NestedFadeGroup fade in toFade) 
                fade.AlphaSelf = 1;
        }

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
                    finalCoroutine = StartCoroutine(DropUnit(u, fromFloor.PosFromCoord(u.coord), toFloor.PosFromCoord(u.coord), toFloor.CoordContents(u.coord)));
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }
        yield return finalCoroutine;
        print("final coroutine");
    }

    private IEnumerator DropUnit(Unit unit, Vector3 from, Vector3 to, GridElement subElement = null) {
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
            if (subElement is not LandingBuff)
                yield return StartCoroutine(unit.CollideFromAbove());
        }
    }

    public void Descend() {
        StartCoroutine(DescendFloors());
        
    }

    public IEnumerator DescendFloors() {


        EnemyManager enemy = (EnemyManager)currentFloor.enemy;
        
        currentFloor.DisableGridHighlight();
        yield return StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Descent));
        yield return StartCoroutine(TransitionFloors(true, false));

        yield return new WaitForSecondsRealtime(0.25f);

        yield return StartCoroutine(DropUnits(floors[currentFloor.index-1], currentFloor));

        scenario.player.DescendGrids(currentFloor);
        enemy.SeedUnits(currentFloor);
        scenario.currentEnemy = (EnemyManager)currentFloor.enemy;
        
        
        yield return new WaitForSecondsRealtime(0.75f);
        print("dropping nail");
        yield return StartCoroutine(scenario.player.DropNail());        
        UIManager.instance.metaDisplay.UpdateTurnsToDescend(scenario.turnsToDescend - scenario.turnCount);


        yield return new WaitForSecondsRealtime(.75f);
        for (int i = currentFloor.gridElements.Count - 1; i >= 0; i--) {
            if (currentFloor.gridElements[i] is LandingBuff b)
                StartCoroutine(b.DestroyElement()); 
        }
        yield return new WaitForSecondsRealtime(.75f);

// Check if player wins
        if (currentFloor.index >= 11) {

            StartCoroutine(scenario.Win());

        } else {

            yield return StartCoroutine(TransitionFloors(true, false));

            yield return StartCoroutine(GenerateFloor());

            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(PreviewFloor(false, false));
            yield return new WaitForSeconds(0.5f);

            StartCoroutine(scenario.SwitchTurns(scenario.prevTurn));
        }
    }

}
