using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandingBuff : GridElement
{

    public enum Buff { Heal, Armor, Energy, }
    public Buff buff;

    public int value;

    public override IEnumerator CollideFromBelow(GridElement above) {
    
        switch (buff) {
            default:
            case Buff.Heal:
                StartCoroutine(above.TakeDamage(-value));

            break;
        }
        StartCoroutine(DestroyElement());
        yield return null;

    }

}
