using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotMachine : MonoBehaviour
{
    [SerializeField] List<EquipmentData> equipmentTable;
    public EquipmentData selectedReward;
    [SerializeField] Transform slotsContainer;
    List<SlotMachineSlot> slots;
    List<int> rolledEquipment = new List<int>();
    [SerializeField] Button spinButton, backButton;

    private void Start() {
        Initialize(equipmentTable);
    }

    public void Initialize(List<EquipmentData> table) {
        spinButton.interactable = true;
        slots = new List<SlotMachineSlot>();
        foreach(Transform child in slotsContainer) {
            slots.Add(child.GetComponent<SlotMachineSlot>());
        }
        foreach(SlotMachineSlot slot in slots) {
            slot.Initialize(this, table, Random.Range(0, equipmentTable.Count - 1));
        }
    }

    public void SpinSlots() {
        spinButton.interactable = false;
        rolledEquipment = new List<int>();
        for (int i = 0; i <= 2; i++) {
            int newEquip = Random.Range(0, equipmentTable.Count - 1);
            while (rolledEquipment.Contains(newEquip)) {
                newEquip = Random.Range(0, equipmentTable.Count - 1);
            }
            rolledEquipment.Insert(i, newEquip);
            slots[i].Spin(rolledEquipment[i]);
        }
    }

    public void SelectReward(int index) {
        selectedReward = equipmentTable[rolledEquipment[index]];
    }

    public void LoadOutEquipment() {
        ScenarioManager.instance.currentTurn = ScenarioManager.Turn.Loadout;
        StartCoroutine(UIManager.instance.LoadOutScreen());
        backButton.gameObject.SetActive(true);
    }

    public void BackToSlots() {
        ScenarioManager.instance.currentTurn = ScenarioManager.Turn.Slots;
        backButton.gameObject.SetActive(false);
    }

    public void SkipSlots() {
        ScenarioManager.instance.currentTurn = ScenarioManager.Turn.Descent;
        gameObject.SetActive(false);
    }

}
