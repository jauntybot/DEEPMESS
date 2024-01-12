using System.Collections;
using System.Collections.Generic;
using UnityEditor.XR;
using UnityEngine;

[CreateAssetMenu(menuName = "Floors/Floor Sequence")]
[System.Serializable]
public class FloorSequence : ScriptableObject {

    public List<FloorPacket> packets;
    public List<FloorPacket> localPackets;
    public FloorPacket activePacket;

    public int floorsTutorial;
    public GameObject bossPrefab;
    public FloorPacket.PacketType currentThreshold;
    public int floorsGot = 0;

    public void Init(int index) {
        localPackets = new List<FloorPacket>();
        foreach (FloorPacket packet in packets) localPackets.Add(packet);
        activePacket.floors = new List<FloorDefinition>();
        
        floorsGot = 0;
        switch (index) {
            case 0: currentThreshold = FloorPacket.PacketType.Tutorial; break;
            case 1: currentThreshold = FloorPacket.PacketType.I; break;
            case 2: currentThreshold = FloorPacket.PacketType.II; break;
            case 3: currentThreshold = FloorPacket.PacketType.III; break;
            case 4: currentThreshold = FloorPacket.PacketType.BOSS; break;
        }
        activePacket.packetType = currentThreshold;
        StartPacket(currentThreshold);
    }

    public void StartPacket(FloorPacket.PacketType type) {
        List<FloorPacket> options = new();
        for (int i = localPackets.Count - 1; i >= 0; i--) 
            if (localPackets[i].packetType == type) options.Add(localPackets[i]);

        int index = Random.Range(0, options.Count-1);

// Replace active packet params manually
        activePacket.packetType = options[index].packetType;
        activePacket.inOrder = options[index].inOrder;
        activePacket.packetLength = options[index].packetLength;

        activePacket.firstFloors = new();
        for (int i = 0; i <= options[index].firstFloors.Count - 1; i++) 
            activePacket.firstFloors.Add(options[index].firstFloors[i]);
        activePacket.floors = new();
        for (int i = 0; i <= options[index].floors.Count - 1; i++)
            activePacket.floors.Add(options[index].floors[i]);


        if (options[index].packetType != FloorPacket.PacketType.BOSS)
            localPackets.Remove(options[index]);
    }

// Returns true if threshold is passed
    public bool ThresholdCheck() {  
        FloorPacket.PacketType prevThreshold = currentThreshold;
        switch(prevThreshold) {
            case FloorPacket.PacketType.Tutorial:
                if (floorsGot > floorsTutorial) currentThreshold = FloorPacket.PacketType.I;
            break;
            case FloorPacket.PacketType.I:
                if (floorsGot > activePacket.packetLength) currentThreshold = FloorPacket.PacketType.II;
            break;
            case FloorPacket.PacketType.II:
                if (floorsGot > activePacket.packetLength) currentThreshold = FloorPacket.PacketType.III;
            break;
            case FloorPacket.PacketType.III:
                if (floorsGot > activePacket.packetLength) currentThreshold = FloorPacket.PacketType.BOSS;
            break;
            case FloorPacket.PacketType.BOSS:
                if (FloorManager.instance.bossSpawn && !FloorManager.instance.currentFloor.gridElements.Find(u => u is BossUnit)) currentThreshold = FloorPacket.PacketType.BARRIER;
            break;
        }

        if (currentThreshold != prevThreshold) {
            floorsGot = 0;
            return true;
        }
        return false;
    }

    public FloorDefinition GetFloor() {
        ThresholdCheck();

// First floor of packet
        if (activePacket.packetType != currentThreshold || activePacket.floors.Count == 0) {
            StartPacket(currentThreshold);
// Boss override
            if (currentThreshold == FloorPacket.PacketType.BOSS) {
                int index = activePacket.inOrder ? 0 : Random.Range(0, activePacket.firstFloors.Count-1);
                FloorDefinition floor = activePacket.floors[index];
                activePacket.floors.Remove(floor);
                floorsGot += 1;
                return floor;
            } else {
                int index = activePacket.inOrder ? 0 : Random.Range(0, activePacket.firstFloors.Count-1);
                FloorDefinition firstFloor = activePacket.firstFloors[index];
                activePacket.firstFloors.Remove(firstFloor);
                floorsGot += 1;
                return firstFloor;
            }
// Middle of packet
        } else {
            int index = activePacket.inOrder ? 0 : Random.Range(0, activePacket.floors.Count-1);
            FloorDefinition floor = activePacket.floors[index];
            activePacket.floors.Remove(floor);
            floorsGot += 1;
            return floor;
        }
    }


}
