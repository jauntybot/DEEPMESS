using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UpgradeManager : MonoBehaviour {

    PlayerManager pManager;
    AudioSource audioS;
    [SerializeField] SFX selectSFX;
    [SerializeField] GameObject upgradeScreen, confirmButton, unitUIContainer, nuggetContainer;
    [SerializeField] GameObject godNuggetPrefab;
    [SerializeField] List<SlagEquipmentData.UpgradePath> nuggets = new();
    public NuggetButton selectedParticle;


    [SerializeField] GameObject unitUpgradeUIPrefab, hammerUpgradeGO;
    [SerializeField] List<UnitUpgradeUI> unitUpgradeUIs = new();
    
    bool upgrading;


    public void Init(List<Unit> _units, PlayerManager _pManager) {
        audioS = GetComponent<AudioSource>();

        unitUIContainer.GetComponent<VerticalLayoutGroup>().enabled = true;
        for (int i = unitUIContainer.transform.childCount - 1; i >= 0; i--)
            Destroy(unitUIContainer.transform.GetChild(i).gameObject);
        foreach (Unit unit in _units) {
            if (unit is PlayerUnit pu) {
                UnitUpgradeUI ui = Instantiate(unitUpgradeUIPrefab, unitUIContainer.transform).GetComponent<UnitUpgradeUI>();
                ui.Initialize(pu, this);
                unitUpgradeUIs.Add(ui);
            }
        }


        UnitUpgradeUI hammer = hammerUpgradeGO.GetComponent<UnitUpgradeUI>();
        hammer.Initialize(_pManager.hammerActions[0], this);
        unitUpgradeUIs.Add(hammer);
        pManager = _pManager;      
    }

    public void CollectNugget(SlagEquipmentData.UpgradePath nugget) {
        nuggets.Add(nugget);
        
// Instantiate particle UI buttons from PlayerManager
        nuggetContainer.SetActive(true);
        for (int i = nuggetContainer.transform.childCount - 1; i >= 0; i--)
            Destroy(nuggetContainer.transform.GetChild(i).gameObject);
        for (int n = 0; n <= nuggets.Count - 1; n++) {
            NuggetButton newPart = Instantiate(godNuggetPrefab, nuggetContainer.transform).GetComponent<NuggetButton>();
            newPart.Init(nuggets[n]);
        }
        foreach (Transform nug in nuggetContainer.transform) {
            NuggetButton nugUI = nug.GetComponent<NuggetButton>();
            Button butt = nug.GetComponent<Button>();
            butt.onClick.AddListener(delegate{SelectParticle(nugUI);});
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(nuggetContainer.GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
    }

    public IEnumerator UpgradeSequence() {
        upgrading = true;
        
        upgradeScreen.SetActive(true);
        ConfirmCheck();
        LayoutRebuilder.ForceRebuildLayoutImmediate(unitUIContainer.GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
        yield return null;
        unitUIContainer.GetComponent<VerticalLayoutGroup>().enabled = false;
        
        foreach(UnitUpgradeUI ui in unitUpgradeUIs) {

            Animator anim = ui.transform.GetComponent<Animator>();
            anim.SetTrigger("SlideIn");
            yield return null;
            if (ui.hpPips)
                ui.hpPips.UpdatePips();

            float t = 0;
            while (t <= 0.125f) {
                t += Time.deltaTime;
                yield return null;
            }
        }
        foreach (Transform child in nuggetContainer.transform) 
            child.GetComponent<Button>().interactable = true;
        

        // LayoutRebuilder.ForceRebuildLayoutImmediate(unitUIContainer.GetComponent<RectTransform>());
        // Canvas.ForceUpdateCanvases();

        while (upgrading) {

            yield return null;
        }

        upgradeScreen.SetActive(false); 
        nuggetContainer.SetActive(false);
    }

    public void SelectParticle(NuggetButton nug) {
        audioS.PlayOneShot(selectSFX.Get());
        selectedParticle = nug;
        foreach (Transform child in nuggetContainer.transform)
            child.GetComponent<NuggetButton>().frame.SetActive(false);
        nug.frame.SetActive(true);

        SlagEquipmentData.UpgradePath path = (SlagEquipmentData.UpgradePath)(int)nug.type;
        foreach (UnitUpgradeUI ui in unitUpgradeUIs) {
            ui.UpdateModifier(path);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(unitUIContainer.GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
    }

    public void ApplyParticle() {
        nuggets.Remove(selectedParticle.type);
        Destroy(selectedParticle.gameObject);
        foreach(UnitUpgradeUI ui in unitUpgradeUIs)
            ui.ClearModifier();

        LayoutRebuilder.ForceRebuildLayoutImmediate(unitUIContainer.GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
        ConfirmCheck();
    }

    void ConfirmCheck() {
        if (nuggets.Count <= 0)
            confirmButton.SetActive(true);
        else
            confirmButton.SetActive(false);
    }

    public void EndUpgradeSequence() {
        upgrading = false;
    }

}
