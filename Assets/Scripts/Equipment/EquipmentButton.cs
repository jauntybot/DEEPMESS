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
    [Header("Badge Count")]
    [SerializeField] GameObject badge;
    [SerializeField] TMPro.TMP_Text badgeNumber;

    public void Initialize(EquipmentData d, GridElement ge) {
        data = d;
        equipmentNameText.text = data.name;
        PlayerUnit unit = (PlayerUnit)ge;
        EquipmentSelected += unit.UpdateAction;
        bg = GetComponent<Image>();
        bg.sprite = data.icon;
        badge.SetActive(d is PlacementData);
    }

    public void SelectEquipment() {
        EquipmentSelected?.Invoke(data);
    }

    public void UpdateBadge(int num) {
        badgeNumber.text = num.ToString();
    }
}
