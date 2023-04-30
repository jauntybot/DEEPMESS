using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotMachine : MonoBehaviour
{
    LoadoutManager manager;
    [SerializeField] public List<EquipmentData> equipmentTable;
    public EquipmentData selectedReward;
    UnitUI selectedUI;
    [SerializeField] Transform slotsContainer, orText;
    List<SlotMachineSlot> slots;
    List<int> rolledEquipment = new List<int>();
    [SerializeField] Button spinButton, backButton, healButton;


    public void Initialize(List<EquipmentData> table) {
        
        foreach (UnitUI ui in ScenarioManager.instance.player.loadout.unitUI) {
            ui.slotsLoadoutButton.GetComponent<Button>().onClick.AddListener(ui.SwapEquipmentFromSlots);
            ui.slotsLoadoutButton.GetComponent<Button>().onClick.AddListener(ClaimSelectedReward);
        }

        spinButton.interactable = true;
        slots = new List<SlotMachineSlot>();
        foreach(Transform child in slotsContainer) {
            slots.Add(child.GetComponent<SlotMachineSlot>());
        }
        foreach(SlotMachineSlot slot in slots) {
            slot.Initialize(this, table, Random.Range(0, equipmentTable.Count - 1));
        }
        spinButton.gameObject.SetActive(true);
        spinButton.interactable = true;
        backButton.gameObject.SetActive(false);
        healButton.gameObject.SetActive(true);
        orText.gameObject.SetActive(true);
        orText.GetComponent<TMPro.TMP_Text>().text = "- or -";
        foreach (SlotMachineSlot slot in slots) 
            slot.gameObject.SetActive(true);
    }

    public void SpinSlots() {
        spinButton.interactable = false;
        healButton.gameObject.SetActive(false);
        orText.gameObject.SetActive(false);
        rolledEquipment = new List<int>();
        for (int i = 0; i <= 2; i++) {
            int newEquip = Random.Range(0, equipmentTable.Count - 1);
            while (rolledEquipment.Contains(newEquip)) {
                newEquip = Random.Range(0, equipmentTable.Count - 1);
            }
            rolledEquipment.Insert(i, newEquip);
            slots[i].Spin(rolledEquipment[i]);
        }
        spinButton.gameObject.SetActive(false);
        orText.gameObject.SetActive(true);
        orText.GetComponent<TMPro.TMP_Text>().text = "SELECT AN EQUIPMENT";
    }

    public void SelectReward(int index, SlotMachineSlot selected) {
        selectedReward = equipmentTable[index];
        foreach (SlotMachineSlot slot in slots) 
            slot.gameObject.SetActive(slot == selected);
        backButton.gameObject.SetActive(true);
        spinButton.gameObject.SetActive(false);
        orText.gameObject.SetActive(true);
        orText.GetComponent<TMPro.TMP_Text>().text = "SELECT UNIT TO SWAP EQUIPMENT";
    }

    public void DeselectReward() {
        selectedReward = null;
        foreach (SlotMachineSlot slot in slots) 
            slot.gameObject.SetActive(true);
        backButton.gameObject.SetActive(false);
        orText.gameObject.SetActive(true);
        orText.GetComponent<TMPro.TMP_Text>().text = "SELECT AN EQUIPMENT";
    }

    void ClaimSelectedReward() {
        selectedReward = null;
        foreach (SlotMachineSlot slot in slots)
            slot.GetComponent<Button>().interactable = false;
        Invoke("SkipSlots", 1f);
    }
    public void HealAllUnits() {
        foreach (Unit u in ScenarioManager.instance.player.units) {
            if (u is Nail) 
                StartCoroutine(u.TakeDamage(-3));
            else
                StartCoroutine(u.TakeDamage(-1));
        }
        Invoke("SkipSlots", 1f);
    }

    public void SkipSlots() {
        ScenarioManager.instance.currentTurn = ScenarioManager.Turn.Descent;
        gameObject.SetActive(false);
    }


}
