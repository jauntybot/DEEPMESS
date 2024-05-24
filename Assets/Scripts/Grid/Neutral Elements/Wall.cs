using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : GridElement {

    [SerializeField] Animator anim;
    [SerializeField] List<Sprite> rndSprite;
    [SerializeField] List<PolygonCollider2D> colliders;

    protected override void Start() {
        base.Start();
        anim.enabled = false;
        if (rndSprite.Count > 0) {
            int rnd = Random.Range(0,rndSprite.Count);
            gfx[0].sprite = rndSprite[rnd];
            hitbox = colliders[rnd];
        }
    }

    public override IEnumerator DestroySequence(DamageType dmgType = DamageType.Unspecified, GridElement source = null, GearData sourceEquip = null) {
        anim.enabled = true;
        anim.SetTrigger("Destroy");
        yield return base.DestroySequence(dmgType, source, sourceEquip);
    }

}
