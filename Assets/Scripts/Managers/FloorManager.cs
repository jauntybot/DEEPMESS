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

    public IEnumerator GenerateFloor(bool topFloor) {
        int index = floors.Count;

        Grid newFloor = Instantiate(floorPrefab, this.transform).GetComponent<Grid>();
        if (topFloor) currentFloor = newFloor;
        LevelDefinition floorDef = floorDefinitions[index];
        newFloor.lvlDef = floorDef;
    
        Coroutine co = StartCoroutine(newFloor.GenerateGrid(index));
        yield return co;
    
        newFloor.gameObject.name = "Floor" + newFloor.index;
        floors.Add(newFloor);

    }

    public IEnumerator PreviewFloor(bool down, bool draw) {
        transitioning = true;
        if (down) {
            if (draw) StartCoroutine(ToggleDescentPreview(true));
            if (draw) {
                SetButtonActive(downButton, false); SetButtonActive(upButton, true);
            }
            yield return StartCoroutine(TransitionFloors(currentFloor.gameObject, floors[currentFloor.index+1].gameObject));
            transitioning = false;
        }
        else {
            StartCoroutine(ToggleDescentPreview(false));
            if (draw) {
                SetButtonActive(downButton, true); SetButtonActive(upButton, false);
            }
            yield return StartCoroutine(TransitionFloors(currentFloor.gameObject, floors[currentFloor.index-1].gameObject));
        
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

    public IEnumerator TransitionFloors(GameObject floor1, GameObject floor2 = null) {
        float dir = 1;
        Vector3 from2 = Vector3.zero;
        Vector3 to2 = Vector3.zero;
        Vector3 scaleFrom2 = Vector3.zero;
        Vector3 scaleTo2 = Vector3.zero;
        if (floor2) {
            dir = (floor1.transform.position.y > floor2.transform.position.y) ? 1 : -1;
            from2 = floor2.transform.position;
            to2 = new Vector3(floor2.transform.position.x, floor2.transform.position.y + floorOffset * dir, floor2.transform.position.z);
            floor2.GetComponent<SortingGroup>().sortingOrder = 0;
        }
        Vector3 from1 = floor1.transform.position;
        Vector3 to1 = new Vector3(floor1.transform.position.x, floor1.transform.position.y + floorOffset * dir, floor1.transform.position.z);
        Vector3 scaleFrom1 = Vector3.zero;
        Vector3 scaleTo1 = Vector3.zero;
        floor1.GetComponent<SortingGroup>().sortingOrder = -1;

        float timer = 0;
        while (timer <= transitionDur) {
            floor1.transform.position = Vector3.Lerp(from1, to1, timer/transitionDur);
            if (floor2) {
                floor2.transform.position = Vector3.Lerp(from2, to2, timer/transitionDur);
                //floor2.transform.localScale = Vector3.Lerp()
            }
            yield return null;
            timer += Time.deltaTime;
        }
        floor1.transform.position = to1;
        if (floor2) {
            currentFloor = floor2.GetComponent<Grid>();
            floor2.transform.position = to2;
        }
    }

    public void Descend() {
        StartCoroutine(DescendFloors());
        
    }

    public IEnumerator DescendFloors() {

        EnemyManager enemy = (EnemyManager)currentFloor.enemy;
        enemy.transform.parent = transitionParent;
        scenario.player.transform.parent = transitionParent;
        scenario.player.nail.transform.parent = currentFloor.transform;

        currentFloor.DisableGridHighlight();
        yield return StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Descent));
        DescentCollisionSolver();
        yield return StartCoroutine(TransitionFloors(currentFloor.gameObject, floors[currentFloor.index+1].gameObject));

        enemy.SeedUnits(currentFloor);
        scenario.currentEnemy = (EnemyManager)currentFloor.enemy;
        scenario.player.DescendGrids(currentFloor);
        
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

            yield return StartCoroutine(TransitionFloors(currentFloor.gameObject));

            yield return StartCoroutine(GenerateFloor(true));

            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(PreviewFloor(false, false));
            yield return new WaitForSeconds(0.5f);

            StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Player));
        }
    }

    public void DescentCollisionSolver() {
        List<GridElement> subElements = new List<GridElement>();

        foreach (GridElement ge in currentFloor.gridElements) {
            if (ge is Unit u) { // Replace with descends? bool
                if (u is not Nail) {
                    GridElement subGE = floors[currentFloor.index+1].gridElements.Find(g => g.coord == u.coord);
                    if (subGE) {
                        StartCoroutine(u.CollideFromAbove(u.coord));
                        StartCoroutine(subGE.CollideFromBelow(u));
                    }
                }     
            }
        }
    }

}
