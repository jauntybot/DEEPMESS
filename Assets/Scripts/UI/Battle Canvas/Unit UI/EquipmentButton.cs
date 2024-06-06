using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentButton : MonoBehaviour {
    UnitGameUI ui;
    public GearData data;
    public enum EquipType { PerFloor, Hammer, Bulb };
    public Animator subAnim;
    [HideInInspector] public EquipType equipType;
    [HideInInspector] public Unit unit;
    
    public Button button;
    [SerializeField] Image icon;
    public delegate void OnEquipmentUpdate(GearData equipment, int rangeMod);
    private int rangeMod;
    public event OnEquipmentUpdate EquipmentSelected;


    GearTooltipTrigger tooltip;
    public bool selected;
    

    public void Initialize(UnitGameUI _ui, GearData d, GridElement ge) {
        ui = _ui;
        data = d;
        if (d is SlagGearData && d is not HammerData) equipType = EquipType.PerFloor;
        else if (d is HammerData) equipType = EquipType.Hammer;
        else if (d is BulbEquipmentData) equipType = EquipType.Bulb;
        unit = (Unit)ge;
        EquipmentSelected += unit.UpdateAction;
        if (icon)
            icon.sprite = data.icon;
        tooltip = GetComponentInChildren<GearTooltipTrigger>();
        if (tooltip)
            tooltip.Initialize(d);

        button.animator.keepAnimatorStateOnDisable = true;
            
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
        button.GetComponent<Animator>().SetBool("Disarm", true);
        
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

            button.GetComponent<Animator>().SetBool("Disarm", false);
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
