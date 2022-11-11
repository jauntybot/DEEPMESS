using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridElement : MonoBehaviour{

    Grid grid;

    protected GameObject obj;
    protected Vector2 coord;

    void Start() {
        if (Grid.instance) grid=Grid.instance;
    }

    public virtual void UpdateElement(GameObject go, Vector2 c) {
        obj=go;
        obj.transform.position=Grid.PosFromCoord(c);
        coord=c;
    }  
}
