using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GodParticleGE : GridElement {

    bool harvested = false;
    public enum ParticleType { Orange, Green, Blue };
    public ParticleType type;

    [SerializeField] List<Sprite> sprites;

    public void Init() {
        type = (ParticleType)Random.Range(0, ParticleType.GetNames(typeof(ParticleType)).Length - 1);
        switch (type) {
            case ParticleType.Orange:
                gfx[0].sprite = sprites[0];
            break;
            case ParticleType.Green:
                gfx[0].sprite = sprites[1];
            break;
            case ParticleType.Blue:
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
