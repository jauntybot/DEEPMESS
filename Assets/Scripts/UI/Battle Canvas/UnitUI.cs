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
    public EquipmentButton perFloor;
    public EquipmentButton hammer, bulb;
    [SerializeField] GameObject equipmentPanel, hammerPanel, perFloorButtonPrefab, hammerButtonPrefab, bulbButtonPrefab;

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
        } else if (u is EnemyUnit) {
            portrait.rectTransform.localPosition = new Vector2(-12, -315);
            portrait.rectTransform.sizeDelta = new Vector2(900, 900);
        } else if (u is Anvil) {
            portrait.rectTransform.localPosition = new Vector3(-17, -51, 0);
            portrait.rectTransform.sizeDelta = new Vector2(500, 500);
        } else if (u is Nail) {
            portrait.rectTransform.localPosition = new Vector3(0,-65,0);
            portrait.rectTransform.sizeDelta = new Vector2(600, 600);
        }
        
        gfx.sprite = u.gfx[0].sprite;

        if (u is PlayerUnit) {
            if (overviewParent != null) {
                UnitOverview view = Instantiate(overviewPrefab, overviewParent).GetComponent<UnitOverview>();
                overview = view.Initialize(u, overviewLayoutParent);
            }
            UpdateEquipmentButtons();
           
        }
        ToggleUnitPanel(false);
        if (initialLoadoutButton != null) {
            initialLoadoutButton.SetActive(true); slotsLoadoutButton.SetActive(false);
            
            if (perFloor) perFloor.gameObject.GetComponentInChildren<Button>().interactable = true;
            if (hammer) hammer.gameObject.GetComponentInChildren<Button>().interactable = true;
            if (bulb) bulb.gameObject.GetComponentInChildren<Button>().interactable = true;
        }

        u.ElementDestroyed += UnitDestroyed;

        return this;
    }

    public void ToggleUnitPanel(bool active) {
        if (portraitPanel)
            portraitPanel.SetActive(active);

    }

    public void ToggleEquipmentPanel(bool active) {
        equipmentPanel.SetActive(active);

    }

    public void ToggleEquipmentButtons() {
        PlayerManager pManager = (PlayerManager)unit.manager;
        if (perFloor) perFloor.gameObject.GetComponentInChildren<Button>().interactable = (unit.energyCurrent >= perFloor.data.energyCost && !unit.conditions.Contains(Unit.Status.Restricted) && !pManager.unitActing);
        if (unit.usedEquip)
            perFloor.gameObject.GetComponentInChildren<Button>().interactable = false;
        if (hammer) hammer.gameObject.GetComponentInChildren<Button>().interactable = (unit.energyCurrent >= hammer.data.energyCost && !unit.conditions.Contains(Unit.Status.Restricted) && !pManager.unitActing);
        if (bulb) bulb.gameObject.GetComponentInChildren<Button>().interactable = (unit.energyCurrent >= bulb.data.energyCost && !unit.conditions.Contains(Unit.Status.Restricted) && !pManager.unitActing);

        if (overview != null )
            overview.UpdateOverview(unit.hpCurrent);
    }

    public void DisarmButton() {
        unit.selectedEquipment.UntargetEquipment(unit);
        unit.selectedEquipment = null;
        unit.grid.DisableGridHighlight();
        if (!unit.moved)
            unit.UpdateAction(unit.equipment[0]);
        else {
            unit.UpdateAction();
            PlayerManager pManager = (PlayerManager)unit.manager;
            pManager.contextuals.displaying = false;
        }
        unit.ui.UpdateEquipmentButtons();
    }

    public void UpdateEquipmentButtons() {

// Remove all buttons
        if (perFloor) {
            Destroy(perFloor.gameObject);
            perFloor = null;
        } 
        if (hammer) {
            Destroy(hammer.gameObject);
            hammer = null;
        } 
        if (bulb) {
            Destroy(bulb.gameObject);
            bulb = null;
        } 
    
// Add buttons unit owns but does not have
        for (int i = unit.equipment.Count - 1; i >= 0; i--) {
            if (unit.equipment[i] is not MoveData) {
                GameObject prefab = null;
                Transform parent = null;
                if (unit.equipment[i] is PerFloorEquipmentData) {
                    prefab = perFloorButtonPrefab;
                    parent = equipmentPanel.transform;
                } else if (unit.equipment[i] is HammerData) {
                    prefab = hammerButtonPrefab;
                    parent = hammerPanel.transform;
                } else if (unit.equipment[i] is BulbEquipmentData) {
                    prefab = bulbButtonPrefab;
                    parent = equipmentPanel.transform;
                }
                EquipmentButton newButt = Instantiate(prefab, parent).GetComponent<EquipmentButton>();
                newButt.transform.localScale = Vector3.one;
                newButt.Initialize(this, unit.equipment[i], unit);
                if (newButt.data is PerFloorEquipmentData) perFloor = newButt;
                if (newButt.data is HammerData) hammer = newButt;
                if (newButt.data is BulbEquipmentData) bulb = newButt;
            }
        }
        if (overview != null )
            overview.UpdateOverview(unit.hpCurrent);
        if (unit is PlayerUnit)
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

    public void UpdateLoadout(EquipmentData equip) {
// Remove old equipment unless the same
        if (equip is PerFloorEquipmentData) {
            for (int i = unit.equipment.Count - 1; i >= 0; i--) {
                if (unit.equipment[i] is PerFloorEquipmentData e) {
                    if (equip == e) return;
                    unit.equipment.Remove(e);
                }
            }
        } 
// Add new equipment to unit
        unit.equipment.Insert(1, equip);
        UpdateEquipmentButtons(); 
        if (perFloor) perFloor.gameObject.GetComponentInChildren<Button>().interactable = true;
        if (hammer) hammer.gameObject.GetComponentInChildren<Button>().interactable = true;
        if (bulb) bulb.gameObject.GetComponentInChildren<Button>().interactable = true;

        if (overview != null )  
            overview.UpdateOverview(unit.hpCurrent);
    }

    public void SwapEquipmentFromSlots() {
        EquipmentData reward = FloorManager.instance.betweenFloor.slotMachine.selectedReward;
        if (reward != null)
            UpdateLoadout(reward);
        ToggleEquipmentButtons();
        perFloor.gameObject.GetComponentInChildren<Button>().interactable = true;
        hammer.gameObject.GetComponentInChildren<Button>().interactable = true;
        bulb.gameObject.GetComponentInChildren<Button>().interactable = true;
    }

}
