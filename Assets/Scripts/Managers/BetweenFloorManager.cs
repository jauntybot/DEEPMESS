using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BetweenFloorManager : MonoBehaviour
{
    [System.Serializable]
    public class BetweenFloor {
        public SlotMachine slotMachine;
        public int floorTrigger;
    }

    public List<BetweenFloor> betweenFloors;
    public BetweenFloor currentBetween;

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

    public IEnumerator BetweenFloorSegment(int currentFloorIndex) {
        BetweenFloor seg = betweenFloors.Find(bw => bw.floorTrigger == currentFloorIndex);
        currentBetween = seg;
        ScenarioManager.instance.currentTurn = ScenarioManager.Turn.Slots;
        seg.slotMachine.gameObject.SetActive(true);

        while (ScenarioManager.instance.currentTurn != ScenarioManager.Turn.Descent) {
            yield return null;
        }
    }

}
