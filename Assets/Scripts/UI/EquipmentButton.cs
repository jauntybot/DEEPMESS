using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentButton : MonoBehaviour
{
    public EquipmentData data;
    PlayerUnit unit;

    Image bg;
    public delegate void OnEquipmentUpdate(EquipmentData equipment, int rangeMod);
    private int rangeMod;
    public event OnEquipmentUpdate EquipmentSelected;
    [Header("Badge Count")]
    [SerializeField] GameObject badge;
    [SerializeField] TMPro.TMP_Text badgeNumber;
    

    public void Initialize(EquipmentData d, GridElement ge) {
        data = d;
        unit = (PlayerUnit)ge;
        EquipmentSelected += unit.UpdateAction;
        bg = GetComponent<Image>();
        bg.sprite = data.icon;
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
    }

    public void UpdateBadge(int num) {
        badgeNumber.text = num.ToString();
    }
}
