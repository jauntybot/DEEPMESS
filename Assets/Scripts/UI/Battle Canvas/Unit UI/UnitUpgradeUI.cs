using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitUpgradeUI : UnitUI {

    UpgradeManager upgrade;
    SlagEquipmentData equip;
    [SerializeField] Button upgradeButton;
    [SerializeField] TMP_Text modifiersTMP, activeModifiersTMP;
    [SerializeField] GameObject particlePrefab;
    public RectTransform particlesLayout, emptyParticlesLayout;
    [SerializeField] List<Color> particleSprites;
    SlagEquipmentData.UpgradePath activeParticle;

    [HideInInspector] public GameObject previewParticle;


    public UnitUI Initialize(Unit u, UpgradeManager _upgrade) {
        UnitUI ui = base.Initialize(u);
        equip = (SlagEquipmentData)u.equipment.Find(e => e is SlagEquipmentData && e is not HammerData);
        upgradeButton.GetComponent<UpgradeButtonHoldHandler>().Init(this);
        upgrade = _upgrade;
        return ui;
    }

// Custom init function for hammer data with no persistent unit
    public UnitUI Initialize(HammerData hammer, UpgradeManager _upgrade) {
        gameObject.name = " HAMMER - Upgrade Unit UI";
        unitName.text = "HAMMER";
        equip = hammer;
        upgradeButton.GetComponent<UpgradeButtonHoldHandler>().Init(this);
        Destroy(emptyParticlesLayout.GetChild(emptyParticlesLayout.childCount - 1).gameObject);
        upgrade = _upgrade;
        return this;
    }

    public void UpdateModifier(SlagEquipmentData.UpgradePath path) {
        ClearModifier();
        activeParticle = path;
        string mod = "";
        if (equip.totalUpgrades < 3 || equip.upgrades[path] < 2) {
            previewParticle = Instantiate(particlePrefab, particlesLayout);
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
        modifiersTMP.text = mod;
    }

    public void ClearModifier(bool apply = false) {
        if (!apply && previewParticle) {
            Destroy(previewParticle);
        } else if (previewParticle) {
            Image image = previewParticle.GetComponentInChildren<Image>();
            image.color = new Color(image.color.r, image.color.g, image.color.b, 1);
            previewParticle = null;
        }
        modifiersTMP.text = "";
    }


    public void ApplyUpgrade() {
        equip.UpgradeEquipment(activeParticle);
        ClearModifier(true);

        string activeMods = "";
        for (int i = 0; i <= equip.upgrades[SlagEquipmentData.UpgradePath.Power] - 1; i++) {
            activeMods += equip.upgradeStrings.powerStrings[i] + '\n';
        }
        for (int i = 0; i <= equip.upgrades[SlagEquipmentData.UpgradePath.Special] - 1; i++) {
            activeMods += equip.upgradeStrings.specialStrings[i] + '\n';
        }
        for (int i = 0; i <= equip.upgrades[SlagEquipmentData.UpgradePath.Unit] - 1; i++) {
            activeMods += equip.upgradeStrings.unitStrings[i] + '\n';
        }
        activeModifiersTMP.text = activeMods;
        
        upgrade.ApplyParticle();
    }




}
