using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeManager : MonoBehaviour {

    PlayerManager pManager;
    List<PlayerUnit> units = new();
    public PlayerUnit selectedUnit;
    
    [SerializeField] GameObject upgradePanel, particlePanel, particleContainer;
    [SerializeField] GameObject godParticleUIPrefab;
    [SerializeField] List<Sprite> particleSprites;
    [SerializeField] TMP_Text unitName, upgradePointsTMP;
    int selectionIndex = 0;
    //List<UnitUpgradeUI> unitUpgradeUIs;
    int upgradePoints;

    [SerializeField] List<UpgradeBranch> upgradeBranches;


    public void Init(List<Unit> _units, PlayerManager _pManager) {
        foreach (Unit unit in _units) {
            if (unit is PlayerUnit pu)
                units.Add(pu);
        }
        pManager = _pManager;
        
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
        
        for (int i = particleContainer.transform.childCount - 1; i >= 0; i++)
            Destroy(particleContainer.transform.GetChild(i).gameObject);
        for (int n = 0; n <= pManager.collectedParticles.Count - 1; n++) {
            GameObject newPart = Instantiate(godParticleUIPrefab, particleContainer.transform);
            newPart.GetComponentInChildren<Image>().sprite = particleSprites[(int)pManager.collectedParticles[n]];
        }

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
