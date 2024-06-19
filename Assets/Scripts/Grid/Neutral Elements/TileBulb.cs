using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBulb : Tile {

    [Header("Bulb")]
    public GameObject undoPrefab;
    
    public BulbEquipmentData bulb;
    public bool harvested;
    [SerializeField] SFX harvestSFX;

    public override void Init(Grid g, Vector2 c) {
        base.Init(g, c);
        string name = gfxAnim.GetCurrentAnimatorClipInfo(0)[0].clip.name;
        gfxAnim.Play(name, 0, Util.Remap(8 + (int)coord.x - (int)coord.y, 1, 15, 1, 18));
    }

    public void HarvestBulb(PlayerUnit pu) {
        if (!harvested) {
            gfxAnim.SetBool("Harvest", true);
            pu.ui.UpdateLoadout(bulb);
            harvested = true;
            PlaySound(harvestSFX);
        }
    }

    public void UndoHarvest() {
        StopAllCoroutines();
        harvested = false;
        gfxAnim.SetTrigger("Undo");
    }

        public override void UpdateElement(Vector2 c) {
        base.UpdateElement(c);
// Offset tile animation to break up the grid
        if (gfxAnim != null) {
            string name = gfxAnim.GetCurrentAnimatorClipInfo(0)[0].clip.name;
            gfxAnim.Play(name, 0, Util.Remap(8 + (int)coord.x - (int)coord.y, 1, 15, 1, 7));
        }
    }

}
