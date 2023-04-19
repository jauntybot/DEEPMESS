using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotMachineSlot : MonoBehaviour
{

    List<EquipmentData> equipmentTable;
    int index = 0;
    Animator anim;
    [SerializeField] Image slotSpin1, slotSpin2;
    int fastSpins;
    [SerializeField] int fastSpinCount;

    void Start() {
        anim = GetComponent<Animator>();
        anim.StopPlayback();
    }

    public void Initialize(List<EquipmentData> table, int finalIndex) {

        equipmentTable = table;
        index = finalIndex;
        fastSpins = 0;
        
    }


    public void Spin() {
        anim.SetTrigger("startSpin");
        Debug.Log(equipmentTable[index].name);
        index = index - ((fastSpinCount * 2) % equipmentTable.Count) - 2;
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
        if (fastSpins >= fastSpinCount && fastSpins%2 == 0) 
           anim.speed -= 0.25f;
        else if (anim.speed <= 0.25f) {
            anim.SetTrigger("spunOut");
            anim.speed = 1;
        }
        else
            fastSpins++;
    }
}
