using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Relics {
    
    public abstract class RelicData : ScriptableObject {
        
        public new string name;
        public string description;
        public Sprite sprite;
        public int scrapValue;

        public virtual void Init() {       
        }

       

// OnDamage Relics
        [CreateAssetMenu(menuName = "Relics/On Damage Relic")]
        public class OnDamageRelic : RelicData {
        
        enum RelicType { SelfDestructButtons, HardHats };
        [SerializeField] RelicType relicType;
        

        public override void Init() {
            ObjectiveEventManager.AddListener<GridElementDamagedEvent>(OnElementDamaged);
            base.Init();
        }

        protected virtual void OnElementDamaged(GridElementDamagedEvent evt) {
            switch (relicType) {
                default:
                case RelicType.SelfDestructButtons:
                    if (evt.element is Unit u && u.manager is PlayerManager && u.hpCurrent - evt.dmg <= 0) {

                    }
                break;
                case RelicType.HardHats:
                    if (evt.element is PlayerUnit pu && pu.conditions.Contains(Unit.Status.Disabled) && evt.dmg < 0) {
                        

                    }
                break;
            }
        }


    }

    

// OnDamage Relics
    [CreateAssetMenu(menuName = "Relics/On Equipment Relic")]
    public class OnEquipmentRelic : RelicData {
        
        enum RelicType { ReturnToSenderStamp, HardHats };
        [SerializeField] RelicType relicType;

        public override void Init() {
            ObjectiveEventManager.AddListener<OnEquipmentUse>(OnEquipmentUse);
            base.Init();
        }

        protected virtual void OnEquipmentUse(OnEquipmentUse evt) {
            switch (relicType) {
                default:
                case OnDamageRelicType.SelfDestructButtons:
                    if (evt.data is HammerData && evt.target is EnemyUnit && evt.secondTarget == evt.user) {

                    }
                break;
            }

        }
    }

    }
}
