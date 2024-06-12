using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitUpgradeUI : UnitUI {

    [SerializeField] Image gearIcon;
    UpgradeManager upgrade;
    public SlagGearData gear;
    public HPPips hpPips;
    public List<UpgradeSlot> slots;
    public HPUpgradeSlot hpSlot;

    public UnitUI Initialize(Unit u, UpgradeManager _upgrade) {
        UnitUI ui = base.Initialize(u);
        hpPips.Init(u);

        gear = (SlagGearData)u.equipment.Find(e => e is SlagGearData && e is not HammerData);
        if (gearIcon)
            gearIcon.sprite = gear.icon;
        
        upgrade = _upgrade;

        foreach (UpgradeSlot slot in slots) {
            slot.Init(this);
            slot.radialFill.fillAmount = 0;
            slot.radialFill.GetComponent<AudioSource>().enabled = false;
        }
        if (hpSlot) hpSlot.Init(this);
                
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();

        return ui;
    }

    public void SelectUpgrade(GearUpgrade u = null) {
        foreach(UpgradeSlot slot in slots) {
            slot.UpdateSlot(u);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
        
    }

    public void ApplyUpgrade(GearUpgrade u, UpgradeSlot slot) {
        int index = slots.IndexOf(slot);
        gear.UpgradeGear(u, index);

        foreach(UpgradeSlot s in slots) {
            s.UpdateSlot();
        }
        
        upgrade.ApplyUpgrade(u);
    }

    public void UpgradeHP() {
        unit.hpMax += 1;
        unit.elementCanvas.InstantiateMaxPips();
        //unit.ui.overview.hPPips.UpdatePips();
        unit.StartCoroutine(unit.TakeDamage(-1, GridElement.DamageType.Heal));
        upgrade.HPPurchased();
        PlayerManager pm = (PlayerManager)unit.manager;
        pm.collectedNuggets-=3;
        upgrade.nuggetDisplay.UpdateNuggetCount();
    }
}
