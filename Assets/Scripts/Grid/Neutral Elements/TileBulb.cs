using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBulb : Tile
{

    [Header("Bulb")]
    public GameObject undoPrefab;
    
    public BulbEquipmentData bulb;
    public bool harvested;

    [SerializeField] Animator anim;

    public void HarvestBulb(PlayerUnit pu) {
        anim.SetBool("Harvested", true);
        pu.ui.UpdateLoadout(bulb);
        harvested = true;
    }

    IEnumerator WaitForUndoClear(PlayerManager manager) {
        foreach (SpriteRenderer sr in gfx) sr.enabled = false;
        while (manager.undoableMoves.Count > 0) yield return null;
        yield return new WaitForSecondsRealtime(0.125f);
        StartCoroutine(DestroyElement());
    }

    public void UndoDestroy() {
        StopAllCoroutines();
        foreach (SpriteRenderer sr in gfx) sr.enabled = true;
    }

}
