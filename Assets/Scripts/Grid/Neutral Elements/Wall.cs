using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : GridElement {
    [SerializeField] List<Sprite> rndSprite;
    [SerializeField] List<PolygonCollider2D> colliders;

    public override void Init(Grid g, Vector2 c) {
        base.Init(g, c);
        gfxAnim.enabled = false;
        if (rndSprite.Count > 0) {
            int rnd = Random.Range(0,rndSprite.Count);
            gfx[0].sprite = rndSprite[rnd];
            hitbox = colliders[rnd];
        }
    }

    public override IEnumerator DestroySequence(DamageType dmgType = DamageType.Unspecified, GridElement source = null, GearData sourceEquip = null) {
        gfxAnim.enabled = true;
        gfxAnim.SetTrigger("Destroy");
        yield return base.DestroySequence(dmgType, source, sourceEquip);
    }

}
