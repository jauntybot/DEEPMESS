using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossUnit : EnemyUnit
{

    bool secondPhase = false;

    public override IEnumerator TakeDamage(int dmg, DamageType dmgType = DamageType.Unspecified, GridElement source = null)
    {
        yield return base.TakeDamage(dmg, dmgType, source);
        if (hpCurrent <= hpMax/2 && !secondPhase) {
            secondPhase = true;
            manager.scenario.player.TriggerDescent();
        }

    }


}
