using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodSplatter : MonoBehaviour {

    SpriteRenderer gfx;
    
    [SerializeField]
    List<Sprite> directions;


    public void Init(GridElement origin, Vector2 dir) {
        gfx = GetComponentInChildren<SpriteRenderer>();

        if (dir.x > 0)
            gfx.sprite = directions[0];
        else if (dir.x < 0)
            gfx.sprite = directions[1];
        else if (dir.y > 0)
            gfx.sprite = directions[2];
        else if (dir.y < 0)
            gfx.sprite = directions[3];

        transform.position = origin.grid.PosFromCoord(origin.coord + (dir/2));
        gfx.sortingOrder = origin.grid.SortOrderFromCoord(origin.coord + dir);
    }

}
