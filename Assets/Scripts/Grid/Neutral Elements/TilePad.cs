using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilePad : GroundElement
{

    public enum Buff { Heal, Equip }
    public Buff buff;

    public int value;


    public override void OnSharedSpace(GridElement sharedWith)
    {
        if (ScenarioManager.instance.currentTurn != ScenarioManager.Turn.Cascade) {
            base.OnSharedSpace(sharedWith);
            switch (buff) {
                default:
                case Buff.Heal:
                    if (sharedWith.hpCurrent < sharedWith.hpMax) {
                        StartCoroutine(sharedWith.TakeDamage(-value));
                        StartCoroutine(DestroyElement());
                    }
                break;
                case Buff.Equip:
                    if (sharedWith is PlayerUnit pu) {
                        if (pu.usedEquip) {
                            pu.usedEquip = false;
                            StartCoroutine(DestroyElement());
                        }
                    }
                break;
            }
        }
    }
}
