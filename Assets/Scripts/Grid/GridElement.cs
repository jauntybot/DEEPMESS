using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class GridElement : MonoBehaviour{

    protected Grid grid;

    public Vector2 coord;
    public bool selectable;
    public BoxCollider2D hitbox;

    protected virtual void Start() {
        if (Grid.instance) {
            grid=Grid.instance;
            grid.gridElements.Add(this);
        }
        hitbox = GetComponent<BoxCollider2D>();
        hitbox.enabled = false;
    }

    public virtual void UpdateElement(Vector2 c) {
        transform.position = Grid.PosFromCoord(c);
        coord=c;
    }  
}
