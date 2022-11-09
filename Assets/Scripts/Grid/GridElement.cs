using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface GridElement {

    public static GameObject obj;
    public static Vector2 coord;

     public virtual void Initialize(GameObject go, Vector3 p, Vector2 c, bool w) {
        obj=go;
        obj.transform.position=p;
        coord=c;
    }
    
}
