using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyAttackData : EquipmentData {

    public int dmg;
    public int dmgMod;

    public override void EquipEquipment(Unit user) {
        base.EquipEquipment(user);
        dmgMod = 0;
    }

}
