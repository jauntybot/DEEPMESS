using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[CreateAssetMenu(menuName = "Floors/Floor Definition")]
[System.Serializable]
public class FloorDefinition : ScriptableObject
{
    public enum FloorType { Combat, SlotMachine };
    public FloorType floorType; 
    public int genPool;

// Combat floor refs
    public FloorAtlas atlas;
    public List<Spawn> initSpawns = new();
    public int minEnemies;


// Slot machine floor refs
    public enum SlotsType { Equipment, Upgrades };
    public SlotsType slotsType;
    public List<EquipmentData> equipmentTable;
    //public List<UpgradeData> upgradeTable;

    [System.Serializable]
    public class Spawn {

        public FloorAsset asset;
        public Vector2 coord;

    }
}

