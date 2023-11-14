using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]   
public class SlagEquipmentData : EquipmentData {

    public enum UpgradePath { Power, Special, Unit };
    public Dictionary<UpgradePath, float> upgrades = new Dictionary<UpgradePath, float>{{UpgradePath.Power, 0}, {UpgradePath.Special, 0}, {UpgradePath.Unit, 0}};

    

}
