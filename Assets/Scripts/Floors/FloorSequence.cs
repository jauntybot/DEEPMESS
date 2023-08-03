using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Floors/Floor Sequence")]
[System.Serializable]
public class FloorSequence : ScriptableObject {

    public List<FloorPacket> packets;
    [SerializeField] List<FloorPacket> localPackets;
    public FloorPacket activePacket;

    public int iiThreshold, iiiThreshold, bossThreshold;
    FloorPacket.PacketType currentThreshold;

    public void Init() {
        localPackets = new List<FloorPacket>();
        foreach (FloorPacket packet in packets) localPackets.Add(packet);
        Debug.Log("init");
    }

    public void StartPacket(FloorPacket.PacketType type) {
        List<FloorPacket> options = new List<FloorPacket>();
        for (int i = localPackets.Count - 1; i >= 0; i--) 
            if (localPackets[i].packetType == type) options.Add(localPackets[i]);
        
        
        foreach (FloorPacket packet in options) Debug.Log(packet.floors[0].name);
        int index = Random.Range(0, options.Count-1);

        activePacket.packetType = options[index].packetType;
        activePacket.floors = new List<FloorDefinition>();
        for (int i = options[index].floors.Count - 1; i >= 0; i--) 
            activePacket.floors.Add(options[index].floors[i]);
        localPackets.Remove(options[index]);
    }

    public FloorDefinition GetFloor(int index) {
        if (index < iiThreshold)
            currentThreshold = FloorPacket.PacketType.I;
        else if (index >= iiThreshold && index < iiiThreshold) 
            currentThreshold = FloorPacket.PacketType.II;
        else if (index >= iiiThreshold && index < bossThreshold) 
            currentThreshold = FloorPacket.PacketType.III;
        else if (index >= bossThreshold) 
            currentThreshold = FloorPacket.PacketType.BOSS;
        

        if (activePacket == null || activePacket.packetType != currentThreshold || activePacket.floors.Count == 0) {
            Debug.Log(index + " " + currentThreshold);
            StartPacket(currentThreshold);
        }

        FloorDefinition floor = activePacket.floors[Random.Range(0, activePacket.floors.Count-1)];
        activePacket.floors.Remove(floor);
        return floor;
    }


}
