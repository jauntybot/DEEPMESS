using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnitCanvas : ElementCanvas
{

    [SerializeField] GameObject equipmentDisplay, equipmentButtonPrefab;
    [SerializeField] List<EquipmentButton> buttons;

    public override void Initialize(GridElement ge) {
        base.Initialize(ge);
        UpdateEquipmentDisplay();
        ToggleEquipmentDisplay(false);
    }


    public void UpdateEquipmentDisplay() {
        PlayerUnit unit = (PlayerUnit)element;
        foreach (EquipmentData equip in unit.equipment) {
            if (!buttons.Find(b => b.data == equip)) {
                EquipmentButton newButt = Instantiate(equipmentButtonPrefab, equipmentDisplay.transform).GetComponent<EquipmentButton>();
                newButt.Initialize(equip, element);
                buttons.Add(newButt);
            }
        }
    }


    public void ToggleEquipmentDisplay(bool state) {
        if (!disable)
            equipmentDisplay.SetActive(state);
    }
}
