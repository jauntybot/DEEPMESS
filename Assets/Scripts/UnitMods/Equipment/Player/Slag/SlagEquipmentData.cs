using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]   
public class SlagEquipmentData : EquipmentData {

    public PlayerUnit slag;
    public enum UpgradePath { Power, Special, Unit };
    public Dictionary<UpgradePath, int> upgrades = new() { {UpgradePath.Power, 0}, {UpgradePath.Special, 0}, {UpgradePath.Unit, 0}};
    public EquipmentUpgrades upgradeStrings;

    public virtual void UpgradeEquipment(Unit user, UpgradePath targetPath) {
        if (upgrades[targetPath] <= 2)
            upgrades[targetPath]++;
        else
            Debug.LogWarning("Upgrade path maxed out!");
    }

    public override void EquipEquipment(Unit user) {
        base.EquipEquipment(user);
        slag = (PlayerUnit)user;
        upgrades = new Dictionary<UpgradePath, int>{{UpgradePath.Power, -1}, {UpgradePath.Special, -1}, {UpgradePath.Unit, -1}};
        UpgradeEquipment(slag, UpgradePath.Special);
        UpgradeEquipment(slag, UpgradePath.Power);
        UpgradeEquipment(slag, UpgradePath.Unit);
    }

}


[System.Serializable]
public class EquipmentUpgrades {

    [SerializeField]
    public List<string> powerStrings = new();
    public List<string> specialStrings = new();
    public List<string> unitStrings = new();
    
}