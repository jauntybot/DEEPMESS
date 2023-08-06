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

    [SerializeField] public SlotMachine slotMachine;
    [SerializeField] int slotMachineInterval;

    public bool InbetweenTrigger(int index) {
        return index%slotMachineInterval == 0 && index != 0;
    }

    public IEnumerator BetweenFloorSegment(int currentFloorIndex) {
        ScenarioManager.instance.currentTurn = ScenarioManager.Turn.Loadout;
        StartCoroutine(UIManager.instance.LoadOutScreen());
        slotMachine.gameObject.SetActive(true);
        slotMachine.Initialize(slotMachine.equipmentTable);
        
        while (ScenarioManager.instance.currentTurn != ScenarioManager.Turn.Descent) {
            yield return null;
        }
    }

}
