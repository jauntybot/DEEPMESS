using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPUpgradeSlot : UpgradeSlot {

    public virtual void Init(UnitUpgradeUI _ui, bool used) {
        base.Init(_ui, null);

        selectable = true;
        ttTrigger.enabled = true;

        if (used) filled = true;
    }



    public virtual void UpdateSlot(bool state = true) {
        selectable = state && ScenarioManager.instance.player.collectedNuggets >= 3;

        selection.SetActive(!filled && selectable);
        actionPreview.gameObject.SetActive(filled || selectable);

        if (!selectable) {
            ttTrigger.enabled = false;
            buttonHold.GetComponent<Button>().interactable = false;
        } else {
            ttTrigger.enabled = true;
            buttonHold.GetComponent<Button>().interactable = true;
            //upgradePreview.sprite = upgrade.icon;
            //ttTrigger.header = upgrade.name; ttTrigger.content = upgrade.description;
        }
    }

    public override void ApplyUpgrade() {
        RectTransform rect = Instantiate(sparksPrefab, transform).GetComponent<RectTransform>();
        rect.anchoredPosition += new Vector2(25, -400);
        filled = true;

        ui.UpgradeHP();
    }
}
