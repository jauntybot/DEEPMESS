using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]   
public class SlagEquipmentData : EquipmentData {

    public PlayerUnit slag;
    public enum UpgradePath { Shunt, Scab, Sludge };
    public Dictionary<UpgradePath, int> upgrades = new() { {UpgradePath.Shunt, 0}, {UpgradePath.Scab, 0}, {UpgradePath.Sludge, 0}};
    public int totalUpgrades;
    public EquipmentUpgrades upgradeStrings;

    public virtual void UpgradeEquipment(UpgradePath targetPath) {
        if (upgrades[targetPath] <= 2) {
            upgrades[targetPath]++;
            totalUpgrades++;
        }
        else
            Debug.LogWarning("Upgrade path maxed out!");
    }

    public override void EquipEquipment(Unit user) {
        base.EquipEquipment(user);
        slag = (PlayerUnit)user;
        upgrades = new Dictionary<UpgradePath, int>{{UpgradePath.Shunt, -1}, {UpgradePath.Scab, -1}, {UpgradePath.Sludge, -1}};
        totalUpgrades = 0;
        UpgradeEquipment(UpgradePath.Scab);
        UpgradeEquipment(UpgradePath.Shunt);
        UpgradeEquipment(UpgradePath.Sludge);
    }

}


[System.Serializable]
public class EquipmentUpgrades {
    public List<string> powerStrings = new();
    public List<string> specialStrings = new();
    public List<string> unitStrings = new();
    
}