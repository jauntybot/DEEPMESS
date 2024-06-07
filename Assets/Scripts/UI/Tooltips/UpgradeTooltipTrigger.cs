using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UpgradeTooltipTrigger : TooltipTrigger {

    public Sprite gearIcon;
    public Sprite upgradeIcon;
    public int lvl;
    public bool slotTooltip, cardTooltip;
    [HideInInspector] public bool slottable;

    public void Initialize(GearUpgrade _upgrade) {
        if (_upgrade != null) {
            content = _upgrade.description;
            header = _upgrade.name;
            upgradeIcon = _upgrade.icon;
            gearIcon = _upgrade.modifiedGear.icon;
            lvl = _upgrade.ugpradeLevel;
        } else if (slotTooltip) {
            header = "Empty";
            content = "Empty upgrade slot.";
            upgradeIcon = null;
            gearIcon = null;
            lvl = 0;
        }
    }

    public override void OnPointerEnter(PointerEventData eventData) {
        TooltipSystem.ShowUpgradeHover(this);
    }


}
