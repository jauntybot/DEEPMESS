using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipSystem : MonoBehaviour {

    public static TooltipSystem instance;
    public Tooltip tooltipBL, tooltipTR, tooltipHover;
    public Transform upgradeContainer;
    public static TooltipTrigger activeTrigger;

    public void Awake() {
        instance = this;
    }

    public static void ShowHover(TooltipTrigger trigger) {
        activeTrigger = trigger;
        instance.tooltipHover.SetText(trigger.content, trigger.header, false);
        instance.tooltipHover.transform.GetChild(0).gameObject.SetActive(true);
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

        if (trigger is TooltipEquipmentTrigger tr) {
            if (tr.equip is SlagEquipmentData data && data.orderedUpgrades.Count > 0) {
                instance.upgradeContainer.gameObject.SetActive(true);
                int  shunt = 0, scab = 0, sludge = 0;
                for (int i = 0; i < 3; i++) {
                    if (i <= data.orderedUpgrades.Count - 1) {
                        instance.upgradeContainer.GetChild(i).gameObject.SetActive(true);
                        UpgradeTooltip tt = instance.upgradeContainer.GetChild(i).GetComponent<UpgradeTooltip>();
                        string no = data.upgrades[data.orderedUpgrades[i]] == 1 ? "I" : "II";
                        string body = "";
                        int nug = 0;
                        switch (data.orderedUpgrades[i]) {
                            case SlagEquipmentData.UpgradePath.Shunt:
                                body = data.upgradeStrings.powerStrings[shunt];
                                shunt++;
                            break;
                            case SlagEquipmentData.UpgradePath.Scab:
                                body = data.upgradeStrings.specialStrings[scab];
                                nug = 1;
                                scab++;
                            break;
                            case SlagEquipmentData.UpgradePath.Sludge:
                                body = data.upgradeStrings.unitStrings[sludge];
                                nug = 2;
                                sludge++;
                            break;
                        }
                        //data.orderedUpgrades[i].ToString() + " " + no
                        tt.SetText(body, "", false, nug);
                    } else
                        instance.upgradeContainer.GetChild(i).gameObject.SetActive(false);
                }
            } else 
                instance.upgradeContainer.gameObject.SetActive(false);
        }
    }

    public static void Hide() {
        instance.tooltipBL.transform.GetChild(0).gameObject.SetActive(false);
        instance.tooltipTR.transform.GetChild(0).gameObject.SetActive(false);
        instance.tooltipHover.transform.GetChild(0).gameObject.SetActive(false);
        instance.upgradeContainer.gameObject.SetActive(false);
        activeTrigger = null;
    }

}
