using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]   
public class SlagEquipmentData : EquipmentData {

    public PlayerUnit slag;
    public enum UpgradePath { Shunt, Scab, Sludge };
    public Dictionary<UpgradePath, int> upgrades = new() { {UpgradePath.Shunt, 0}, {UpgradePath.Scab, 0}, {UpgradePath.Sludge, 0}};
    [HideInInspector] public List <UpgradePath> orderedUpgrades;
    public EquipmentUpgrades upgradeStrings;

    public virtual void UpgradeEquipment(UpgradePath targetPath) {
        if (upgrades[targetPath] <= 2) {
            upgrades[targetPath]++;
            orderedUpgrades.Add(targetPath);
        }
        else
            Debug.LogWarning("Upgrade path maxed out!");
    }

    public override void EquipEquipment(Unit user) {
        base.EquipEquipment(user);
        slag = (PlayerUnit)user;
        upgrades = new Dictionary<UpgradePath, int>{{UpgradePath.Shunt, -1}, {UpgradePath.Scab, -1}, {UpgradePath.Sludge, -1}};
        UpgradeEquipment(UpgradePath.Scab);
        UpgradeEquipment(UpgradePath.Shunt);
        UpgradeEquipment(UpgradePath.Sludge);
        orderedUpgrades = new();
    }

}


[System.Serializable]
public class EquipmentUpgrades {
    public List<string> powerStrings = new();
    public List<string> specialStrings = new();
    public List<string> unitStrings = new();
    
}