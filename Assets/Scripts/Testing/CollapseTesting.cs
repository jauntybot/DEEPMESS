using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CollapseTesting : MonoBehaviour
{

    [SerializeField] GameObject gridPrefab;
    [SerializeField] FloorDefinition floorDef;
    [SerializeField] Grid activeGrid = null;
    [SerializeField] bool regenerate, collapse;

    void Update() {
        if (regenerate) {

            activeGrid = Instantiate(gridPrefab).GetComponent<Grid>();
            activeGrid.lvlDef = floorDef;
            activeGrid.GenerateGrid(0);

            regenerate = false;
        }
        if (collapse) { 
            StartCoroutine(activeGrid.ShockwaveCollapse());

            collapse = false;
        }
    }

}
