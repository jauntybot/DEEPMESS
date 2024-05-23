using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyAttackData : GearData {

    public int dmg;
    public int dmgMod;

    public override void EquipGear(Unit user) {
        base.EquipGear(user);
        dmgMod = 0;
    }

}
