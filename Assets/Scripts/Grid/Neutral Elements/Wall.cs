using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : GridElement
{

    [SerializeField] Animator anim;


    public override IEnumerator DestroySequence(DamageType dmgType = DamageType.Unspecified, GridElement source = null, EquipmentData sourceEquip = null) {
        if (!destroyed) {
            anim.SetTrigger("Destroy");
            yield return base.DestroySequence(dmgType, source, sourceEquip);
        }
    }

}
