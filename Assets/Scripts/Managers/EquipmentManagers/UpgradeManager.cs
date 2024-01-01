using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UpgradeManager : MonoBehaviour {

    PlayerManager pManager;
    
    [SerializeField] GameObject upgradeScreen, upgradePanel, unitUIContainer, particlePanel, particleContainer;
    [SerializeField] GameObject godParticleUIPrefab;
    List<SlagEquipmentData.UpgradePath> particles = new();
    public UpgradeUIParticle selectedParticle;


    [SerializeField] GameObject unitUpgradeUIPrefab;
    List<UnitUpgradeUI> unitUpgradeUIs = new();
    
    bool upgrading;


    public void Init(List<Unit> _units, PlayerManager _pManager) {
        for (int i = unitUIContainer.transform.childCount - 1; i >= 0; i--)
            Destroy(unitUIContainer.transform.GetChild(i).gameObject);
        foreach (Unit unit in _units) {
            if (unit is PlayerUnit pu) {
                UnitUpgradeUI ui = Instantiate(unitUpgradeUIPrefab, unitUIContainer.transform).GetComponent<UnitUpgradeUI>();
                ui.Initialize(pu, this);
                unitUpgradeUIs.Add(ui);
            }
        }
        UnitUpgradeUI hammer = Instantiate(unitUpgradeUIPrefab, unitUIContainer.transform).GetComponent<UnitUpgradeUI>();
        hammer.Initialize(_pManager.hammerActions[0], this);
        unitUpgradeUIs.Add(hammer);
        pManager = _pManager;      
    }

    public void CollectParticles(List<SlagEquipmentData.UpgradePath> _particles) {
        foreach (SlagEquipmentData.UpgradePath part in _particles) 
            particles.Add(part);
    }

    public IEnumerator UpgradeSequence() {
        upgrading = true;
        
        upgradeScreen.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(upgradePanel.GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
        
// Instantiate particle UI buttons from PlayerManager
        for (int i = particleContainer.transform.childCount - 1; i >= 0; i--)
            Destroy(particleContainer.transform.GetChild(i).gameObject);
        for (int n = 0; n <= particles.Count - 1; n++) {
            UpgradeUIParticle newPart = Instantiate(godParticleUIPrefab, particleContainer.transform).GetComponent<UpgradeUIParticle>();
            newPart.Init(particles[n]);
        }
        foreach (Transform part in particleContainer.transform) {
            UpgradeUIParticle partUI = part.GetComponent<UpgradeUIParticle>();
            Button butt = part.GetComponent<Button>();
            butt.onClick.AddListener(delegate{SelectParticle(partUI);});
        }

        while (upgrading) {

            yield return null;
        }

        upgradeScreen.SetActive(false); 
    }

    public void SelectParticle(UpgradeUIParticle part) {
        selectedParticle = part;
        SlagEquipmentData.UpgradePath path = (SlagEquipmentData.UpgradePath)(int)part.type;
        foreach (UnitUpgradeUI ui in unitUpgradeUIs) {
            ui.UpdateModifier(path);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(upgradePanel.GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
    }

    public void DeselectParticle() {

    }

    public void ApplyParticle() {
        particles.Remove(selectedParticle.type);
        Destroy(selectedParticle.gameObject);
        foreach(UnitUpgradeUI ui in unitUpgradeUIs)
            ui.ClearModifier();

        LayoutRebuilder.ForceRebuildLayoutImmediate(upgradePanel.GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
    }

    public void EndUpgradeSequence() {
        upgrading = false;
    }

}
