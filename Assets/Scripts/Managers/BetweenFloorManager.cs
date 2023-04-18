using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BetweenFloorManager : MonoBehaviour
{
    
    public class BetweenFloor {
        public SlotMachine slotMachine;
        public int floorTrigger;
    }

    public List<BetweenFloor> betweenFloors;

    public bool InbetweenTrigger(int index) {
        bool trigger = false;
        foreach (BetweenFloor bw in betweenFloors) {
            if (bw.floorTrigger == index) {
                trigger = true;
                break;
            }
        }

        return trigger;
    }

}
