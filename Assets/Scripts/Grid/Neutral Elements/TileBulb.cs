using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBulb : Tile
{

    [Header("Bulb")]
    public GameObject undoPrefab;
    
    public BulbEquipmentData bulb;
    public bool harvested;
    [SerializeField] SFX harvestSFX;

    public void HarvestBulb(PlayerUnit pu) {
        if (!harvested) {
            anim.SetBool("Harvest", true);
            pu.ui.UpdateLoadout(bulb);
            pu.bulbPickups++;
            harvested = true;
            PlaySound(harvestSFX);
        }
    }

    public void UndoHarvest() {
        StopAllCoroutines();
        harvested = false;
        anim.SetTrigger("Undo");
    }

}
