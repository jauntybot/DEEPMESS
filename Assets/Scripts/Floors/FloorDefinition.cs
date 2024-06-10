using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[CreateAssetMenu(menuName = "Floors/Floor Definition")]
[System.Serializable]
public class FloorDefinition : ScriptableObject {
    public enum FloorType { Combat, SlotMachine };
    public FloorType floorType; 
    public int genPool;

// Combat floor refs
    public FloorAtlas atlas;
    public List<Spawn> initSpawns = new();
    [HideInInspector] public bool spawnBeacon = false;
    [HideInInspector] public bool spawnBloatedBulb = false;
    [HideInInspector] public bool spawnElite = false;


    //public List<UpgradeData> upgradeTable;

    [System.Serializable]
    public class Spawn {

        public FloorAsset asset;
        public Vector2 coord;

    }
}

