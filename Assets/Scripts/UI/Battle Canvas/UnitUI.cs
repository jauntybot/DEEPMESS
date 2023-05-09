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
    public GameObject portraitPanel;
    public TMPro.TMP_Text unitName;
    public Image portrait;
    public Image gfx;

    [Header("Equipment")]
    public List<EquipmentButton> equipment; 
    [SerializeField] GameObject equipmentPanel, hammerPanel, equipmentButtonPrefab, hammerButtonPrefab;

    [SerializeField] Button disarmButton;

    [Header("Loadout")]
    [SerializeField] public GameObject equipmentOptions;
    public GameObject initialLoadoutButton, slotsLoadoutButton;
    [SerializeField] public SFX equipSelectSFX, hammerSelectSFX;

    [Header("Overview")]
    [SerializeField] public UnitOverview overview;
    [SerializeField] GameObject overviewPrefab;

    public UnitUI Initialize(Unit u, Transform overviewParent = null, Transform overviewLayoutParent = null) {

        unit = u;
        unitName.text = u.name;
        portrait.sprite = u.portrait;
        if (u is PlayerUnit) {
            portrait.rectTransform.localPosition = new Vector2(-43, -86);
            portrait.rectTransform.sizeDelta = new Vector2(900, 900);
        } else {
            portrait.rectTransform.localPosition = new Vector2(0, -165);
            portrait.rectTransform.sizeDelta = new Vector2(600, 600);
        }
        gfx.sprite = u.gfx[0].sprite;

        if (u is PlayerUnit) {
            if (overviewParent != null) {
                UnitOverview view = Instantiate(overviewPrefab, overviewParent).GetComponent<UnitOverview>();
                overview = view.Initialize(u, overviewLayoutParent);
            }
            UpdateEquipmentButtons();
            ToggleEquipmentPanel(false);
        }
        ToggleUnitPanel(false);
        if (initialLoadoutButton != null) {
            initialLoadoutButton.SetActive(true); slotsLoadoutButton.SetActive(false);
            foreach (EquipmentButton b in equipment) 
                b.gameObject.GetComponent<Button>().interactable = true;
        }

        u.ElementDestroyed += UnitDestroyed;

        return this;
    }

    public void ToggleUnitPanel(bool active) {

        portraitPanel.SetActive(active);
        if (overview != null && unit is not Nail)
            overview.ToggleOverview(!active);
    }

    public void ToggleEquipmentPanel(bool active) {
        equipmentPanel.SetActive(active);

    }

    public void ToggleEquipmentButtons() {
        foreach (EquipmentButton b in equipment) {
            b.gameObject.GetComponent<Button>().interactable = (unit.energyCurrent >= b.data.energyCost && !unit.conditions.Contains(Unit.Status.Restricted));
            if (b.data is ConsumableEquipmentData && unit.usedEquip)
                b.gameObject.GetComponent<Button>().interactable = false;
        }

        if (disarmButton)
            disarmButton.gameObject.SetActive(unit.selectedEquipment != null);
        
        if (overview != null )
            overview.UpdateOverview(unit.hpCurrent);
    }

    public void DisarmButton() {
        unit.selectedEquipment.UntargetEquipment(unit);
        unit.selectedEquipment = null;
        unit.grid.DisableGridHighlight();
        disarmButton.gameObject.SetActive(false);
        if (!unit.moved)
            unit.UpdateAction(unit.equipment[0]);
        else {
            PlayerManager pManager = (PlayerManager)unit.manager;
            pManager.contextuals.displaying = false;
        }
        unit.ui.UpdateEquipmentButtons();

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
                    EquipmentButton newButt = Instantiate(equipmentButtonPrefab).GetComponent<EquipmentButton>();
                    newButt.transform.SetParent(unit.equipment[i] is HammerData ? hammerPanel.transform : equipmentPanel.transform);
                    newButt.transform.localScale = Vector3.one;
                    newButt.Initialize(unit.equipment[i], unit);
                    equipment.Add(newButt);
                }
            }
        }
        UpdateEquipmentButtonMods();
        if (overview != null )
            overview.UpdateOverview(unit.hpCurrent);
        ToggleEquipmentButtons();
    }

    private void UnitDestroyed(GridElement ge) {
        DestroyImmediate(this.gameObject);
    }

    public void ToggleEquipmentOptionsOn() {
        equipmentOptions.SetActive(true);
    }

    public void ToggleEquipmentOptionsOff() {
        equipmentOptions.SetActive(false);
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
        foreach (EquipmentButton b in equipment) 
            b.gameObject.GetComponent<Button>().interactable = true;

        for(int i = equipment.Count - 1; i >= 0; i--) {
            if (equipment[i].data is not ConsumableEquipmentData) {
                EquipmentButton b = equipment[i];
                equipment.Remove(b);
                Destroy(b.gameObject);
            }
        }
        if (overview != null )  
            overview.UpdateOverview(unit.hpCurrent);
    }

    public void SwapEquipmentFromSlots() {
        EquipmentData reward = FloorManager.instance.betweenFloor.slotMachine.selectedReward;
        if (reward != null)
            UpdateLoadout(reward);
        ToggleEquipmentButtons();
        foreach (EquipmentButton b in equipment) 
            b.gameObject.GetComponent<Button>().interactable = true;
    }

}
