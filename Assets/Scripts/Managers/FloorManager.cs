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
    public 
    Coroutine currentTransition;
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

    public void SwitchFloors(bool up) {
        if (!up) {
            currentTransition = StartCoroutine(TransitionFloors(currentFloor.gameObject, floors[currentFloor.index+1].gameObject));
            SetButtonActive(upButton, floors[currentFloor.index+1] != null);
            SetButtonActive(downButton, floors[currentFloor.index+1] != null);
        } else {
            StartCoroutine(TransitionFloors(currentFloor.gameObject, floors[currentFloor.index-1].gameObject));
            SetButtonActive(upButton, floors[currentFloor.index-1] != null);
            SetButtonActive(downButton, floors[currentFloor.index-1] != null);
        }
    }

    void SetButtonActive(GameObject button, bool state) {
        button.SetActive(state);
    }

    public IEnumerator TransitionFloors(GameObject floor1, GameObject floor2 = null) {
        float dir = 1;
        Vector3 from2 = Vector3.zero;
        Vector3 to2 = Vector3.zero;
        if (floor2) {
            dir = (floor1.transform.position.y > floor2.transform.position.y) ? 1 : -1;
            from2 = floor2.transform.position;
            to2 = new Vector3(floor2.transform.position.x, floor2.transform.position.y + floorOffset * dir, floor2.transform.position.z);
            floor2.GetComponent<SortingGroup>().sortingOrder = 0;
        }
        Vector3 from1 = floor1.transform.position;
        Vector3 to1 = new Vector3(floor1.transform.position.x, floor1.transform.position.y + floorOffset * dir, floor1.transform.position.z);
        floor1.GetComponent<SortingGroup>().sortingOrder = -1;

        float timer = 0;
        while (timer < transitionDur) {
            floor1.transform.position = Vector3.Lerp(from1, to1, timer/transitionDur);
            if (floor2) floor2.transform.position = Vector3.Lerp(from2, to2, timer/transitionDur);
            
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
        if (currentFloor.index > 11) {

            StartCoroutine(scenario.Win());

        } else {

            yield return StartCoroutine(TransitionFloors(currentFloor.gameObject));

            yield return StartCoroutine(GenerateFloor(true));

            yield return new WaitForSeconds(0.5f);
            SwitchFloors(true);
            yield return new WaitForSeconds(0.5f);

            StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Enemy));
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
