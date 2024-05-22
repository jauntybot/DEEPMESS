using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStaticUnit : EnemyUnit {

    public override IEnumerator ScatterTurn() {
        yield return null;
        manager.unitActing = false;
    }

    public override IEnumerator CalculateAction() {
        if (!conditions.Contains(Status.Stunned)) {
            yield return StartCoroutine(AttackScan());
            yield return null;
        } else {
            moved = true;
            energyCurrent = 0;
            yield return new WaitForSecondsRealtime(0.125f);
            RemoveCondition(Status.Stunned);
            yield return new WaitForSecondsRealtime(0.25f);
        }
        manager.unitActing = false;
    }

    protected override IEnumerator AttackScan() {
        UpdateAction(equipment[1]);
        gfxAnim.SetTrigger("Attack");
        yield return new WaitForSecondsRealtime(0.5f);
        Coroutine co = StartCoroutine(selectedEquipment.UseGear(this));
        grid.UpdateSelectedCursor(false, Vector2.one * -32);
        grid.DisableGridHighlight();
        yield return co;
        yield return new WaitForSecondsRealtime(0.125f);
    }

}
