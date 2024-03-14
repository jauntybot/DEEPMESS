using System.Collections;
using System.Collections.Generic;
using UnityEditor.XR;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "Floors/Floor Sequence")]
[System.Serializable]
public class FloorSequence : ScriptableObject {

    public List<FloorPacket> packets;
    List<FloorPacket> localPackets;
    public FloorPacket bossPacket;
    public void AddPacket(FloorPacket packet) { localPackets.Add(packet); }
    public FloorPacket activePacket;

    public int floorsTutorial;
    public Unit elitePrefab, bossPrefab;
    public FloorPacket.PacketType currentThreshold;
    public int floorsGot = 0;
    int hazardFloorsGot;

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
    }

    public List<FloorPacket> RandomNodes(int count) {
        List<FloorPacket> options = new();
        for (int i = localPackets.Count - 1; i >= 0; i--) 
            if (localPackets[i].packetType == currentThreshold) options.Add(localPackets[i]);

        List<FloorPacket> rnd = new();
        for (int c = 0; c < count; c++) {
            int index = Random.Range(0, options.Count-1);
            rnd.Add(options[index]);
            options.RemoveAt(index);
        }

        return rnd;
    }

    public void StartPacket(FloorPacket packet) {
// Replace active packet params manually
        activePacket.packetType = packet.packetType;
        activePacket.inOrder = packet.inOrder;
        activePacket.packetLength = packet.packetLength;
        activePacket.nuggets = packet.nuggets;
        activePacket.relics = packet.relics;

        activePacket.packetMods = new(packet.packetMods);
        activePacket.extremeFloors = new(packet.extremeFloors);
        activePacket.eliteSpawn = false;
        activePacket.eliteRange = new(packet.eliteRange.x,packet.eliteRange.y);
        activePacket.objectives = new(packet.objectives);
        activePacket.firstFloors = new(packet.firstFloors);
        activePacket.floors = new(packet.floors);


        if (packet.packetType != FloorPacket.PacketType.BOSS)
            localPackets.Remove(packet);

        floorsGot = 0;
        hazardFloorsGot = 0;
    }

// Returns true if threshold is passed
    public bool ThresholdCheck() {  
        FloorPacket.PacketType prevThreshold = currentThreshold;
        switch(prevThreshold) {
            case FloorPacket.PacketType.Tutorial:
                if (floorsGot >= activePacket.packetLength) currentThreshold = FloorPacket.PacketType.I;
            break;
            case FloorPacket.PacketType.I:
                if (floorsGot >= activePacket.packetLength) currentThreshold = FloorPacket.PacketType.II;
            break;
            case FloorPacket.PacketType.II:
                if (floorsGot >= activePacket.packetLength) currentThreshold = FloorPacket.PacketType.III;
            break;
            case FloorPacket.PacketType.III:
                if (floorsGot >= activePacket.packetLength) currentThreshold = FloorPacket.PacketType.BOSS;
            break;
            case FloorPacket.PacketType.BOSS:
                if (FloorManager.instance.floorSequence.activePacket.eliteSpawn && !FloorManager.instance.currentFloor.gridElements.Find(u => u is BossUnit)) currentThreshold = FloorPacket.PacketType.BARRIER;
            break;
        }

        if (currentThreshold != prevThreshold) {
            return true;
        }
        return false;
    }

    public FloorDefinition GetFloor(bool first = false) {
// First floor of packet
        if (first) {
            FloorDefinition floor;
// Boss override
            if (currentThreshold == FloorPacket.PacketType.BOSS) {
                int index = activePacket.inOrder ? 0 : Random.Range(0, activePacket.firstFloors.Count);
                floor = activePacket.floors[index];
                activePacket.floors.Remove(floor);    
            } else {
                int index = activePacket.inOrder ? 0 : Random.Range(0, activePacket.firstFloors.Count);
                floor = activePacket.firstFloors[index];
                activePacket.firstFloors.Remove(floor);
            }

            floorsGot += 1;
            return floor;
// Middle of packet
        } else {
            int index = 0;
            FloorDefinition floor = null;
            if (!activePacket.inOrder) {
                if (activePacket.packetMods.Contains(FloorPacket.PacketMods.Extreme)) {
                    int hazardTotal = Mathf.RoundToInt(activePacket.packetLength/2);
                    if (hazardFloorsGot < hazardTotal) {
                        int odds = Random.Range(0, hazardTotal - floorsGot - 1);
                        if (odds <= 0) {
                            floor = activePacket.extremeFloors[Random.Range(0, activePacket.extremeFloors.Count)];
                            activePacket.extremeFloors.Remove(floor);
                            hazardFloorsGot++;
                        } else {
                            index = Random.Range(0, activePacket.floors.Count);
                            floor = activePacket.floors[index];
                            activePacket.floors.Remove(floor);
                        }
                    } else {
                        index = Random.Range(0, activePacket.floors.Count);
                        floor = activePacket.floors[index];
                        activePacket.floors.Remove(floor);
                    }
                } else {
                    index = Random.Range(0, activePacket.floors.Count);
                    floor = activePacket.floors[index];
                    activePacket.floors.Remove(floor);
                }
            } else {
                floor = activePacket.floors[index];
                activePacket.floors.Remove(floor);
            }
            
            floorsGot += 1;
            return floor;
        }
    }


}
