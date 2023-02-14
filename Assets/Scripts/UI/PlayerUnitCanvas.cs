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
        for (int i = buttons.Count - 1; i >= 0; i--) {
            if (!unit.equipment.Find(e => e ==  buttons[i].data)) {
                EquipmentButton b = buttons[i];
                buttons.Remove(b);
                Destroy(b.gameObject);
            }
        }
        foreach (EquipmentData equip in unit.equipment) {
            if (!buttons.Find(b => b.data == equip)) {
                EquipmentButton newButt = Instantiate(equipmentButtonPrefab, equipmentDisplay.transform).GetComponent<EquipmentButton>();
                newButt.Initialize(equip, element);
                buttons.Add(newButt);
            }
        }
    }

    public void ToggleEquipmentDisplay(bool state) {
        equipmentDisplay.SetActive(state);
    }
}
