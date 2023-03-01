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
    [SerializeField] private GameObject descentPreview;
    [SerializeField] private Dictionary<GridElement, LineRenderer> lineRenderers;
    [SerializeField] private Material previewMaterial;
    [SerializeField] private Color playerColor, enemyColor;
    [SerializeField] GameObject upButton, downButton;

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
    
        newFloor.gameObject.name = "Floor" + newFloor.index;
        newFloor.transform.parent = floorParent;
        floors.Add(newFloor);

    }

    public IEnumerator PreviewFloor(bool down, bool draw) {
        transitioning = true;
        if (down) {
            if (draw) StartCoroutine(ToggleDescentPreview(true));
            if (draw) {
                SetButtonActive(downButton, false); SetButtonActive(upButton, true);
            }
            yield return StartCoroutine(TransitionFloors(down));
            transitioning = false;
        }
        else {
            StartCoroutine(ToggleDescentPreview(false));
            if (draw) {
                SetButtonActive(downButton, true); SetButtonActive(upButton, false);
            }
            yield return StartCoroutine(TransitionFloors(down));
        
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

                    lineRenderers.Add(ge, lr);
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

    public void PreviewButton(bool down) {
        StartCoroutine(PreviewFloor(down, true));
    }

    void SetButtonActive(GameObject button, bool state) {
        button.SetActive(state);
    }

    public IEnumerator TransitionFloors(bool down) {
        int dir = down? 1 : -1;
        Grid toFloor = null;
        if (floors.Count - 1 >= currentFloor.index + dir)
            toFloor = floors[currentFloor.index + dir];
        
        if (toFloor) toFloor.GetComponent<SortingGroup>().sortingOrder = 0;
        currentFloor.GetComponent<SortingGroup>().sortingOrder = -1;
        
        Vector3 from = floorParent.transform.position;
        Vector3 to = new Vector3(from.x, from.y + floorOffset * dir, from.z);

        float timer = 0;
        while (timer <= transitionDur) {
            floorParent.transform.position = Vector3.Lerp(from, to, timer/transitionDur);
            yield return null;
            timer += Time.deltaTime;
        }
        floorParent.transform.position = to;

        if (toFloor) currentFloor = toFloor;
    }

    public IEnumerator DropUnits(Grid fromFloor, Grid toFloor) {
        
        scenario.player.transform.parent = transitionParent;
        scenario.player.nail.transform.parent = currentFloor.transform;
        scenario.currentEnemy.transform.parent = transitionParent;

        for (int i = fromFloor.gridElements.Count - 1; i >= 0; i--) {
            if (fromFloor.gridElements[i] is Unit u && fromFloor.gridElements[i] is not Nail) {
                StartCoroutine(DropUnit(u, fromFloor.PosFromCoord(u.coord), toFloor.PosFromCoord(u.coord), toFloor.CoordContents(u.coord)));
                yield return new WaitForSeconds(0.1f);
            }
        }


    }

    private IEnumerator DropUnit(Unit unit, Vector3 from, Vector3 to, GridElement subElement = null) {
        float timer = 0;
        while (timer <= transitionDur) {
            unit.transform.position = Vector3.Lerp(from, to, timer/transitionDur);
            yield return null;
            timer += Time.deltaTime;
        }
        if (subElement) {
            yield return StartCoroutine(subElement.CollideFromBelow(unit));
            yield return StartCoroutine(unit.CollideFromAbove(unit.coord));
        }
    }

    public void Descend() {
        StartCoroutine(DescendFloors());
        
    }

    public IEnumerator DescendFloors() {


        EnemyManager enemy = (EnemyManager)currentFloor.enemy;
        
        currentFloor.DisableGridHighlight();
        yield return StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Descent));
        yield return StartCoroutine(TransitionFloors(true));

        yield return new WaitForSecondsRealtime(0.25f);

        yield return StartCoroutine(DropUnits(floors[currentFloor.index-1], currentFloor));

        scenario.player.DescendGrids(currentFloor);
        enemy.SeedUnits(currentFloor);
        scenario.currentEnemy = (EnemyManager)currentFloor.enemy;
        
        
        yield return new WaitForSecondsRealtime(0.75f);
        yield return StartCoroutine(scenario.player.DropNail());        
        

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

            yield return StartCoroutine(TransitionFloors(true));

            yield return StartCoroutine(GenerateFloor());

            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(PreviewFloor(false, false));
            yield return new WaitForSeconds(0.5f);

            StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Player));
        }
    }

    public IEnumerator DescentCollisionSolver(Grid subFloor) {
        List<GridElement> subElements = new List<GridElement>();

        foreach (GridElement ge in floors[subFloor.index-1].gridElements) {
            if (ge is Unit u) { // Replace with descends? bool
                if (u is not Nail) {
                    GridElement subGE = currentFloor.gridElements.Find(g => g.coord == u.coord);
                    if (subGE) {
                        StartCoroutine(u.CollideFromAbove(u.coord));
                        StartCoroutine(subGE.CollideFromBelow(u));
                    }
                }     
            }
        }
        yield return new WaitForSeconds(0.25f);
    }

}
