using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotMachineSlot : MonoBehaviour
{

    [SerializeField] List<EquipmentData> equipmentTable;
    int index = 0;
    [SerializeField] Image slotSpin1, slotSpin2;
    int fastSpins;
    [SerializeField] int fastSpinCount;

    public void Initialize(List<EquipmentData> table, int finalIndex) {
        equipmentTable = table;
        index = Random.Range(0, table.Count - 1);
        fastSpins = 0;
    }

    


    public void RestartSpin(int i = -1) {
        int prevIndex = index;
        index++;
        if (index >= equipmentTable.Count) index = 0;
        if (i != -1) index = i;
        slotSpin1.sprite = equipmentTable[index].icon;
        slotSpin2.sprite = equipmentTable[prevIndex].icon;
    }

    public void SlowSpin() {
        if (fastSpins >= fastSpinCount && fastSpins%2 == 0) 
            GetComponent<Animator>().speed -= .25f;
        else
            fastSpins++;
    }
}
