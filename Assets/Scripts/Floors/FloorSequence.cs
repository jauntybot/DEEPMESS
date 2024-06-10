using System.Collections;
using System.Collections.Generic;
using UnityEditor.XR;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "Floors/Floor Sequence")]
[System.Serializable]
public class FloorSequence : ScriptableObject {

    public List<FloorChunk> packets;
    [SerializeField] List<FloorChunk> localPackets;
    public FloorChunk bossPacket;
    public void AddPacket(FloorChunk packet) { localPackets.Add(packet); }
    public FloorChunk activePacket;

    public int floorsTutorial;
    public Unit elitePrefab, bossPrefab;
    public FloorChunk.PacketType currentThreshold;
    public int floorsGot = 0;

    public void Init(int index) {
        localPackets = new List<FloorChunk>();
        foreach (FloorChunk packet in packets) localPackets.Add(packet);
        activePacket.floors = new List<FloorDefinition>();
        
        floorsGot = 0;
        switch (index) {
            case 0: currentThreshold = FloorChunk.PacketType.Tutorial; break;
            case 1: currentThreshold = FloorChunk.PacketType.I; break;
            case 2: currentThreshold = FloorChunk.PacketType.II; break;
            case 3: currentThreshold = FloorChunk.PacketType.III; break;
            case 4: currentThreshold = FloorChunk.PacketType.BOSS; break;
        }
        activePacket.packetType = currentThreshold;
    }

    public List<FloorChunk> RandomNodes(int count) {
        List<FloorChunk> options = new();
        for (int i = localPackets.Count - 1; i >= 0; i--) 
            if (localPackets[i].packetType == currentThreshold) options.Add(localPackets[i]);

        List<FloorChunk> rnd = new();
        //bool hazard = false;
        int m = (options.Count < count) ? options.Count : count;
        for (int c = 0; c < m; c++) {
            int index = Random.Range(0, options.Count);
            // if (options[index].packetMods.Count > 0 && !hazard) {
            //     rnd.Add(options[index]);
            //     hazard = true;
                
            // } else 
            rnd.Add(options[index]);
            
            options.RemoveAt(index);
        }

        return rnd;
    }

    public void StartPacket(FloorChunk packet) {
// Replace active packet params manually
        activePacket.packetType = packet.packetType;
        activePacket.inOrder = true;
        activePacket.minEnemies = packet.minEnemies;
        activePacket.packetLength = packet.packetLength;
        // activePacket.nuggets = packet.nuggets;
        // activePacket.relics = packet.relics;

        activePacket.packetMods = new(packet.packetMods);
        activePacket.hazardFloors = new(packet.hazardFloors);
        activePacket.eliteSpawn = false;
        activePacket.evtOffset = packet.evtOffset;
        //activePacket.objectives = new(packet.objectives);
        activePacket.firstFloors = new(packet.firstFloors);
        //activePacket.floors = new(packet.floors);


        if (packet.packetType != FloorChunk.PacketType.BOSS)
            localPackets.Remove(packet);

// Assign hazard floor indexes
        List<int> hazardIndex = new();
        if (packet.packetMods.Contains(FloorChunk.PacketMods.Hazard)) {
            for (int i = 1; i <= packet.packetLength - 1; i++) hazardIndex.Add(i);
            ShuffleBag<int> rndIndex = new(hazardIndex.ToArray());
            hazardIndex = new();
            for (int i = 1; i <= (packet.packetLength - 1)/2; i++) {
                hazardIndex.Add(rndIndex.Next());
            }
        }
        ShuffleBag<FloorDefinition> rndHazardOrder = new(packet.hazardFloors.ToArray());
        
        ShuffleBag<FloorDefinition> rndOrder = new(packet.floors.ToArray());
        for (int i = 0; i <= packet.packetLength - 2; i++) {
            if (hazardIndex.Contains(i)) 
                activePacket.floors.Add(rndHazardOrder.Next());
            else
                activePacket.floors.Add(rndOrder.Next());
        }
        rndOrder = new(packet.firstFloors.ToArray());
        activePacket.firstFloors = new List<FloorDefinition> { rndOrder.Next() };

        floorsGot = 0;
    }

// Returns true if threshold is passed
    public bool ThresholdCheck() {  
        FloorChunk.PacketType prevThreshold = currentThreshold;
        switch(prevThreshold) {
            case FloorChunk.PacketType.Tutorial:
                if (floorsGot >= activePacket.packetLength) currentThreshold = FloorChunk.PacketType.I;
            break;
            case FloorChunk.PacketType.I:
                if (floorsGot >= activePacket.packetLength) currentThreshold = FloorChunk.PacketType.II;
            break;
            case FloorChunk.PacketType.II:
                if (floorsGot >= activePacket.packetLength) currentThreshold = FloorChunk.PacketType.BOSS;
            break;
            case FloorChunk.PacketType.BOSS:
                if (FloorManager.instance.floorSequence.activePacket.eliteSpawn && !FloorManager.instance.currentFloor.gridElements.Find(u => u is BossUnit)) currentThreshold = FloorChunk.PacketType.BARRIER;
            break;
        }

        if (currentThreshold != prevThreshold) {
            return true;
        }
        return false;
    }

