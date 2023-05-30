using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilePad : GroundElement
{

    public GameObject undoPrefab;
    public enum Buff { Heal, Equip }
    public Buff buff;

    public int value;
    public int usedValue;

    public override void OnSharedSpace(GridElement sharedWith)
    {
        if (ScenarioManager.instance.currentTurn != ScenarioManager.Turn.Cascade) {
            base.OnSharedSpace(sharedWith);
            switch (buff) {
                default:
                case Buff.Heal:
                    if (sharedWith.hpCurrent < sharedWith.hpMax) {
                        int missing = sharedWith.hpMax - sharedWith.hpCurrent;
                        if (missing > 0) {
                            usedValue = value - missing;
                            usedValue = usedValue <= 0 ? value : usedValue;
                        }                        
                        StartCoroutine(sharedWith.TakeDamage(-value));
                        if (sharedWith is PlayerUnit u)
                            StartCoroutine(WaitForUndoClear(u.pManager));
                        else
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

    IEnumerator WaitForUndoClear(PlayerManager manager) {
        foreach (SpriteRenderer sr in gfx) sr.enabled = false;
        while (manager.undoableMoves.Count > 0) yield return null;
        yield return new WaitForSecondsRealtime(0.125f);
        StartCoroutine(DestroyElement());
    }

    public void UndoDestroy() {
        StopAllCoroutines();
        foreach (SpriteRenderer sr in gfx) sr.enabled = true;
    }
}
