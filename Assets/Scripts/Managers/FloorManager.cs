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
        
        Grid newFloor = Instantiate(floorPrefab, this.transform).GetComponent<Grid>();
        if (topFloor) currentFloor = newFloor;
        LevelDefinition floorDef = floorDefinitions[Random.Range(0, floorDefinitions.Count)];
        newFloor.lvlDef = floorDef;

        Coroutine co = StartCoroutine(newFloor.GenerateGrid(floors.Count));
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
        if (floor2) currentFloor = floor2.GetComponent<Grid>();
        
    }

    public void Descend() {
        StartCoroutine(DescendFloors());
        
    }

    public IEnumerator DescendFloors() {

        DescentCollisionSolver();

        EnemyManager enemy = (EnemyManager)currentFloor.enemy;
        enemy.transform.parent = transitionParent;
        scenario.player.transform.parent = transitionParent;
        scenario.player.drill.transform.parent = currentFloor.transform;

        yield return StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Descent));
        yield return StartCoroutine(TransitionFloors(currentFloor.gameObject, floors[currentFloor.index+1].gameObject));
        
        enemy.SeedUnits(currentFloor);
        scenario.currentEnemy = (EnemyManager)currentFloor.enemy;
        scenario.player.DescendGrids(currentFloor);
        
        yield return new WaitForSecondsRealtime(0.75f);
        yield return StartCoroutine(currentFloor.DropDrill());        
        yield return new WaitForSecondsRealtime(0.75f);

        yield return StartCoroutine(TransitionFloors(currentFloor.gameObject));

        yield return StartCoroutine(GenerateFloor(true));

        yield return new WaitForSeconds(0.5f);
        SwitchFloors(true);
        yield return new WaitForSeconds(0.5f);

        StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Enemy));
    }

    public void DescentCollisionSolver() {
        List<Unit> bumpedUnits = new List<Unit>();
        List<Vector2> bumpOccupancy = new List<Vector2>();
        foreach (GridElement ge in currentFloor.gridElements) {
            if (ge is Unit u) {
                GridElement subGE = floors[currentFloor.index+1].gridElements.Find(g => g.coord == u.coord);
                if (subGE) 
                    bumpedUnits.Add(u);
                else
                    bumpOccupancy.Add(u.coord);
            }
        }
        foreach (Unit u in bumpedUnits) {
            List<Vector2> bumpCoords = new List<Vector2>();
                    Vector2 dir = Vector2.zero;
                    for (int i = 0; i <= 3; i++) {
                        switch (i) {
                            case 0: dir = Vector2.up; break;
                            case 1: dir = Vector2.right; break;
                            case 2: dir = Vector2.down; break;
                            case 3: dir = Vector2.left; break;
                        }
                        Vector2 coord = u.coord + dir;
                        foreach(Vector2 c in bumpOccupancy)
                            print ("bumpCoords" + c);
                        print (coord);
                        if (!floors[currentFloor.index+1].CoordContents(coord) 
                            && coord.x >= 0 && coord.x <= gridSize -1 && coord.y >= 0 && coord.y <= gridSize -1) {
                                if (bumpOccupancy.Contains(coord)) print ("occupied");
                                else bumpCoords.Add(coord);
                            }
                    }
                    Vector2 target = bumpCoords[Random.Range(0, bumpCoords.Count - 1)];
                    bumpOccupancy.Add(target);
                    GridElement subGE = floors[currentFloor.index+1].gridElements.Find(ge => ge.coord == u.coord);
                    if (subGE is Unit subU) {
                        StartCoroutine(subU.TakeDamage(1));
                    }
                    StartCoroutine(u.CollideOnDescent(target));
                    
        }

    }

}
