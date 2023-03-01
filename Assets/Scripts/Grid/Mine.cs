using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : Unit
{
    public override IEnumerator CollideFromAbove() {

        yield return StartCoroutine(TakeDamage(hpCurrent));

    }
}
