using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentButton : MonoBehaviour
{
    public EquipmentData data;
    bool hammer = false;
    PlayerUnit unit;

    Image bg;
    public delegate void OnEquipmentUpdate(EquipmentData equipment, int rangeMod);
    private int rangeMod;
    public event OnEquipmentUpdate EquipmentSelected;
    [Header("Badge Count")]
    [SerializeField] GameObject badge;
    [SerializeField] TMPro.TMP_Text badgeNumber;
    TooltipEquipmentTrigger tooltip;
    

    public void Initialize(EquipmentData d, GridElement ge) {
        data = d;
        if (d is HammerData) hammer = true;
        unit = (PlayerUnit)ge;
        EquipmentSelected += unit.UpdateAction;
        bg = GetComponent<Image>();
        bg.sprite = data.icon;
        tooltip = GetComponent<TooltipEquipmentTrigger>();
        if (tooltip)
            tooltip.Initialize(d.name);
        //badge.SetActive(d is ConsumableEquipmentData);
        UpdateMod();
    }

    public void ButtonEnabled() {
        GetComponent<Button>().enabled = !unit.conditions.Contains(Unit.Status.Restricted);
    }

    public void UpdateMod() {
        if (data is MoveData) {
            rangeMod = unit.moveMod;
        } else if (data is AttackData) {
            rangeMod = unit.attackMod;
        }
    }

    public void SelectEquipment() {
        EquipmentSelected?.Invoke(data, rangeMod);
        if (hammer) {
            if (unit.ui.hammerSelectSFX)
                UIManager.instance.PlaySound(unit.ui.hammerSelectSFX.Get());

        } else {
            if (unit.ui.equipSelectSFX)
                UIManager.instance.PlaySound(unit.ui.equipSelectSFX.Get());
        }
    }

    public void UpdateBadge(int num) {
        badgeNumber.text = num.ToString();
    }
}
