using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadoutManager : MonoBehaviour
{


    public Unit[] unitPrefabs;
    List<UnitUI> unitUI;
    [SerializeField] TMPro.TMP_Text titleText;
    [SerializeField] Button initialDescentButton;

    public IEnumerator Initialize(List<Unit> units) {
        unitUI = new List<UnitUI>();
        yield return null;
        ScenarioManager.instance.currentTurn = ScenarioManager.Turn.Loadout;
        foreach(Unit u in units) {
            UnitUI ui = UIManager.instance.CreateLoadoutUI(u);
            ui.ToggleUnitPanel(true);
            for(int i = ui.equipment.Count - 1; i >= 0; i--) {
                if (ui.equipment[i].data is not ConsumableEquipmentData) {
                    EquipmentButton b = ui.equipment[i];
                    ui.equipment.Remove(b);
                    Destroy(b.gameObject);
                }
            }
            ui.ToggleEquipmentPanel(true);
            unitUI.Add(ui);
        }
        yield return StartCoroutine(UIManager.instance.LoadOutScreen(true));
    }

    public void DisplayLoadout(bool first) {
        if (first) {
            titleText.text = "Select Equipment";
            initialDescentButton.gameObject.SetActive(true);
            foreach (UnitUI ui in unitUI) {
                ui.initialLoadoutButton.SetActive(first);
                ui.slotsLoadoutButton.SetActive(!first);
            }
        }
        else {
            titleText.text = "Equip to Unit";
            initialDescentButton.gameObject.SetActive(false);
            foreach (UnitUI ui in unitUI) {
                ui.initialLoadoutButton.SetActive(first);
                ui.slotsLoadoutButton.SetActive(!first);
            }
        }
    }

}
