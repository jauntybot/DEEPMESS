using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GodParticleGE : GridElement {

    bool harvested = false;
    public SlagEquipmentData.UpgradePath type;

    [SerializeField] List<Sprite> sprites;

    public void Init() {
        type = (SlagEquipmentData.UpgradePath)Random.Range(0, SlagEquipmentData.UpgradePath.GetNames(typeof(SlagEquipmentData.UpgradePath)).Length);
        switch (type) {
            case SlagEquipmentData.UpgradePath.Shunt:
                gfx[0].sprite = sprites[0];
            break;
            case SlagEquipmentData.UpgradePath.Scab:
                gfx[0].sprite = sprites[1];
            break;
            case SlagEquipmentData.UpgradePath.Sludge:
                gfx[0].sprite = sprites[2];
            break;
        }
    }

    public override void OnSharedSpace(GridElement sharedWith) {
        if (!harvested) {
            base.OnSharedSpace(sharedWith);
            if (sharedWith is PlayerUnit pu) {
                pu.pManager.collectedParticles.Add(type);
                pu.pManager.harvestedByMove.Add(pu, this);
                pu.pManager.UndoClearCallback += DestroySelf;
                foreach(SpriteRenderer sr in gfx)
                    sr.enabled = false;

                harvested = true;
            }
        }
    }

    public void UndoHarvest() {
        foreach(SpriteRenderer sr in gfx)
            sr.enabled = true;

        harvested = false;
    }

    void DestroySelf(PlayerManager pm) {
        pm.UndoClearCallback -= DestroySelf;
        StartCoroutine(DestroySequence());
    }
}
