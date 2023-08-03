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

// Returns true if threshold is passed
    public bool ThresholdCheck(int index) {
        FloorPacket.PacketType prevThreshold = currentThreshold;
        if (index < reqII)
            currentThreshold = FloorPacket.PacketType.I;
        else if (index >= reqII && index - reqII < reqIII) 
            currentThreshold = FloorPacket.PacketType.II;
        else if (index - reqII >= reqIII && index - reqII - reqIII < reqBoss) 
            currentThreshold = FloorPacket.PacketType.III;
        else if (index - reqII - reqIII >= reqBoss) 
            currentThreshold = FloorPacket.PacketType.BOSS;

        return currentThreshold != prevThreshold;
    }

    public FloorDefinition GetFloor(int index) {
        ThresholdCheck(index);

        if (activePacket == null || activePacket.packetType != currentThreshold || activePacket.floors.Count == 0) {
            StartPacket(currentThreshold);
        }

        FloorDefinition floor = activePacket.floors[Random.Range(0, activePacket.floors.Count-1)];
        activePacket.floors.Remove(floor);
        return floor;
    }


}
