using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentButton : MonoBehaviour
{
    public EquipmentData data;

    [SerializeField] TMPro.TMP_Text equipmentNameText;
    Image bg;
    public delegate void OnEquipmentUpdate(EquipmentData equipment);
    public event OnEquipmentUpdate EquipmentSelected;

    public void Initialize(EquipmentData d, GridElement ge) {
        data = d;
        equipmentNameText.text = data.name;
        PlayerUnit unit = (PlayerUnit)ge;
        EquipmentSelected += unit.UpdateAction;
        bg = GetComponent<Image>();
        if (d is MoveData || d is PlacementData) bg.color = FloorManager.instance.moveColor;
        if (d is AttackData) bg.color = FloorManager.instance.attackColor;
        if (d is HammerData) bg.color = FloorManager.instance.hammerColor;
        bg.color = new Color(bg.color.r, bg.color.g, bg.color.b, 255);
    }

    public void SelectEquipment() {
        EquipmentSelected?.Invoke(data);
    }
}
