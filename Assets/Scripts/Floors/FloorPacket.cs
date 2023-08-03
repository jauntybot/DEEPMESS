using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Floors/Floor Packet")]
[System.Serializable]
public class FloorPacket : ScriptableObject {

    public enum PacketType { I, II, III, BOSS, BARRIER };
    public PacketType packetType;
    public List<FloorDefinition> floors;


}