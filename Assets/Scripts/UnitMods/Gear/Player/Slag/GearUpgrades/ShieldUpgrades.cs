using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Gear/Slag/Upgrades/Shield")]
[System.Serializable]
public class ShieldUpgrades : GearUpgrade {

    ShieldData shieldData;
    public enum Upgrade { Thorns, LiveWired, Aerodynamics, PinchClosed, Base, BasePlus  }
    public Upgrade upgrade;

    public override void EquipDequip(bool equip) {
        base.EquipDequip(equip);
        shieldData = (ShieldData)modifiedGear;
        switch (upgrade) {
            default: break;
            case Upgrade.Thorns:
                shieldData.thorns = equip;
            break;
            case Upgrade.LiveWired:
                shieldData.liveWired = equip;
            break;
            case Upgrade.Aerodynamics:
                shieldData.aerodynamics = equip;
            break;
            case Upgrade.PinchClosed:
                shieldData.pinchClosed = equip;
            break;
            case Upgrade.Base:
                if (equip) {
                    shieldData.activeShieldLimit++;
                    shieldData.range++;
                } else {
                    shieldData.activeShieldLimit--;
                    shieldData.range--;
                }
            break;
            case Upgrade.BasePlus:
                if (equip) {
                    shieldData.activeShieldLimit+=2;
                    shieldData.range+=2;
                } else {
                    shieldData.activeShieldLimit-=2;
                    shieldData.range-=2;
                }
            break;
        }
    }

}
