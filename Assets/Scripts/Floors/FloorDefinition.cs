using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[CreateAssetMenu(menuName = "Floors/Floor Definition")]
[System.Serializable]
public class FloorDefinition : ScriptableObject
{
    public enum FloorType { Combat, SlotMachine, Reaper };
    public FloorType floorType; 

    public FloorAtlas atlas;
    public int genPool;
    public List<Spawn> initSpawns;

}

[System.Serializable]
public class Spawn {

    public FloorAsset asset;
    public Vector2 coord;

}

