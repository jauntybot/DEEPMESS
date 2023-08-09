using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentButton : MonoBehaviour
{
    UnitUI ui;
    public EquipmentData data;
    public enum EquipType { PerFloor, Hammer, Bulb };
    [HideInInspector] public EquipType equipType;
    [HideInInspector] public PlayerUnit unit;
    
    [SerializeField] Button button;
    public delegate void OnEquipmentUpdate(EquipmentData equipment, int rangeMod);
    private int rangeMod;
    public event OnEquipmentUpdate EquipmentSelected;


    TooltipEquipmentTrigger tooltip;
    public bool selected;
    [SerializeField] GameObject disarmOverlay;
    

    public void Initialize(UnitUI _ui, EquipmentData d, GridElement ge) {
        ui = _ui;
        data = d;
        if (d is PerFloorEquipmentData) equipType = EquipType.PerFloor;
        else if (d is HammerData) equipType = EquipType.Hammer;
        else if (d is BulbEquipmentData) equipType = EquipType.Bulb;
        unit = (PlayerUnit)ge;
        EquipmentSelected += unit.UpdateAction;
        Image bg = GetComponentInChildren<Image>();
        bg.sprite = data.icon;
        tooltip = GetComponentInChildren<TooltipEquipmentTrigger>();
        if (tooltip)
            tooltip.Initialize(d.name);
            
        UpdateMod();
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
        disarmOverlay.SetActive(true);
        
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(DeselectEquipment);
        button.onClick.AddListener(DeselectInclusive);
        selected = true;

// SFX
        if (equipType == EquipType.Hammer) {
            if (unit.ui.hammerSelectSFX)
                UIManager.instance.PlaySound(unit.ui.hammerSelectSFX.Get());

        } else {
            if (unit.ui.equipSelectSFX)
                UIManager.instance.PlaySound(unit.ui.equipSelectSFX.Get());
        }

        unit.ui.UpdateEquipmentButtons();
    }

    public void DeselectEquipment() {
        if (button) {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(SelectEquipment);

            disarmOverlay.SetActive(false);
            selected = false;
            
            unit.ui.UpdateEquipmentButtons();
        }
    }

    public void DeselectInclusive() {
        if (!unit.moved)
            unit.UpdateAction(unit.equipment[0]);
        else {
            unit.UpdateAction();
            PlayerManager pManager = (PlayerManager)unit.manager;
            pManager.contextuals.displaying = false;
        }
    }

}
