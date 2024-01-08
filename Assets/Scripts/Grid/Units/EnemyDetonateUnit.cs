using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetonateUnit : EnemyUnit {

    public bool primed;
    [SerializeField] Animator explosion;

    public void PrimeSelf() {
        primed = true;
        gfxAnim.SetBool("Primed", true);
    }

    public void Explode() {
        gfxAnim.SetTrigger("Explode");
        explosion.gameObject.SetActive(true);
    }

    public override IEnumerator ScatterTurn() {
        if (!conditions.Contains(Status.Stunned)) {
            if (!primed)
                yield return base.ScatterTurn();
            else
                yield return StartCoroutine(ExplodeCo());
        } else {
            moved = true;
            energyCurrent = 0;
            yield return new WaitForSecondsRealtime(0.125f);
            RemoveCondition(Status.Stunned);
        }
    
    }

    public override IEnumerator CalculateAction() {
        if (!conditions.Contains(Status.Stunned)) {
            if (!primed)
                yield return base.CalculateAction();
            else 
                yield return StartCoroutine(ExplodeCo());
        }
        else {
            moved = true;
            energyCurrent = 0;
            yield return new WaitForSecondsRealtime(0.125f);
        }

        grid.DisableGridHighlight();
        manager.unitActing = false;
    }

    IEnumerator ExplodeCo() {

        manager.SelectUnit(this);
        UpdateAction(equipment[1]);
        grid.DisplayValidCoords(validActionCoords, selectedEquipment.gridColor);
        yield return new WaitForSecondsRealtime(0.5f);
        StartCoroutine(selectedEquipment.UseEquipment(this, null));
        grid.UpdateSelectedCursor(false, Vector2.one * -32);
        grid.DisableGridHighlight();
        Coroutine co = StartCoroutine(TakeDamage(hpCurrent));
        manager.DeselectUnit();
        yield return co;
    }

    public override IEnumerator DestroySequence(DamageType dmgType = DamageType.Unspecified, GridElement source = null, EquipmentData sourceEquip = null) {
        if (!destroyed) {
            if (primed) {
                ObjectiveEventManager.Broadcast(GenerateDestroyEvent(dmgType, source, sourceEquip));        
                yield return StartCoroutine(ExplodeCo());
            } else
                yield return base.DestroySequence(dmgType, source, sourceEquip);
        }
    }

}
