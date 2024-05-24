using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Floors/Floor Packet")]
[System.Serializable]
public class FloorChunk : ScriptableObject {

    public enum PacketType { Tutorial, I, II, III, BOSS, BARRIER };
    public PacketType packetType;
    public int packetLength;
    public bool inOrder;
    public List<FloorDefinition> firstFloors;
    public List<FloorDefinition> floors;
    public int minEnemies;
    
    public enum PacketMods { Hazard, Elite };
    [HideInInspector] public bool eliteSpawn;
    public List<PacketMods> packetMods;
    public int evtOffset;
    public List<FloorDefinition> hazardFloors;

    // public int nuggets;
    // public int relics;

    // public int bonusNuggetObjectives;
    // public int bonusRelicObjectives;
    // //[HideInInspector]
    // public List<Objective> objectives;
}