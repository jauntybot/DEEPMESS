using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeSlot : MonoBehaviour {

    protected UnitUpgradeUI ui;
    public bool selectable = false;
    [SerializeField] protected GameObject selection;
    GearUpgrade currentUpgrade;
    public Image actionPreview, slottedUpgrade;
    [SerializeField] Sprite placeIcon, swapIcon;
    public Image radialFill;
    public bool filled;
    [SerializeField] protected GameObject sparksPrefab;
    [SerializeField] protected UpgradeButtonHoldHandler buttonHold;
    [SerializeField] protected UpgradeTooltipTrigger ttTrigger;

    public virtual void Init(UnitUpgradeUI _ui, GearUpgrade upgrade) {
        ui = _ui;
        buttonHold.Init(this);
        UpdateSlot();
        ttTrigger.Initialize(null);

        if (upgrade != null) {
            radialFill.fillAmount = 0;
            filled = true;
            slottedUpgrade.gameObject.SetActive(true);
            slottedUpgrade.sprite = upgrade.icon;
            ttTrigger.header = upgrade.name; ttTrigger.content = upgrade.description;
            ttTrigger.Initialize(upgrade);
        }
    }

    public virtual void UpdateSlot(GearUpgrade upgrade = null) {
        currentUpgrade = upgrade;
        selectable = upgrade != null;
        selection.SetActive(upgrade != null);
        actionPreview.gameObject.SetActive(upgrade != null);

        if (!selectable) {
            buttonHold.GetComponent<Button>().interactable = false;
            ttTrigger.slottable = false;
        } else {
            buttonHold.GetComponent<Button>().interactable = true;
            actionPreview.sprite = filled? swapIcon : placeIcon;
            ttTrigger.slottable = true;
        }

    }

    public virtual void ApplyUpgrade() {
        radialFill.fillAmount = 0;
        filled = true;
        RectTransform rect = Instantiate(sparksPrefab, transform).GetComponent<RectTransform>();
        rect.anchoredPosition += new Vector2(25, -400);
        slottedUpgrade.gameObject.SetActive(true);
        slottedUpgrade.sprite = currentUpgrade.icon;

        ttTrigger.header = currentUpgrade.name; ttTrigger.content = currentUpgrade.description;
        
        ttTrigger.Initialize(currentUpgrade);
        ui.ApplyUpgrade(currentUpgrade, this);
    }
}
