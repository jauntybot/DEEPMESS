using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UpgradeManager : MonoBehaviour {

    PlayerManager pManager;
    List<PlayerUnit> units = new();
    public PlayerUnit selectedUnit;
    
    [SerializeField] GameObject upgradePanel, unitUIContainer, particlePanel, particleContainer;
    [SerializeField] GameObject godParticleUIPrefab;
    public UpgradeUIParticle selectedParticle;


    [SerializeField] GameObject unitUpgradeUIPrefab;
    List<UnitUpgradeUI> unitUpgradeUIs = new();
    


    public void Init(List<Unit> _units, PlayerManager _pManager) {
        foreach (Unit unit in _units) {
            if (unit is PlayerUnit pu) {
                units.Add(pu);
                UnitUpgradeUI ui = Instantiate(unitUpgradeUIPrefab, unitUIContainer.transform).GetComponent<UnitUpgradeUI>();
                ui.Initialize(pu);
                unitUpgradeUIs.Add(ui);
            }
        }
        pManager = _pManager;      

        selectedUnit = units[0];
    }

    bool upgrading;
    public IEnumerator UpgradeSequence() {
        upgrading = true;
        
        upgradePanel.SetActive(true);
        
// Instantiate particle UI buttons from PlayerManager
        for (int i = particleContainer.transform.childCount - 1; i >= 0; i--)
            Destroy(particleContainer.transform.GetChild(i).gameObject);
        for (int n = 0; n <= pManager.collectedParticles.Count - 1; n++) {
            UpgradeUIParticle newPart = Instantiate(godParticleUIPrefab, particleContainer.transform).GetComponent<UpgradeUIParticle>();
            newPart.Init(pManager.collectedParticles[n]);
        }
        foreach (Transform part in particleContainer.transform) {
            UpgradeUIParticle partUI = part.GetComponent<UpgradeUIParticle>();
            Button butt = part.GetComponent<Button>();
            butt.onClick.AddListener(delegate{SelectParticle(partUI);});
        }

        while (upgrading) {

            yield return null;
        }

        upgradePanel.SetActive(false);
        FloorManager.instance.ExitCavity();    
    }

    public void SelectParticle(UpgradeUIParticle part) {
        selectedParticle = part;
        foreach (UnitUpgradeUI ui in unitUpgradeUIs) {
            SlagEquipmentData.UpgradePath path = (SlagEquipmentData.UpgradePath)(int)part.type;
            SlagEquipmentData equip = (SlagEquipmentData)ui.unit.equipment.Find(e => e is SlagEquipmentData && e is not HammerData);
            switch (path) {
                case SlagEquipmentData.UpgradePath.Power:
                    ui.UpdateModifier(equip.upgradeStrings.powerStrings[equip.upgrades[path]]);
                break;
                case SlagEquipmentData.UpgradePath.Special:
                    ui.UpdateModifier(equip.upgradeStrings.specialStrings[equip.upgrades[path]]);
                break;
                case SlagEquipmentData.UpgradePath.Unit:
                    ui.UpdateModifier(equip.upgradeStrings.unitStrings[equip.upgrades[path]]);
                break;
            }
        }
    }

    public void DeselectParticle() {

    }

    public void ApplyParticle() {

    }

    public void EndUpgradeSequence() {
        upgrading = false;
    }

    public void UpgradeUnit(SlagEquipmentData.UpgradePath upgradePath) {
        SlagEquipmentData data = (SlagEquipmentData)selectedUnit.equipment.Find(e => e is SlagEquipmentData);
        data.UpgradeEquipment(selectedUnit, upgradePath);
    }

}
