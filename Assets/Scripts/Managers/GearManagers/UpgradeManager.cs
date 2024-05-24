using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Relics;

public class UpgradeManager : MonoBehaviour {

    PlayerManager pManager;
    AudioSource audioS;
    [SerializeField] SFX selectSFX;
    [SerializeField] GameObject upgradeScreen, scrapButton, confirmButton, unitUIContainer;
    [SerializeField] GameObject godNuggetPrefab;
    //[SerializeField] List<SlagGearData.UpgradePath> nuggets = new();
    public NuggetButton selectedParticle;


    [SerializeField] GameObject unitUpgradeUIPrefab;
    [SerializeField] List<UnitUpgradeUI> unitUpgradeUIs = new();
    
    bool upgrading;


    public void Init(List<Unit> _units, PlayerManager _pManager) {
        audioS = GetComponent<AudioSource>();
        unitUIContainer.GetComponent<VerticalLayoutGroup>().enabled = true;

        scrapButton.SetActive(false);

        for (int i = unitUIContainer.transform.childCount - 1; i >= 0; i--)
            Destroy(unitUIContainer.transform.GetChild(i).gameObject);
        foreach (Unit unit in _units) {
            if (unit is PlayerUnit pu) {
                UnitUpgradeUI ui = Instantiate(unitUpgradeUIPrefab, unitUIContainer.transform).GetComponent<UnitUpgradeUI>();
                ui.Initialize(pu, this);
                unitUpgradeUIs.Add(ui);
            }
        }

        pManager = _pManager;      
    }

    public void CollectNugget() {

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
        

        // LayoutRebuilder.ForceRebuildLayoutImmediate(unitUIContainer.GetComponent<RectTransform>());
        // Canvas.ForceUpdateCanvases();

        while (upgrading) {

            yield return null;
        }

        upgradeScreen.SetActive(false); 
    }

    public void SelectParticle(NuggetButton nug) {
        audioS.PlayOneShot(selectSFX.Get());
        selectedParticle = nug;
        nug.frame.SetActive(true);

        //SlagGearData.UpgradePath path = (SlagGearData.UpgradePath)(int)nug.type;
        // foreach (UnitUpgradeUI ui in unitUpgradeUIs) {
        //     ui.UpdateModifier(path);
        // }

        scrapButton.SetActive(true);

        LayoutRebuilder.ForceRebuildLayoutImmediate(unitUIContainer.GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
    }

    public void ApplyParticle() {
        //nuggets.Remove(selectedParticle.type);
        Destroy(selectedParticle.gameObject);
        foreach(UnitUpgradeUI ui in unitUpgradeUIs)
            ui.ClearModifier();

        scrapButton.SetActive(false);
        
        RelicManager.instance.scrapValue += 33;

        LayoutRebuilder.ForceRebuildLayoutImmediate(unitUIContainer.GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
        ConfirmCheck();
    }

    public void ScrapParticle() {
        //nuggets.Remove(selectedParticle.type);
        Destroy(selectedParticle.gameObject);
        foreach (UnitUpgradeUI ui in unitUpgradeUIs) 
            ui.ClearModifier();

        scrapButton.SetActive(false);


        LayoutRebuilder.ForceRebuildLayoutImmediate(unitUIContainer.GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
        ConfirmCheck();
    }

    void ConfirmCheck() {
        // if (nuggets.Count <= 0)
        //     confirmButton.SetActive(true);
        // else
            confirmButton.SetActive(true);
    }

    public void EndUpgradeSequence() {
        upgrading = false;
    }

}
