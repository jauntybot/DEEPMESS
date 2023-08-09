using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadoutManager : MonoBehaviour
{

    [SerializeField] GameObject loadoutPrefab;
    [SerializeField] Transform loadoutUIParent;
    public Unit[] unitPrefabs;
    public List<UnitUI> unitUI;
    [SerializeField] Button initialDescentButton;

    public IEnumerator Initialize(List<Unit> units, bool loadout = false) {
        unitUI = new List<UnitUI>();
        yield return null;
        ScenarioManager.instance.currentTurn = ScenarioManager.Turn.Loadout;
// Create unit loadout UIs
        foreach(Unit u in units) {
            UnitUI ui = Instantiate(loadoutPrefab, loadoutUIParent).GetComponent<UnitUI>();
            ui.Initialize(u);
            ui.ToggleUnitPanel(true);
            if (ui.unit.equipment.Find(e => e is HammerData)) {
                EquipmentButton button = ui.equipButtons[1];
                ui.equipButtons.Remove(button);
                Destroy(button.gameObject);
            }
            ui.ToggleEquipmentPanel(true);
            unitUI.Add(ui);
        }
// Initialize loadoutbuttons and subscribe their onClicks
        foreach (UnitUI ui in unitUI) {
            for (int i = 0; i <= ui.equipmentOptions.transform.childCount - 1; i++) {
                Button b = ui.equipmentOptions.transform.GetChild(i).GetComponent<Button>();
                for (int e = 0; e <= unitUI.Count - 1; e++) 
                    b.onClick.AddListener(unitUI[e].ToggleEquipmentOptionsOff);
            }
            for (int e = 0; e <= unitUI.Count - 1; e++) {
                if (unitUI[e] != ui)
                    ui.initialLoadoutButton.GetComponent<Button>().onClick.AddListener(unitUI[e].ToggleEquipmentOptionsOff);
                initialDescentButton.onClick.AddListener(unitUI[e].ToggleEquipmentOptionsOff);
                
            }
        }
// Start waiting coroutine
        if (loadout)
            yield return StartCoroutine(UIManager.instance.LoadOutScreen(true));
        
    }

    public void DisplayLoadout(bool first) {
        if (first) {
            initialDescentButton.gameObject.SetActive(true);
            foreach (UnitUI ui in unitUI) {
                ui.initialLoadoutButton.SetActive(first);
                ui.slotsLoadoutButton.SetActive(!first);
            }
        }
        else {
            initialDescentButton.gameObject.SetActive(false);
            foreach (UnitUI ui in unitUI) {
                ui.initialLoadoutButton.SetActive(first);
                ui.slotsLoadoutButton.SetActive(!first);
            }
        }
    }

}
