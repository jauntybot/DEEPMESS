using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Relics {
    
    [CreateAssetMenu(menuName = "Relics/On Relic Equip")]
    public class OnRelicEquipRelic : RelicData {

        enum RelicType { AutopsyReport, BulbShoot, TackleBox, FireSaleTag, TTS800 };
        [SerializeField] RelicType relicType;


        public override void Init() {
            ScenarioManager scenario = ScenarioManager.instance;
            PlayerManager manager = scenario.player;
            switch (relicType) {
                default: break;
                case RelicType.AutopsyReport:
                    manager.reviveTo = 3;
                break;
                case RelicType.BulbShoot:
                    foreach (Unit u in manager.units) {
                        if (u is PlayerUnit pu)
                            pu.bulbMod++;
                    }
                break;
                case RelicType.TackleBox:
                    scenario.tackleChance = 15;
                break;

                case RelicType.TTS800:
                    scenario.floorManager.nailTries=3;
                break;
            }

        }

        public override void UnsubRelic() {
            base.UnsubRelic();
            ScenarioManager scenario = ScenarioManager.instance;
            PlayerManager manager = scenario.player;
            switch (relicType) {
                default: break;
                case RelicType.AutopsyReport:
                    manager.reviveTo = 3;
                break;
                case RelicType.BulbShoot:
                    foreach (Unit u in manager.units) {
                        if (u is PlayerUnit pu)
                            pu.bulbMod--;
                    }
                break;
                case RelicType.TackleBox:
                    scenario.tackleChance = 0;
                break;
                case RelicType.TTS800:
                    scenario.floorManager.nailTries=0;
                break;
            }
        }
    }
}
