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

        //gfx.sortingOrder = origin.grid.SortOrderFromCoord(origin.coord + dir);
        Tile parent1 = origin.grid.tiles.Find(t => t.coord == origin.coord+dir);
        Tile parent2 = origin.grid.tiles.Find(t => t.coord == origin.coord);
        transform.position = origin.grid.PosFromCoord(origin.coord);
        if (!parent1 || (parent1 && (parent1.tileType != Tile.TileType.Bone || parent1 is TileBulb))
            || parent2.tileType != Tile.TileType.Bone || parent1 is TileBulb) {
            anim.SetBool("Dissolve", true);
        }
    }

}
