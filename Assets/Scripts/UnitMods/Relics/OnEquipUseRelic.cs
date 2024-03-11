using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Relics {

    [CreateAssetMenu(menuName = "Relics/On Equipment Use")]
    public class OnEquipUseRelic : RelicData {
        
        enum RelicType { PostageStamp, AluminumShaft, RibbedKeychain, RouletteWheel };
        [SerializeField] RelicType relicType;

        public override void Init() {
            ObjectiveEventManager.AddListener<OnEquipmentUse>(OnEquipmentUse);
            switch (relicType) {
                default: break;
                case RelicType.AluminumShaft:
                    PlayerManager manager = ScenarioManager.instance.player;
                    foreach(Unit u in manager.units) {
                        foreach (EquipmentData equip in u.equipment) {
                            if (equip is HammerData)
                                u.moveMod ++;
                        }
                    }
                break;
            }
            base.Init();
        }

        protected virtual void OnEquipmentUse(OnEquipmentUse evt) {
            switch (relicType) {
                default:
                case RelicType.PostageStamp:
                    if (evt.data is HammerData h) {
                        if (evt.target is EnemyUnit && evt.secondTarget == evt.user) {
                            h.dmgMod = 1;
                        }
                        else {
                            h.dmgMod = 0;
                        }
                    }
                break;
                case RelicType.AluminumShaft:
                    if (evt.data is HammerData) {
                        if (evt.user is PlayerUnit pu1) pu1.moveMod--;
                        if (evt.secondTarget is PlayerUnit pu2) pu2.moveMod++;
                    }

                break;
                case RelicType.RibbedKeychain:
                    if (evt.data is EnemyAttackData ea) {
                        if (evt.target is EnemyUnit)
                            ea.dmgMod += ea.dmg;
                        else ea.dmgMod = 0;
                    }
    
                break;
                case RelicType.RouletteWheel:
                    if (evt.data is HammerData && evt.target is Nail) {
                        int rnd = Random.Range(0,101);
                        if (rnd <= 15) {
                            evt.target.StartCoroutine(evt.target.TakeDamage(-1, GridElement.DamageType.Heal, evt.user));
                        }
                    }
                break;
            }

        }
    }
}