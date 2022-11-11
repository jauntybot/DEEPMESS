using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GridSquare : GridElement {

    [SerializeField] Sprite[] sprites;
    public bool white;

    public override void UpdateElement(GameObject go, Vector2 c)
    {
        base.UpdateElement(go, c);
        if (white) go.GetComponent<SpriteRenderer>().sprite = sprites[1];
    }
}
