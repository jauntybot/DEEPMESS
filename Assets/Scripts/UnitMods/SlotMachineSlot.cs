using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotMachineSlot : MonoBehaviour
{

    SlotMachine slotMachine;
    List<EquipmentData> equipmentTable;
    int index = 0, finalIndex;
    Animator anim;
    [SerializeField] Image slotSpin1, slotSpin2;
    int fastSpins;
    [SerializeField] int fastSpinCount;

    void Start() {
        anim = GetComponent<Animator>();
        anim.StopPlayback();
    }

    public void Initialize(SlotMachine slot, List<EquipmentData> table, int _finalIndex) {
        slotMachine = slot;
        equipmentTable = table;
        GetComponent<Button>().interactable = false;
    }


    public void Spin(int _finalIndex) {
        anim.SetTrigger("startSpin");
        fastSpins = 0;
        finalIndex = _finalIndex;
        RestartSpin(1);
    }
    
    public void RestartSpin(int first) {
        if (first != 1) {
            int prevIndex = index;
            index++;
            if (index >= equipmentTable.Count) index = 0;
            slotSpin2.sprite = equipmentTable[prevIndex].icon;
        }
        slotSpin1.sprite = equipmentTable[index].icon;
    }

    public void SlowSpin() {
        if (fastSpins >= fastSpinCount) 
           anim.SetTrigger("slow");
        else
            fastSpins++;
    }

    public void LastSpin() {
        int prevIndex = index;
        slotSpin2.sprite = equipmentTable[prevIndex].icon;
        index = finalIndex;
        slotSpin1.sprite = equipmentTable[index].icon;
        GetComponent<Button>().interactable = true;
    }

    public void SelectSlot() {
        slotMachine.LoadOutEquipment();

    }
}
