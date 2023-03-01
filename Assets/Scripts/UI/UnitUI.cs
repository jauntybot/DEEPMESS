using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitUI : MonoBehaviour
{

    public Unit unit;

    [Header("Canvas Elements")]
    public GameObject unitPanel;
    public TMPro.TMP_Text unitName;
    public Image portrait;
    public Image gfx;

    [Header("Equipment")]
    public List<EquipmentButton> equipment; 
    [SerializeField] GameObject equipmentPanel, equipmentButtonPrefab;
    
    public void Initialize(Unit u) {

        unit = u;
        unitName.text = u.name;
        portrait.sprite = u.portrait;
        if (u is PlayerUnit) {
            UpdateEquipmentButtons();
            ToggleEquipmentPanel(false);
        }
        ToggleUnitPanel(false);

        u.ui = this;
    }

    public void ToggleUnitPanel(bool active) {

        unitPanel.SetActive(active);

    }

    public void ToggleEquipmentPanel(bool active) {

        equipmentPanel.SetActive(active);

    }

    public void UpdateEquipmentButtons() {

// Remove buttons no longer owned by unit
        for (int i = equipment.Count - 1; i >= 0; i--) {
            EquipmentButton b = equipment[i];
            if (unit.equipment.Find(d => d == b.data) == null) {
                equipment.Remove(b);
                Destroy(b.gameObject);
            }
            
        }
// Add buttons unit owns but does not have
        foreach (EquipmentData equip in unit.equipment) {
            if (equipment.Find(b => b.data == equip) == null) {
                EquipmentButton newButt = Instantiate(equipmentButtonPrefab, equipmentPanel.transform).GetComponent<EquipmentButton>();
                newButt.Initialize(equip, unit);
                equipment.Add(newButt);
            }
            if (equip is PlacementData place) {
                EquipmentButton b = equipment.Find(b => b.data == place);
                b.UpdateBadge(place.count);
            }
        }
    }
}
