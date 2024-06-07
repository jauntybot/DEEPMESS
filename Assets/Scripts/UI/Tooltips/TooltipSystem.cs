using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TooltipSystem : MonoBehaviour {

    public static TooltipSystem instance;
    public Tooltip tooltipBL, tooltipTR, tooltipHover;
    public UpgradeTooltip tooltipUpgrade, tooltipUpgradeCompare;
    [SerializeField] GameObject upgradeContext;
    [SerializeField] TMP_Text contextText;
    [SerializeField] Image[] contextImages;
    [SerializeField] Sprite[] contextIcons;
    public Transform upgradeContainer;
    public static TooltipTrigger activeTrigger;
    static UpgradeTooltipTrigger selectedUpgrade;

    public void Awake() {
        instance = this;
    }

    public static void ShowHover(TooltipTrigger trigger) {
        activeTrigger = trigger;
        instance.tooltipHover.SetText(trigger.content, trigger.header, false);
        instance.tooltipHover.transform.GetChild(0).gameObject.SetActive(true);
    }

    public static void SelectUpgrade(UpgradeTooltipTrigger upgrade = null) {
        selectedUpgrade = upgrade;
    }

    public static void ShowUpgradeHover(UpgradeTooltipTrigger trigger) {
        activeTrigger = trigger;
        instance.tooltipUpgrade.SetText(trigger.content, trigger.header, (trigger.slotTooltip || trigger.cardTooltip)? trigger.gearIcon : trigger.upgradeIcon);
        instance.tooltipUpgrade.transform.GetChild(0).gameObject.SetActive(true);
        instance.upgradeContext.SetActive(trigger.slottable && trigger.slotTooltip && selectedUpgrade != null);
        if (trigger.slottable && trigger.slotTooltip && selectedUpgrade != null) {
            instance.tooltipUpgradeCompare.SetText(selectedUpgrade.content, selectedUpgrade.header, selectedUpgrade.gearIcon);
            if (trigger.header != "Empty") {
                instance.contextText.text = "REPLACE";
                foreach (Image i in instance.contextImages) 
                    i.sprite = instance.contextIcons[1];
            } else {
                instance.contextText.text = "EQUIP";
                foreach (Image i in instance.contextImages) 
                    i.sprite = instance.contextIcons[0];
            }
        }

        instance.tooltipUpgradeCompare.transform.GetChild(0).gameObject.SetActive(trigger.slottable && trigger.slotTooltip && selectedUpgrade != null);
    }

    public static void ShowTR(TooltipTrigger trigger) {
        List<RuntimeAnimatorController> list = null;
        if (trigger.anim != null) {
            list = new List<RuntimeAnimatorController>{ trigger.anim };
        }
        
        activeTrigger = trigger;
        instance.tooltipTR.SetText(trigger.content, trigger.header, false, list);
        instance.tooltipTR.transform.GetChild(0).gameObject.SetActive(true);
    }

    public static void ShowBL(TooltipTrigger trigger) {
        List<RuntimeAnimatorController> list = null;
        if (trigger.anim != null) {
            list = new List<RuntimeAnimatorController>{ trigger.anim };
        }

        activeTrigger = trigger;
        instance.tooltipBL.SetText(trigger.content, trigger.header, false, list);
        instance.tooltipBL.transform.GetChild(0).gameObject.SetActive(true);

        if (trigger is GearTooltipTrigger tr) {
            if (tr.equip is SlagGearData data ) { //&& data.orderedUpgrades.Count > 0
                instance.upgradeContainer.gameObject.SetActive(true);
                for (int i = 0; i <= data.slottedUpgrades.Count - 1; i++) {
                    if (data.slottedUpgrades[i] != null) {
                        instance.upgradeContainer.GetChild(i).gameObject.SetActive(true);
                        UpgradeTooltip tt = instance.upgradeContainer.GetChild(i).GetComponent<UpgradeTooltip>();
                        tt.SetText(data.slottedUpgrades[i].description, data.slottedUpgrades[i].name, data.slottedUpgrades[i].icon);
                    } else
                        instance.upgradeContainer.GetChild(i).gameObject.SetActive(false);
                }
            } else 
                instance.upgradeContainer.gameObject.SetActive(false);
        }
    }

    public static void Hide() {
        instance.tooltipBL.transform.GetChild(0).gameObject.SetActive(false);
        instance.upgradeContainer.gameObject.SetActive(false);
        instance.tooltipTR.transform.GetChild(0).gameObject.SetActive(false);
        instance.tooltipHover.transform.GetChild(0).gameObject.SetActive(false);
        instance.tooltipUpgrade.transform.GetChild(0).gameObject.SetActive(false);
        instance.tooltipUpgradeCompare.transform.GetChild(0).gameObject.SetActive(false);
        instance.upgradeContext.SetActive(false);
        activeTrigger = null;
    }

}
