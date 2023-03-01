using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : Unit
{
    public override IEnumerator CollideFromAbove(Vector2 moveTo) {
        yield return null;
        UpdateElement(moveTo);
        StartCoroutine(TakeDamage(hpCurrent));

    }
}
