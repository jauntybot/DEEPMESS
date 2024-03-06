using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Floors/Floor Packet")]
[System.Serializable]
public class FloorPacket : ScriptableObject {

    public enum PacketType { Tutorial, I, II, III, BOSS, BARRIER };
    public PacketType packetType;
    public int packetLength;
    public bool inOrder;
    public List<FloorDefinition> firstFloors;
    public List<FloorDefinition> floors;

    public enum PacketMods { Extreme, Elite };
    public List<PacketMods> packetMods;

    public int bonusObjectives;
    //[HideInInspector]
    public List<Objective> objectives;
    public int nuggets;
    public int relics;

}