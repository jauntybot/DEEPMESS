using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]   
public class SlagGearData : GearData {

    [Header("SLAG GEAR")]
    public PlayerUnit slag;
    public List<GearUpgrade> upgrades;
    public List<GearUpgrade> slottedUpgrades;


    public virtual void UpgradeGear(GearUpgrade upgrade, int slotIndex) {
        if (slottedUpgrades[slotIndex] != null) {
            slottedUpgrades[slotIndex].EquipDequip(false);
            slottedUpgrades[slotIndex] = null;
        }
        if (upgrade != null) {
            slottedUpgrades[slotIndex] = upgrade;
            upgrade.EquipDequip(true);
        }
    }

    public override void EquipGear(Unit user) {
        base.EquipGear(user);
        slag = (PlayerUnit)user;
        slottedUpgrades = new(){ {null}, {null}, {null} };
    }
}
