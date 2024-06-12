using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Relics {

    [CreateAssetMenu(menuName = "Relics/On Condition Applied")]
    public class OnConditionApplied : RelicData {
        
         enum RelicType { ScytheCells };
        [SerializeField] RelicType relicType;

        public override void Init() {
            ObjectiveEventManager.AddListener<UnitConditionEvent>(ConditionApplied);
            base.Init();
            switch(relicType) {
                default:break;
                case RelicType.ScytheCells:
                    ScenarioManager scenario = ScenarioManager.instance;
                    foreach(Unit u in scenario.currentEnemy.units) {
                        if (u.conditions.Contains(Unit.Status.Restricted)) 
                            u.ApplyCondition(Unit.Status.Weakened);
                    }
                break;
            }
        }

         protected virtual void ConditionApplied(UnitConditionEvent evt) {
            switch (relicType) {
                default:
                case RelicType.ScytheCells:
                    if (evt.target is EnemyUnit && evt.condition is Unit.Status.Restricted && !evt.undo && evt.apply) {
                        evt.target.ApplyCondition(Unit.Status.Weakened);
                    } else if (evt.target is EnemyUnit && evt.condition is Unit.Status.Restricted && !evt.undo && !evt.apply)
                        evt.target.RemoveCondition(Unit.Status.Weakened);
                break;
            }
        }

        public override void UnsubRelic() {
            base.UnsubRelic();
            ObjectiveEventManager.RemoveListener<UnitConditionEvent>(ConditionApplied);
        }

    }
}

