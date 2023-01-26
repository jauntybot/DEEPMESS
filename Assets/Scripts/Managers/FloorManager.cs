using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorManager : MonoBehaviour
{

    [SerializeField] GameObject floorPrefab;
    [SerializeField] List<LevelDefinition> floorDefinitions;


    [SerializeField] int _gridSize;
    public static int gridSize;
    [SerializeField] float _sqrSize;
    public static float sqrSize;

    public static Grid currentFloor;
    public List<Grid> floors;

     #region Singleton (and Awake)
    public static FloorManager instance;
    private void Awake() {
        if (FloorManager.instance) {
            Debug.Log("Warning! More than one instance of Grid found!");
            return;
        }
        FloorManager.instance = this;
        gridSize=_gridSize; sqrSize = _sqrSize;
    }
    #endregion

    public IEnumerator GenerateFloor(bool topFloor) {
        
        Grid newFloor = Instantiate(floorPrefab, this.transform).GetComponent<Grid>();
        if (topFloor) currentFloor = newFloor;
        newFloor.lvlDef = floorDefinitions[Random.Range(0, floorDefinitions.Count)];
        Coroutine co = StartCoroutine(newFloor.GenerateGrid());

        yield return co;
    }

}
