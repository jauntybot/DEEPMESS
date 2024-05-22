using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

[CreateAssetMenu(menuName = "Gear/Slag/Upgrades/Anvil")]
[System.Serializable]
public class AnvilUpgrades : GearUpgrade {

    AnvilData anvilData;
    public enum Upgrade { ReinforcedBottom, LiveWired, Crystalize, SelfDetonate, Base, BasePlus  }
    public Upgrade upgrade;



    public override void EquipDequip(bool equip) {
        base.EquipDequip(equip);
        anvilData = (AnvilData)modifiedGear;
        switch (upgrade) {
            default: break;
            case Upgrade.ReinforcedBottom:
                anvilData.reinforcedBottom = equip;
                if (equip) 
                    anvilData.anvilHP++;
                else
                    anvilData.anvilHP--;
            break;
            case Upgrade.LiveWired:
                anvilData.liveWire = equip;
            break;
            case Upgrade.Crystalize:
                anvilData.crystalize = equip;
            break;
            case Upgrade.SelfDetonate:
                anvilData.explode = equip;
            break;
            case Upgrade.Base:
                if (equip) {
                    anvilData.anvilHP++;
                    anvilData.range++;
                } else {
                    anvilData.anvilHP--;
                    anvilData.range--;
                }
            break;
            case Upgrade.BasePlus:
                if (equip) {
                    anvilData.anvilHP+=2;
                    anvilData.range+=2;
                } else {
                    anvilData.anvilHP-=2;
                    anvilData.range-=2;
                }
            break;
        }
    }

}
