using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitUI : MonoBehaviour
{
    enum UIType {Portrait, Loadout};
    [SerializeField] UIType uiType;
    public Unit unit;

    [Header("Canvas Elements")]
    public GameObject unitPanel;
    public TMPro.TMP_Text unitName;
    public Image portrait;
    public Image gfx;

    [Header("Equipment")]
    public List<EquipmentButton> equipment; 
    [SerializeField] GameObject equipmentPanel, equipmentButtonPrefab;
    [SerializeField] GameObject equipmentOptions;

    
    public UnitUI Initialize(Unit u) {

        unit = u;
        unitName.text = u.name;
        portrait.sprite = u.portrait;
        gfx.sprite = u.gfx[0].sprite;

        if (u is PlayerUnit) {
            UpdateEquipmentButtons();
            ToggleEquipmentPanel(false);
        }
        ToggleUnitPanel(false);

        u.ElementDestroyed += UnitDestroyed;

        return this;
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
            } else {
                b.transform.parent.SetSiblingIndex(i);
            }
            
        }
// Add buttons unit owns but does not have
        for (int i = unit.equipment.Count - 1; i >= 0; i--) {
            if (unit.equipment[i] is not MoveData) {
                if (equipment.Find(b => b.data == unit.equipment[i]) == null) {
                    EquipmentButton newButt = Instantiate(equipmentButtonPrefab, equipmentPanel.transform).GetComponent<EquipmentButton>();
                    newButt.Initialize(unit.equipment[i], unit);
                    equipment.Add(newButt);
                    newButt.transform.parent.SetSiblingIndex(i);
                }
                if (unit.equipment[i] is ConsumableEquipmentData consume) {
                    EquipmentButton b = equipment.Find(b => b.data == consume);
                    PlayerUnit pu = (PlayerUnit)unit;
                    b.UpdateBadge(pu.consumableCount);
                    //if (place.count <= 0) b.
                }
            }
        }
        UpdateEquipmentButtonMods();
    }

    private void UnitDestroyed(GridElement ge) {
        DestroyImmediate(this.gameObject);
    }

    public void ToggleEquipmentOptions() {
        equipmentOptions.SetActive(!equipmentOptions.activeSelf);
    }

    public void UpdateEquipmentButtonMods() {
        foreach (EquipmentButton b in equipment) 
            b.UpdateMod();
        
    }

    public void UpdateLoadout(EquipmentData equip) {
        for (int i = unit.equipment.Count - 1; i >= 0; i--) {
            if (unit.equipment[i] is ConsumableEquipmentData e) {
                if (equip == e) return;
                unit.equipment.Remove(e);
            }
        }
        unit.equipment.Insert(1, equip);

        UpdateEquipmentButtons();
        for(int i = equipment.Count - 1; i >= 0; i--) {
            if (equipment[i].data is not ConsumableEquipmentData) {
                EquipmentButton b = equipment[i];
                equipment.Remove(b);
                Destroy(b.gameObject);
            }
        }
    }

}
