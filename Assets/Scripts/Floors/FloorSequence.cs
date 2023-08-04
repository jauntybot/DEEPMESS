using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Floors/Floor Sequence")]
[System.Serializable]
public class FloorSequence : ScriptableObject {

    public List<FloorPacket> packets;
    List<FloorPacket> localPackets;
    public FloorPacket activePacket;

    public int reqII, reqIII, reqBoss, reqBarrier;
    FloorPacket.PacketType currentThreshold;
    int floorsGot = 0;

    public void Init() {
        localPackets = new List<FloorPacket>();
        foreach (FloorPacket packet in packets) localPackets.Add(packet);
        activePacket.floors = new List<FloorDefinition>();
        currentThreshold = FloorPacket.PacketType.I;
        Debug.Log("init");
    }

    public void StartPacket(FloorPacket.PacketType type) {
        floorsGot = 0;
        Debug.Log("start packet");
        List<FloorPacket> options = new List<FloorPacket>();
        for (int i = localPackets.Count - 1; i >= 0; i--) 
            if (localPackets[i].packetType == type) options.Add(localPackets[i]);
        
        int index = Random.Range(0, options.Count-1);

        activePacket.packetType = options[index].packetType;

        activePacket.firstFloors = new List<FloorDefinition>();
        for (int i = options[index].firstFloors.Count - 1; i >= 0; i--) {
            activePacket.firstFloors.Add(options[index].firstFloors[i]);
        }
        activePacket.floors = new List<FloorDefinition>();
        for (int i = options[index].floors.Count - 1; i >= 0; i--) {
            activePacket.floors.Add(options[index].floors[i]);
        }

        localPackets.Remove(options[index]);
    }

// Returns true if threshold is passed
    public bool ThresholdCheck() {
        FloorPacket.PacketType prevThreshold = currentThreshold;
        switch (currentThreshold) {
            default: break;
            case FloorPacket.PacketType.Tutorial:
            break;
            case FloorPacket.PacketType.I:
                if (floorsGot >= reqII) currentThreshold = FloorPacket.PacketType.II;
            break;
            case FloorPacket.PacketType.II:
                if (floorsGot >= reqIII) currentThreshold = FloorPacket.PacketType.III;
            break;
            case FloorPacket.PacketType.III:
                if (floorsGot >= reqBoss) currentThreshold = FloorPacket.PacketType.BOSS;
            break;
            case FloorPacket.PacketType.BOSS:
                if (floorsGot >= reqBarrier) currentThreshold = FloorPacket.PacketType.BARRIER;
            break;
        }

        return currentThreshold != prevThreshold;
    }

    public FloorDefinition GetFloor() {
        ThresholdCheck();

        if (activePacket.packetType != currentThreshold || activePacket.floors.Count == 0) {
            StartPacket(currentThreshold);
            FloorDefinition firstFloor = activePacket.firstFloors[Random.Range(0, activePacket.firstFloors.Count-1)];
            activePacket.firstFloors.Remove(firstFloor);
            floorsGot += 1;
            return firstFloor;
        } else {
            FloorDefinition floor = activePacket.floors[Random.Range(0, activePacket.floors.Count-1)];
            activePacket.floors.Remove(floor);
            floorsGot += 1;
            return floor;
        }
    }


}
