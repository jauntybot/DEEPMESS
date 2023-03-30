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
    [SerializeField] GameObject equipmentPanel, equipmentButtonPrefab, apPipPrefab;
    [SerializeField] Transform energy, energyContainer;
    [SerializeField] GameObject equipmentOptions;

    
    public UnitUI Initialize(Unit u) {

        unit = u;
        unitName.text = u.name;
        portrait.sprite = u.portrait;
        gfx.sprite = u.gfx[0].sprite;

        if (u is PlayerUnit) {
            UpdateEquipmentButtons();
            ToggleEquipmentPanel(false);
            if (uiType == UIType.Portrait)
                energy.gameObject.SetActive(true);
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

    public void UpdateEnergy() {
        int dif = unit.energyCurrent - energyContainer.childCount;
        for (int i = Mathf.Abs(dif); i > 0; i--) {
            if (dif < 0) {
                if (energyContainer.childCount - i >= 0)
                    DestroyImmediate(energyContainer.GetChild(energyContainer.childCount - i).gameObject);
            } else if (dif > 0) {
                Instantiate(apPipPrefab, energyContainer);
            }
        }

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
        foreach (EquipmentData equip in unit.equipment) {
            if (equipment.Find(b => b.data == equip) == null) {
                EquipmentButton newButt = Instantiate(equipmentButtonPrefab, equipmentPanel.transform).GetComponent<EquipmentButton>();
                newButt.Initialize(equip, unit);
                equipment.Add(newButt);
            }
            if (equip is ConsumableEquipmentData consume) {
                EquipmentButton b = equipment.Find(b => b.data == consume);
                PlayerUnit pu = (PlayerUnit)unit;
                b.UpdateBadge(pu.consumableCount);
                //if (place.count <= 0) b.
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
        unit.equipment.Insert(2, equip);
        unit.ui.UpdateEquipmentButtons();

        UpdateEquipmentButtons();
        for(int i = equipment.Count - 2; i >= 0; i--) {
                EquipmentButton b = equipment[i];
                equipment.Remove(b);
                Destroy(b.gameObject);
        }
    }

}
