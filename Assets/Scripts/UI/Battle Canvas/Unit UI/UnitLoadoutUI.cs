using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitLoadoutUI : UnitUI {
    
    [Header("Loadout")]
    [SerializeField] public GameObject equipmentOptions;
    public GameObject initialLoadoutButton, slotsLoadoutButton;


    public override UnitUI Initialize(Unit u, Transform overviewParent = null, Transform overviewLayoutParent = null) {
        UnitUI unitUI = base.Initialize(u, overviewParent, overviewLayoutParent);
        
        if (initialLoadoutButton != null) {
            initialLoadoutButton.SetActive(true); 
            slotsLoadoutButton.SetActive(false);
            
            foreach (EquipmentButton button in equipButtons) {
                button.gameObject.GetComponentInChildren<Button>().interactable = true;
            }
        }

        return unitUI;
    }

    public void ToggleEquipmentOptionsOn() {
        equipmentOptions.SetActive(true);
    }

    public void ToggleEquipmentOptionsOff() {
        equipmentOptions.SetActive(false);
    }

}
