using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorManager : MonoBehaviour
{

    [SerializeField] GameObject floorPrefab;
    [SerializeField] List<LevelDefinition> floorDefinitions;

    public Grid currentFloor;
    public List<Grid> floors;

    [SerializeField] int _gridSize;
    public static int gridSize;
    [SerializeField] float _sqrSize;
    public static float sqrSize;

    public float floorOffset, transitionDur;
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

    public IEnumerator GenerateFloor(bool topFloor, UnitManager enemy) {
        
        Grid newFloor = Instantiate(floorPrefab, this.transform).GetComponent<Grid>();
        if (topFloor) currentFloor = newFloor;
        newFloor.lvlDef = floorDefinitions[Random.Range(0, floorDefinitions.Count - 1)];

        Coroutine co = StartCoroutine(newFloor.GenerateGrid(floors.Count, enemy));
        yield return co;
        newFloor.gameObject.name = "Floor" + newFloor.index;
        floors.Add(newFloor);
    }

    public void SwitchFloors(bool up) {
        if (!up) {
            StartCoroutine(TransitionFloors(currentFloor.gameObject, floors[currentFloor.index+1].gameObject));
            SetButtonActive(upButton, floors[currentFloor.index+1]);
            SetButtonActive(downButton, floors[currentFloor.index+1]);
        } else {
            StartCoroutine(TransitionFloors(currentFloor.gameObject, floors[currentFloor.index-1].gameObject));
            SetButtonActive(upButton, floors[currentFloor.index-1]);
            SetButtonActive(downButton, floors[currentFloor.index-1]);
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
        }
        Vector3 from1 = floor1.transform.position;
        Vector3 to1 = new Vector3(floor1.transform.position.x, floor1.transform.position.y + floorOffset * dir, floor1.transform.position.z);
        
        float timer = 0;
        while (timer < transitionDur) {
            floor1.transform.position = Vector3.Lerp(from1, to1, timer/transitionDur);
            if (floor2) floor2.transform.position = Vector3.Lerp(from2, to2, timer/transitionDur);
            
            yield return null;
            timer += Time.deltaTime;
        }
        if (floor2) currentFloor = floor2.GetComponent<Grid>();
    }

}
