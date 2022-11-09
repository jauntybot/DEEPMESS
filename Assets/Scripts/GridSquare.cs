using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GridSquare : MonoBehaviour {

    [SerializeField] Sprite[] sprites;
    
    public void Initialize(GameObject go, Vector3 p, Vector2 c, bool w) {
        obj=go;
        obj.transform.position=p;
        coord=c;
        white=w;
        if (!w) {
            go.GetComponent<SpriteRenderer>().sprite = sprites[1];
        }
    }
    public GameObject obj;
    public Vector2 coord;
    public bool white;
}
