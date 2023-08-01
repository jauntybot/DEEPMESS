using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBulb : Tile
{

    [Header("Bulb")]
    public GameObject undoPrefab;
    
    public BulbEquipmentData bulb;
    public bool harvested;

    public void HarvestBulb(PlayerUnit pu) {
        if (!harvested) {
            anim.SetBool("Harvest", true);
            pu.ui.UpdateLoadout(bulb);
            harvested = true;
        }
    }

    public void UndoHarvest() {
        StopAllCoroutines();
        harvested = false;
        anim.SetTrigger("Undo");
    }

}
