using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodSplatter : MonoBehaviour {

    Animator anim;
    
    public void Init(GridElement origin, Vector2 dir) {
        anim = GetComponentInChildren<Animator>();

        if (dir.x > 0) anim.SetTrigger("+X");
        else if (dir.x < 0) anim.SetTrigger("-X");
        else if (dir.y > 0) anim.SetTrigger("+Y");
        else if (dir.y < 0) anim.SetTrigger("-Y");

        transform.position = origin.grid.PosFromCoord(origin.coord + dir/3);
        //gfx.sortingOrder = origin.grid.SortOrderFromCoord(origin.coord + dir);
        Tile parent = origin.grid.tiles.Find(t => t.coord == origin.coord+dir);
        if (!parent || (parent && (parent.tileType == Tile.TileType.Blood || parent.tileType == Tile.TileType.Bile))) {
            anim.SetBool("Dissolve", true);
        }
    }

}
