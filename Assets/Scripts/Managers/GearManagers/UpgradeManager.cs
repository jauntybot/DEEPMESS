using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Relics;

public class UpgradeManager : MonoBehaviour {

    [SerializeField] SFX selectSFX;
    [SerializeField] Button confirmButton;
    [SerializeField] GameObject confirmDiscard;
    [SerializeField] GameObject upgradeScreen, unitUIContainer;
    [SerializeField] GameObject unitUIPrefab;
    public NuggetDisplay nuggetDisplay;

    Dictionary<SlagGearData, Dictionary<int, ShuffleBag<GearUpgrade>>> upgradePool;
    [SerializeField] GameObject scratchOffPrefab;
    ScratchOffCard activeCard;
    
    List<UnitUpgradeUI> unitUpgradeUIs = new();
    
    bool upgrading;


    public void Init(List<Unit> _units, PlayerManager _pManager) {
        
        upgradePool = new();

        for (int i = 0; i <= _units.Count - 1; i++) {
            if (_units[i] is PlayerUnit pu) {
                ShuffleBag<GearUpgrade> lvl1Bag = new();
                ShuffleBag<GearUpgrade> lvl2Bag = new();

                SlagGearData gear = (SlagGearData)pu.equipment[1];
                foreach (GearUpgrade gu in gear.upgrades) {
                    if (gu.ugpradeLevel == 1) {
                        lvl1Bag.Add(gu);
                    } else if (gu.ugpradeLevel == 2) {
                        lvl2Bag.Add(gu);
                    }
                }
                Dictionary<int, ShuffleBag<GearUpgrade>> dict = new() { { 1, lvl1Bag }, { 2, lvl2Bag }  };
                upgradePool.Add(gear, dict);

                UnitUpgradeUI ui = Instantiate(unitUIPrefab, unitUIContainer.transform).GetComponent<UnitUpgradeUI>();
                ui.Initialize(pu, this);
                unitUpgradeUIs.Add(ui);
            }
        }
    }

    bool cont = false;

    public IEnumerator UpgradeSequence() {
        upgrading = true;
        nuggetDisplay.UpdateNuggetCount();
        nuggetDisplay.gameObject.SetActive(false);

        CancelDiscard();

        RectTransform rect = unitUIContainer.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.6f, rect.anchorMin.y);
        
        upgradeScreen.SetActive(true);
        yield return StartCoroutine(ScratchOffSequence());

        while (upgrading) yield return null;

        if (activeCard) Destroy(activeCard.gameObject);
        upgradeScreen.SetActive(false); 
    }

    IEnumerator ScratchOffSequence() {
        if (activeCard != null) Destroy(activeCard.gameObject);

        List<GearUpgrade> rolledUpgrades = new();
        foreach (KeyValuePair<SlagGearData, Dictionary<int, ShuffleBag<GearUpgrade>>> entry in upgradePool) {
            rolledUpgrades.Add(DrawUpgradeOfLevel(entry.Key, 1));
        }
        

        ScratchOffCard card = Instantiate(scratchOffPrefab, upgradeScreen.transform).GetComponent<ScratchOffCard>();
        card.BuildCard(this, rolledUpgrades);
        activeCard = card;

        confirmButton.GetComponentInChildren<TMPro.TMP_Text>().text = "SCRATCH";
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(card.ScratchCard);
        confirmButton.onClick.AddListener(ScratchContinue);
        confirmButton.gameObject.SetActive(true);
        cont = false;

        
        while (upgrading && !cont) yield return null;
        if (upgrading) {
            yield return new WaitForSecondsRealtime(2f);
            
            foreach(UnitUpgradeUI ui in unitUpgradeUIs) {
                ui.hpSlot.UpdateSlot(!ui.hpSlot.filled);

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
        }
        nuggetDisplay.gameObject.SetActive(true);

        confirmButton.GetComponentInChildren<TMPro.TMP_Text>().text = "HANG UP";
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(DiscardScratchOff);
        confirmButton.gameObject.SetActive(true);
    }

    GearUpgrade DrawUpgradeOfLevel(SlagGearData gear, int lvl) {
        GearUpgrade gu = upgradePool[gear][lvl].Next();
        //upgradePool[gear][lvl].Remove(gu);
        return gu;
    }

    public void SelectUpgrade(GearUpgrade upgrade, UpgradeTooltipTrigger trigger) {
        TooltipSystem.SelectUpgrade(trigger);
        foreach (UnitUpgradeUI ui in unitUpgradeUIs) {
            if (ui.gear.GetType() == upgrade.modifiedGear.GetType()) {
                ui.SelectUpgrade(upgrade);
            } else {
                ui.SelectUpgrade();
            }
        }
    }

    public void ApplyUpgrade(GearUpgrade gu) {
        StartCoroutine(FinishScratchOff());
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(EndUpgradeSequence);
        upgradePool[gu.modifiedGear][gu.ugpradeLevel].Remove(gu);
    }

    IEnumerator FinishScratchOff() {
        if (activeCard) activeCard.DestroyCard();
        yield return new WaitForSecondsRealtime(0.5f);

        RectTransform rect = unitUIContainer.GetComponent<RectTransform>();
        float timer = 0;
        while (timer <= 0.25f) {
            rect.anchorMin = new Vector2(Mathf.Lerp(0.6f, 0.5f, timer/0.25f), rect.anchorMin.y);
            rect.anchorMax = new Vector2(Mathf.Lerp(0.6f, 0.5f, timer/0.25f), rect.anchorMin.y);
            yield return null;
            timer += Time.deltaTime;
        }
        rect.anchorMin = new Vector2(0.5f, rect.anchorMin.y);
        SelectUpgrade(null, null);
    }

    public void HPPurchased() {
        foreach (UnitUpgradeUI ui in unitUpgradeUIs) {
            ui.hpSlot.UpdateSlot(false);
        }
    }

    void ScratchContinue() {
        cont = true;
        confirmButton.gameObject.SetActive(false);
    }

    public void DiscardScratchOff() {
        confirmDiscard.SetActive(true);
    }

    public void CancelDiscard() {
        confirmDiscard.SetActive(false);
    }

    public void EndUpgradeSequence() {
        upgrading = false;
    }

}
