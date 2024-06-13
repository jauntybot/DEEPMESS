using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Gear/Slag/Upgrades/Big Grab")]
[System.Serializable]
public class BigGrabUpgrades : GearUpgrade {

    BigGrabData bigGrabData;
    public enum Upgrade { Impact, Maim, FlexibleSlime, Landscaper, SiesmicLanding, ThrowSelf, Base, BasePlus }
    public Upgrade upgrade;

    public override void EquipDequip(bool equip) {
        base.EquipDequip(equip);
        bigGrabData = (BigGrabData)modifiedGear;
        switch (upgrade) {
            default: break;
            case Upgrade.Impact:
                bigGrabData.impact = equip;
            break;
            case Upgrade.Maim:
                bigGrabData.maim = equip;
            break;
            case Upgrade.FlexibleSlime:
                bigGrabData.flexibleSlime = equip;
            break;
            case Upgrade.Landscaper:
                if (equip) {
                    foreach (GridElement ge in bigGrabData.upgradeTargets)
                        bigGrabData.firstTargets.Add(ge);
                } else {
                    foreach (GridElement ge in bigGrabData.upgradeTargets) {
                        if (bigGrabData.firstTargets.Contains(ge))
                            bigGrabData.firstTargets.Remove(ge);
                    }
                }
            break;
            case Upgrade.SiesmicLanding:
                bigGrabData.seismicLanding = true;
            break;
            case Upgrade.ThrowSelf:
                bigGrabData.throwSelf = true;
            break;
            case Upgrade.Base:
                if (equip) {
                    bigGrabData.range++;
                    bigGrabData.grabRange++;
                } else {
                    bigGrabData.range--;
                    bigGrabData.grabRange--;
                }
            break;
            case Upgrade.BasePlus:
                if (equip) {
                    bigGrabData.range+=2;
                    bigGrabData.grabRange+=2;
                } else {
                    bigGrabData.range-=2;
                    bigGrabData.grabRange-=2;
                }
            break;
        }

    }

}
