using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Floors/Floor Sequence")]
[System.Serializable]
public class FloorSequence : ScriptableObject {

    public List<FloorPacket> packets;
    [HideInInspector] public List<FloorPacket> localPackets;
    public FloorPacket activePacket;

    [HideInInspector] public int floorsTutorial;
    public int floorsI, floorsII, floorsIII, floorsBoss;
    public FloorPacket.PacketType currentThreshold;
    int floorsGot = 0;

    public void Init() {
        localPackets = new List<FloorPacket>();
        foreach (FloorPacket packet in packets) localPackets.Add(packet);
        activePacket.floors = new List<FloorDefinition>();
        
        floorsGot = 0;
        currentThreshold = FloorPacket.PacketType.Tutorial;
        if (floorsTutorial == 0) currentThreshold = FloorPacket.PacketType.I;
    }

    public void StartPacket(FloorPacket.PacketType type) {
        List<FloorPacket> options = new List<FloorPacket>();
        for (int i = localPackets.Count - 1; i >= 0; i--) 
            if (localPackets[i].packetType == type) options.Add(localPackets[i]);
        
        int index = Random.Range(0, options.Count-1);

        activePacket.packetType = options[index].packetType;

        activePacket.firstFloors = new List<FloorDefinition>();
        for (int i = 0; i <= options[index].firstFloors.Count - 1; i++) {
            activePacket.firstFloors.Add(options[index].firstFloors[i]);
        }
        activePacket.floors = new List<FloorDefinition>();
        for (int i = 0; i <= options[index].floors.Count - 1; i++) {
            activePacket.floors.Add(options[index].floors[i]);
        }

        localPackets.Remove(options[index]);
    }

// Returns true if threshold is passed
    public bool ThresholdCheck() {
        FloorPacket.PacketType prevThreshold = currentThreshold;
        switch(prevThreshold) {
            case FloorPacket.PacketType.Tutorial:
                if (floorsGot >= floorsTutorial) currentThreshold = FloorPacket.PacketType.I;
            break;
            case FloorPacket.PacketType.I:
                if (floorsGot >= floorsI) currentThreshold = FloorPacket.PacketType.II;
            break;
            case FloorPacket.PacketType.II:
                if (floorsGot >= floorsI) currentThreshold = FloorPacket.PacketType.III;
            break;
            case FloorPacket.PacketType.III:
                if (floorsGot >= floorsI) currentThreshold = FloorPacket.PacketType.BOSS;
            break;
            case FloorPacket.PacketType.BOSS:
                if (floorsGot >= floorsI) currentThreshold = FloorPacket.PacketType.BARRIER;
            break;
        }

        if (currentThreshold != prevThreshold) {
            floorsGot = 0;
            return true;
        }
        return false;
    }

    public FloorDefinition GetFloor(bool ordered = false) {
        ThresholdCheck();

        if (activePacket.packetType != currentThreshold || activePacket.floors.Count == 0) {
            StartPacket(currentThreshold);
            if (activePacket.packetType != FloorPacket.PacketType.BOSS) {
                int index = ordered ? 0 : Random.Range(0, activePacket.firstFloors.Count-1);
                FloorDefinition firstFloor = activePacket.firstFloors[index];
                activePacket.firstFloors.Remove(firstFloor);
                floorsGot += 1;
                return firstFloor;
            } else {
                int index = ordered ? 0 : Random.Range(0, activePacket.floors.Count-1);
                FloorDefinition firstFloor = activePacket.floors[index];
                activePacket.floors.Remove(firstFloor);
                floorsGot += 1;
                return firstFloor;
            }
        } else {
            int index = ordered ? 0 : Random.Range(0, activePacket.floors.Count-1);
            FloorDefinition floor = activePacket.floors[index];
            activePacket.floors.Remove(floor);
            floorsGot += 1;
            return floor;
        }
    }


}
