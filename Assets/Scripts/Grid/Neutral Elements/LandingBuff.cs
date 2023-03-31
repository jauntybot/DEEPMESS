using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandingBuff : GroundElement
{

    public enum Buff { Heal, Armor, Energy, }
    public Buff buff;

    public int value;


    public override void OnSharedSpace(GridElement sharedWith)
    {
        base.OnSharedSpace(sharedWith);
        switch (buff) {
            default:
            case Buff.Heal:
                StartCoroutine(sharedWith.TakeDamage(-value));

            break;
        }
        StartCoroutine(DestroyElement());
    }

}