    public FloorDefinition GetFloor(bool first = false) {
// First floor of packet
        FloorDefinition floor;
        if (first && currentThreshold != FloorChunk.PacketType.BOSS) {
            floor = activePacket.firstFloors[0];
            activePacket.firstFloors.Remove(floor);

            floorsGot += 1;
            return floor;
// Middle of packet
        } else {
            int index = 0;
//             if (!activePacket.inOrder) {
//                 if (activePacket.packetMods.Contains(FloorChunk.PacketMods.Hazard)) {
// // Calculate number of hazard floors, half of chunk
//                     int hazardTotal = Mathf.RoundToInt(activePacket.packetLength/2);
//                     if (hazardFloorsGot < hazardTotal) {
//                         int odds = Random.Range(0, hazardTotal - floorsGot - 1);
//                         if (odds <= 0) {
//                             floor = activePacket.hazardFloors[Random.Range(0, activePacket.hazardFloors.Count)];
//                             activePacket.hazardFloors.Remove(floor);
//                             hazardFloorsGot++;
//                         } else {
//                             index = Random.Range(0, activePacket.floors.Count);
//                             floor = activePacket.floors[index];
//                             activePacket.floors.Remove(floor);
//                         }
//                     } else {
//                         index = Random.Range(0, activePacket.floors.Count);
//                         floor = activePacket.floors[index];
//                         activePacket.floors.Remove(floor);
//                     }
//                 } else {
//                     index = Random.Range(0, activePacket.floors.Count);
//                     floor = activePacket.floors[index];
//                     activePacket.floors.Remove(floor);
//                 }
//             } else {
                floor = activePacket.floors[index];
                activePacket.floors.Remove(floor);
            //}
            
            floorsGot += 1;
        }
// Spawn a beacon if floor count is mod 4
        floor.spawnBeacon = floorsGot%4 == 0;
        if (!activePacket.packetMods.Contains(FloorChunk.PacketMods.Elite) && floorsGot%3 == 0 && 
        (((activePacket.packetMods.Contains(FloorChunk.PacketMods.Hazard) || activePacket.packetMods.Contains(FloorChunk.PacketMods.Extreme)) && ((currentThreshold == FloorChunk.PacketType.I && floorsGot >= 4) || currentThreshold == FloorChunk.PacketType.II)) ||
        (floorsGot >= 4 && floorsGot <= 8)))
            floor.spawnBloatedBulb = true;
        else
            floor.spawnBloatedBulb = false;
        if (activePacket.packetMods.Contains(FloorChunk.PacketMods.Elite) && floorsGot%3 == 0 && 
        ((currentThreshold == FloorChunk.PacketType.I && floorsGot >= 4) || currentThreshold == FloorChunk.PacketType.II) ||
        (floorsGot >= 4 && floorsGot <= 8))
            floor.spawnElite = true;
        else
            floor.spawnElite = false;
        if (activePacket.packetMods.Contains(FloorChunk.PacketMods.Extreme) && floorsGot == 5)
            floor.spawnElite = true;
        else
            floor.spawnElite = false;

        return floor;
    }


}
