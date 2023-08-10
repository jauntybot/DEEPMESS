using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : GridElement
{

    [SerializeField] Animator anim;


    public override IEnumerator DestroyElement(DamageType dmgType = DamageType.Unspecified)
    {
        anim.SetTrigger("Destroy");
        return base.DestroyElement(dmgType);
    }

}
