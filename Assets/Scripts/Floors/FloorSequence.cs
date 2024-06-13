using System.Collections.Generic;
using UnityEngine;

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
            default:
            case 1: currentThreshold = FloorChunk.PacketType.I; break;
            case 2: currentThreshold = FloorChunk.PacketType.II; break;
            case 3: currentThreshold = FloorChunk.PacketType.BOSS; break;
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
        Debug.Log("Start Packet " + packet.packetType);
// Replace active packet params manually
        activePacket.packetType = packet.packetType;
        activePacket.inOrder = true;
        activePacket.minEnemies = packet.minEnemies;
        activePacket.packetLength = packet.packetLength;

        activePacket.packetMods = new(packet.packetMods);
        activePacket.hazardFloors = new(packet.hazardFloors);
        activePacket.eliteSpawn = false;

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
// Build chunk floor order rolling on hazard index
        for (int i = 1; i <= packet.packetLength - 1; i++) {
            FloorDefinition floor = null;
            if (hazardIndex.Contains(i)) 
                floor = rndHazardOrder.Next();
            else
                floor = rndOrder.Next();

// Chunk spawning rules
            if (activePacket.packetType != FloorChunk.PacketType.BOSS) {
// Spawn a beacon if floor count is mod 4
                floor.spawnBeacon = i%4 == 0;
// Bloated bulb spawn rules
                if (!activePacket.packetMods.Contains(FloorChunk.PacketMods.Elite)) {
                    if (i%3 == 0) {
                        if (activePacket.packetMods.Contains(FloorChunk.PacketMods.Hazard) || activePacket.packetMods.Contains(FloorChunk.PacketMods.Extreme)) {
                            floor.spawnBloatedBulb = true;
                        } else if (i >= 6) {
                            floor.spawnBloatedBulb = true;
                        } else 
                            floor.spawnBloatedBulb = false;
                    } else 
                        floor.spawnBloatedBulb = false;
                } else
                    floor.spawnBloatedBulb = false;
// Elite spawn rules
                if (activePacket.packetMods.Contains(FloorChunk.PacketMods.Elite)) {
                    if (i%3 == 0) {
                        floor.spawnElite = true;
                    } else 
                        floor.spawnElite = false;
                } else if (activePacket.packetMods.Contains(FloorChunk.PacketMods.Extreme) && i == 5)
                    floor.spawnElite = true;
                else
                    floor.spawnElite = false;
            }
            
            activePacket.floors.Add(floor);
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
        if (activePacket.floors.Count == 0 && activePacket.packetType == FloorChunk.PacketType.BOSS)
            StartPacket(bossPacket);
        if (first && currentThreshold != FloorChunk.PacketType.BOSS) {
            floor = activePacket.firstFloors[0];
            activePacket.firstFloors.Remove(floor);
            floor.spawnBeacon = true;

            floorsGot += 1;
            return floor;
// Middle of packet
        } else {
            int index = 0;
                floor = activePacket.floors[index];
                activePacket.floors.Remove(floor);
            
            floorsGot += 1;
        }

        return floor;
    }


}
