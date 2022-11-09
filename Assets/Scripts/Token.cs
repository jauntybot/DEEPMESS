using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Token : MonoBehaviour {

    public void Initialize(GameObject go, Vector3 p, Vector2 c) {
        obj=go;
        obj.transform.position=p;
        coord=c;
    }
    public GameObject obj;
    public Vector2 coord;
}