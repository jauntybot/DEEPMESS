using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UpgradeManager : MonoBehaviour {

    [SerializeField] GameObject upgradePanel;
    public PlayerUnit selectedUnit;
    [SerializeField] TMP_Text unitName, upgradePointsTMP;
    int selectionIndex = 0;
    List<PlayerUnit> units = new();
    List<UnitUpgradeUI> unitUpgradeUIs;
    int upgradePoints;

    [SerializeField] List<UpgradeBranch> upgradeBranches;


    public void Init(List<Unit> _units) {
        foreach (Unit unit in _units) {
            if (unit is PlayerUnit pu)
                units.Add(pu);
        }
        foreach (UpgradeBranch branch in upgradeBranches) 
            branch.Init(this);
        
        selectedUnit = units[0];
        selectionIndex = 0;
        upgradePoints = 0;
    }

    bool upgrading;
    public IEnumerator UpgradeSequence() {
        upgrading = true;
        upgradePoints+=3;
        
        upgradePanel.SetActive(true);
        upgradePointsTMP.text = "UPGRADE POINTS: " + upgradePoints;
        
        selectedUnit = units[0];
        selectionIndex = 0;

        while (upgrading) {

            yield return null;
        }

        upgradePanel.SetActive(false);
        FloorManager.instance.ExitCavity();    
    }

    public void EndUpgradeSequence() {
        upgrading = false;
    }

    public void ScrollSelectUnit(bool forward) {
        if (forward) {
            if (selectionIndex < 2)
                selectionIndex++;
            else 
                selectionIndex = 0;
        } else {
            if (selectionIndex > 0) 
                selectionIndex--;
            else
                selectionIndex = 2;
        }
        selectedUnit = units[selectionIndex];
        unitName.text = selectedUnit.name;

        SlagEquipmentData equip = (SlagEquipmentData)selectedUnit.equipment.Find(e => e is SlagEquipmentData);
        Debug.Log(equip.name + " , " + equip.upgrades[SlagEquipmentData.UpgradePath.Power]);
        foreach (UpgradeBranch branch in upgradeBranches)
            branch.UpdateBranch(equip);
    }

    public void UpgradeUnit(SlagEquipmentData.UpgradePath upgradePath) {
        SlagEquipmentData data = (SlagEquipmentData)selectedUnit.equipment.Find(e => e is SlagEquipmentData);
        upgradePoints--;
        upgradePointsTMP.text = "UPGRADE POINTS: " + upgradePoints;
        data.UpgradeEquipment(selectedUnit, upgradePath);
    }

}
