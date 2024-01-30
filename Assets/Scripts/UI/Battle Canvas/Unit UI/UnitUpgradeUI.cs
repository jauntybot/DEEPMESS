using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitUpgradeUI : UnitUI {

    [SerializeField] Image gearIcon;
    public UpgradeManager upgrade;
    public SlagEquipmentData equip;
    public HPPips hpPips;
    [SerializeField] Button upgradeButton;
    [SerializeField] GameObject nuggetPrefab;
    [SerializeField] Transform slotContainer;
    public List<NuggetSlot> slots;
    int appliedUpgrades = 0;
    [SerializeField] List<Color> particleSprites;
    SlagEquipmentData.UpgradePath activeParticle;

    [HideInInspector] public GameObject previewParticle;


    public UnitUI Initialize(Unit u, UpgradeManager _upgrade) {
        UnitUI ui = base.Initialize(u);
        hpPips.Init(u);

        equip = (SlagEquipmentData)u.equipment.Find(e => e is SlagEquipmentData && e is not HammerData);
        if (gearIcon)
            gearIcon.sprite = equip.icon;
        upgradeButton.GetComponent<UpgradeButtonHoldHandler>().Init(this);
        upgrade = _upgrade;
        appliedUpgrades = 0;

        foreach (NuggetSlot slot in slots) {
            slot.radialFill.fillAmount = 0;
            slot.radialFill.GetComponent<AudioSource>().enabled = false;
            slot.DisplayPopup(false);
        }
                
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();

        return ui;
    }

// Custom init function for hammer data with no persistent unit
    public UnitUI Initialize(HammerData hammer, UpgradeManager _upgrade) {
        gameObject.name = " HAMMER - Upgrade Unit UI";
        unitName.text = "HAMMER";
        equip = hammer;
        upgradeButton.GetComponent<UpgradeButtonHoldHandler>().Init(this);
        
        foreach (NuggetSlot slot in slots) {
            slot.radialFill.fillAmount = 0;
            slot.radialFill.GetComponent<AudioSource>().enabled = false;
            slot.DisplayPopup(false);
        }

        upgrade = _upgrade;
        appliedUpgrades = 0;
        return this;
    }

    public void UpdateModifier(SlagEquipmentData.UpgradePath path) {
        if (CurrentSlot()) {
            ClearModifier();

            activeParticle = path;
            string mod = "MAXED OUT";
            if (equip.orderedUpgrades.Count < 3 || equip.upgrades[path] < 2) {
                previewParticle = Instantiate(nuggetPrefab, CurrentSlot().transform);
                Image nugget = previewParticle.GetComponentInChildren<Image>();
                nugget.color = new Color(nugget.color.r, nugget.color.g, nugget.color.b, 0.6f);
                Animator anim = previewParticle.GetComponentInChildren<Animator>();
                switch (path) {
                    case SlagEquipmentData.UpgradePath.Shunt:
                        if ((equip is not HammerData && equip.upgrades[path] < 2) || (equip is HammerData && equip.upgrades[path] < 1))
                            mod = equip.upgradeStrings.powerStrings[equip.upgrades[path]];
                        anim.SetInteger("Color", 0);
                    break;
                    case SlagEquipmentData.UpgradePath.Scab:
                        if ((equip is not HammerData && equip.upgrades[path] < 2) || (equip is HammerData && equip.upgrades[path] < 1))
                            mod = equip.upgradeStrings.specialStrings[equip.upgrades[path]];
                        anim.SetInteger("Color", 1);
                    break;
                    case SlagEquipmentData.UpgradePath.Sludge:
                        if ((equip is not HammerData && equip.upgrades[path] < 2) || (equip is HammerData && equip.upgrades[path] < 1))
                            mod = equip.upgradeStrings.unitStrings[equip.upgrades[path]];
                        anim.SetInteger("Color", 2);
                    break;
                }
                anim.keepAnimatorStateOnDisable = true;
            }

            string no = "";
            if (mod != "MAXED OUT") {
                switch(equip.upgrades[path]) {
                    default:
                    case 0: no = "I"; break;
                    case 1: no = "II"; break;
                    case 2: no = ""; break;
                }
            }

            
            CurrentSlot().UpdateModifier(path.ToString().ToUpper() + " " + no, mod);

            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            Canvas.ForceUpdateCanvases();
        }
    }

    public NuggetSlot CurrentSlot() {
        if ((equip is not HammerData && appliedUpgrades < 3) || (equip is HammerData && appliedUpgrades < 2))
            return slots[appliedUpgrades];
        return null;
    }

    public void ClearModifier(bool apply = false) {
        if (!apply && previewParticle) {
            Destroy(previewParticle);
        } else if (previewParticle) {
            Image image = previewParticle.GetComponentInChildren<Image>();
            image.color = new Color(image.color.r, image.color.g, image.color.b, 1);
            previewParticle = null;
        }

        if (!apply && CurrentSlot()) 
            CurrentSlot().UpdateModifier("", "");
        else if (CurrentSlot()) {
            CurrentSlot().FillSlot();
        }
        if (hpPips) 
            //hpPips.UpdatePips();

        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
    }


    public void ApplyUpgrade() {
        ClearModifier(true);
        appliedUpgrades++;

        equip.UpgradeEquipment(activeParticle);
        upgrade.ApplyParticle();
    }




}
