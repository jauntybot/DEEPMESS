using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossUnit : EnemyUnit {

    [SerializeField] Unit prevTarget = null;

    public override IEnumerator DestroySequence(DamageType dmgType = DamageType.Unspecified, GridElement source = null, EquipmentData sourceEquip = null) {
        manager.StartCoroutine(manager.scenario.FinalDrop());
        yield return base.DestroySequence(dmgType, source, sourceEquip);
    }

    public override bool ValidCommand(Vector2 target, EquipmentData equip) {
        if (equip == null) return false;
        if (validActionCoords.Count == 0) return false;
        if (!validActionCoords.Contains(target)) return false;
        //if (grid.CoordContents(target).Count > 0 && grid.CoordContents(target)[0] == prevTarget) return false;
        if (energyCurrent < equip.energyCost && equip is not MoveData) return false;
        else if (moved && equip is MoveData) return false;

        return true;
    }

}
