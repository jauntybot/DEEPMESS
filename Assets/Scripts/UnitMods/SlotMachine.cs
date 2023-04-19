using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotMachine : MonoBehaviour
{

    [SerializeField] List<EquipmentData> equipmentTable;
    [SerializeField] List<SlotMachineSlot> slots;


    public void Initialize(List<EquipmentData> table) {
        foreach(SlotMachineSlot slot in slots) {
            slot.Initialize(equipmentTable, Random.Range(0, equipmentTable.Count - 1));
        }
    }

    public void SpinSlots() {
        foreach (SlotMachineSlot slot in slots) {
            List<EquipmentData> table = new List<EquipmentData>();
            slot.Spin();
        }
    }

}
