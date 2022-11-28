using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Universal data class for any instance that occupies grid space

[RequireComponent(typeof(PolygonCollider2D))]
public class GridElement : MonoBehaviour{

    protected Grid grid;

    public Vector2 coord;
    public bool selectable;
    public PolygonCollider2D hitbox;

// Initialize references, scale to grid
    protected virtual void Start() {
        if (Grid.instance) {
            grid=Grid.instance;
            grid.gridElements.Add(this);
            transform.localScale = Vector3.one * Grid.sqrSize;
        }
        hitbox = GetComponent<PolygonCollider2D>();
        hitbox.enabled = false;
    }

// Update grid position and coordinate
    public virtual void UpdateElement(Vector2 c) {
        transform.position = Grid.PosFromCoord(c);
        coord=c;
    }  

}
