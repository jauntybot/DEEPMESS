using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossUnit : EnemyUnit {

    [SerializeField] Unit prevTarget = null;

    public override IEnumerator DestroySequence(DamageType dmgType = DamageType.Unspecified, GridElement source = null, GearData sourceEquip = null) {
        manager.StartCoroutine(manager.scenario.FinalDrop());
        yield return base.DestroySequence(dmgType, source, sourceEquip);
    }

    public override IEnumerator CalculateAction() {
        if (!conditions.Contains(Status.Stunned)) {
            UpdateAction(equipment[1]);
            foreach (Vector2 coord in validActionCoords) {
            if (ValidCommand(coord, selectedEquipment)) {
                GridElement target = null;
                foreach (GridElement ge in grid.CoordContents(coord))
                    target = ge;
                yield return new WaitForSecondsRealtime(0.5f);
                Coroutine co = StartCoroutine(selectedEquipment.UseGear(this, target));
                grid.UpdateSelectedCursor(false, Vector2.one * -32);
                grid.DisableGridHighlight();
                yield return co;
                yield return new WaitForSecondsRealtime(0.125f);
            }
        }
            if (energyCurrent > 0) {
                if (!moved) yield return StartCoroutine(MoveScan());
                yield return StartCoroutine(AttackScan());
            }
        } else {
            moved = true;
            //energyCurrent = 0;
            yield return new WaitForSecondsRealtime(0.125f);
            RemoveCondition(Status.Stunned);
            yield return new WaitForSecondsRealtime(0.25f);
        }
       
        grid.DisableGridHighlight();
        manager.unitActing = false;
    }

    protected override IEnumerator AttackScan() {
        UpdateAction(equipment[1]);
        yield return new WaitForSecondsRealtime(0.5f);
        Coroutine co = StartCoroutine(selectedEquipment.UseGear(this));
        grid.UpdateSelectedCursor(false, Vector2.one * -32);
        grid.DisableGridHighlight();
        yield return co;
        yield return new WaitForSecondsRealtime(0.125f);
    }

    public override bool ValidCommand(Vector2 target, GearData equip) {
        if (equip == null) return false;
        if (validActionCoords.Count == 0) return false;
        if (!validActionCoords.Contains(target)) return false;
        //if (grid.CoordContents(target).Count > 0 && grid.CoordContents(target)[0] == prevTarget) return false;
        if (energyCurrent < equip.energyCost && equip is not MoveData) return false;
        else if (moved && equip is MoveData) return false;

        return true;
    }

    public override void ApplyCondition(Status s, bool undo = false) {}

}
