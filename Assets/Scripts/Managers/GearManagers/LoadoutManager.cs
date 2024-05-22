using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadoutManager : MonoBehaviour
{

//     [SerializeField] GameObject loadoutPrefab;
//     [SerializeField] Transform loadoutUIParent;
//     public Unit[] unitPrefabs;
//     public List<UnitLoadoutUI> unitLoadoutUIs;
//     [SerializeField] Button initialDescentButton;

//     public IEnumerator Initialize(List<Unit> units, bool loadout = false) {
//         unitLoadoutUIs = new();
//         yield return null;
//         ScenarioManager.instance.currentTurn = ScenarioManager.Turn.Loadout;
// // Create unit loadout UIs
//         foreach(Unit u in units) {
//             UnitLoadoutUI ui = Instantiate(loadoutPrefab, loadoutUIParent).GetComponent<UnitLoadoutUI>();
//             ui.Initialize(u);
//             ui.ToggleUnitPanel(true);
//             if (ui.unit.equipment.Find(e => e is HammerData)) {
//                 EquipmentButton button = ui.equipButtons[1];
//                 ui.equipButtons.Remove(button);
//                 Destroy(button.gameObject);
//             }
//             ui.ToggleEquipmentPanel(true);
//             unitLoadoutUIs.Add(ui);
//         }
// // Initialize loadoutbuttons and subscribe their onClicks
//         foreach (UnitLoadoutUI ui in unitLoadoutUIs) {
//             for (int i = 0; i <= ui.equipmentOptions.transform.childCount - 1; i++) {
//                 Button b = ui.equipmentOptions.transform.GetChild(i).GetComponent<Button>();
//                 //for (int e = 0; e <= unitLoadoutUIs.Count - 1; e++) 
//                     //b.onClick.AddListener(unitLoadoutUIs[e].ToggleEquipmentOptionsOff);
//             }
//             for (int e = 0; e <= unitLoadoutUIs.Count - 1; e++) {
//                 //if (unitLoadoutUIs[e] != ui)
//                     //ui.initialLoadoutButton.GetComponent<Button>().onClick.AddListener(unitLoadoutUIs[e].ToggleEquipmentOptionsOff);
//                 //initialDescentButton.onClick.AddListener(unitLoadoutUIs[e].ToggleEquipmentOptionsOff);
                
//             }
//         }
// // Start waiting coroutine
//         if (loadout)
//             yield return StartCoroutine(UIManager.instance.LoadOutScreen(true));
        
//     }

//     public void DisplayLoadout(bool first) {
//         if (first) {
//             initialDescentButton.gameObject.SetActive(true);
//             foreach (UnitLoadoutUI ui in unitLoadoutUIs) {
//                 ui.initialLoadoutButton.SetActive(first);
//                 ui.slotsLoadoutButton.SetActive(!first);
//             }
//         }
//         else {
//             initialDescentButton.gameObject.SetActive(false);
//             foreach (UnitLoadoutUI ui in unitLoadoutUIs) {
//                 ui.initialLoadoutButton.SetActive(first);
//                 ui.slotsLoadoutButton.SetActive(!first);
//             }
//         }
//     }

}
