using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : GridElement
{

    [SerializeField] Animator anim;


    public override IEnumerator DestroySequence(DamageType dmgType = DamageType.Unspecified, GridElement source = null, EquipmentData sourceEquip = null) {
        anim.SetTrigger("Destroy");
        return base.DestroySequence(dmgType, source, sourceEquip);
    }

}
