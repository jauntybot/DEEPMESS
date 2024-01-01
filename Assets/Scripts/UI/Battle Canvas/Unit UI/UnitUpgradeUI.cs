using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitUpgradeUI : UnitUI {

    UpgradeManager upgrade;
    SlagEquipmentData equip;
    [SerializeField] Button upgradeButton;
    [SerializeField] GameObject particlePrefab;
    public RectTransform slot1, slot2, slot3;
    [SerializeField] TMP_Text slot1ModifiersTMP, slot2ModifiersTMP, slot3ModifiersTMP;
    int appliedUpgrades = 0;
    [SerializeField] List<Color> particleSprites;
    SlagEquipmentData.UpgradePath activeParticle;

    [HideInInspector] public GameObject previewParticle;


    public UnitUI Initialize(Unit u, UpgradeManager _upgrade) {
        UnitUI ui = base.Initialize(u);
        equip = (SlagEquipmentData)u.equipment.Find(e => e is SlagEquipmentData && e is not HammerData);
        upgradeButton.GetComponent<UpgradeButtonHoldHandler>().Init(this);
        upgrade = _upgrade;
        appliedUpgrades = 0;
                
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
        Destroy(slot3.gameObject);
        upgrade = _upgrade;
        appliedUpgrades = 0;
        return this;
    }

    public void UpdateModifier(SlagEquipmentData.UpgradePath path) {
        ClearModifier();

        activeParticle = path;
        string mod = "";
        if (equip.totalUpgrades < 3 || equip.upgrades[path] < 2) {
            previewParticle = Instantiate(particlePrefab, CurrentSlot());
            previewParticle.GetComponentInChildren<Image>().color = new Color(particleSprites[(int)path].r, particleSprites[(int)path].g, particleSprites[(int)path].b, 0.6f);
            switch (path) {
                case SlagEquipmentData.UpgradePath.Power:
                    mod = equip.upgradeStrings.powerStrings[equip.upgrades[path]];
                break;
                case SlagEquipmentData.UpgradePath.Special:
                    mod = equip.upgradeStrings.specialStrings[equip.upgrades[path]];
                break;
                case SlagEquipmentData.UpgradePath.Unit:
                    mod = equip.upgradeStrings.unitStrings[equip.upgrades[path]];
                break;
            }
        } else mod = "MAX UPGRADES";

        TMP_Text tmp;
        switch (appliedUpgrades) {
            default:
            case 0: tmp = slot1ModifiersTMP; break;
            case 1: tmp = slot2ModifiersTMP; break;
            case 2: tmp = slot3ModifiersTMP; break;
        }
        tmp.transform.parent.gameObject.SetActive(true);
        tmp.text = mod;

        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
    }

    public RectTransform CurrentSlot() {
        switch (appliedUpgrades) {
            default:
            case 0: return slot1;
            case 1: return slot2;
            case 2: return slot3;
        }
    }

    public void ClearModifier(bool apply = false) {
        if (!apply && previewParticle) {
            Destroy(previewParticle);
        } else if (previewParticle) {
            Image image = previewParticle.GetComponentInChildren<Image>();
            image.color = new Color(image.color.r, image.color.g, image.color.b, 1);
            previewParticle = null;
        }

        TMP_Text tmp;
        switch (appliedUpgrades) {
            default:
            case 0: tmp = slot1ModifiersTMP; break;
            case 1: tmp = slot2ModifiersTMP; break;
            case 2: tmp = slot3ModifiersTMP; break;
        }
        if (!apply) {
            tmp.transform.parent.gameObject.SetActive(false);
            tmp.text = "";   
        } else {
            tmp.transform.parent.gameObject.SetActive(true);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
    }


    public void ApplyUpgrade() {
        TMP_Text tmp;
        switch (appliedUpgrades) {
            default:
            case 0: tmp = slot1ModifiersTMP; break;
            case 1: tmp = slot2ModifiersTMP; break;
            case 2: tmp = slot3ModifiersTMP; break;
        }
        ClearModifier(true);
        appliedUpgrades++;

        equip.UpgradeEquipment(activeParticle);
        upgrade.ApplyParticle();
    }




}
