using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : GridElement
{

    [SerializeField] Animator anim;
    [SerializeField] List<Sprite> rndSprite;

    protected override void Start() {
        base.Start();
        anim.enabled = false;
        if (rndSprite.Count > 0) {
            gfx[0].sprite = rndSprite[Random.Range(0,rndSprite.Count)];
            Debug.Log("Rndm wall");

        }
    }

    public override IEnumerator DestroySequence(DamageType dmgType = DamageType.Unspecified, GridElement source = null, EquipmentData sourceEquip = null) {
        if (!destroyed) {
            anim.enabled = true;
            anim.SetTrigger("Destroy");
            yield return base.DestroySequence(dmgType, source, sourceEquip);
        }
    }

}
