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
        }

         protected virtual void ConditionApplied(UnitConditionEvent evt) {
            switch (relicType) {
                default:
                case RelicType.ScytheCells:
                    if (evt.target is EnemyUnit && evt.condition is Unit.Status.Restricted && !evt.undo) {
                        evt.target.ApplyCondition(Unit.Status.Weakened);
                    } else if (evt.target is EnemyUnit && evt.condition is Unit.Status.Restricted && evt.undo)
                        evt.target.RemoveCondition(Unit.Status.Weakened);
                break;
            }
        }
    }
}

