using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentButton : MonoBehaviour
{
    public EquipmentData data;

    [SerializeField] TMPro.TMP_Text equipmentNameText;

    public delegate void OnEquipmentUpdate(EquipmentData equipment);
    public event OnEquipmentUpdate EquipmentSelected;

    public void Initialize(EquipmentData d, GridElement ge) {
        data = d;
        equipmentNameText.text = data.name;
        PlayerUnit unit = (PlayerUnit)ge;
        EquipmentSelected += unit.UpdateAction;
    }

    public void SelectEquipment() {
        EquipmentSelected?.Invoke(data);
    }
}
