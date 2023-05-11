using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anvil : Unit
{
    public override IEnumerator CollideFromAbove(GridElement subGE) {

        yield return StartCoroutine(TakeDamage(hpCurrent));

    }

    public override void EnableSelection(bool state) {}
}
