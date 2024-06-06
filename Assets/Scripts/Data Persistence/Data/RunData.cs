using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Relics;
using UnityEngine;

[System.Serializable]
public class RunData {
    
    public FloorAsset floorAsset;
    public Dictionary<Vector2, string[]> floorDict;
    public FloorChunk activeChunk;
    //public Grid currentFloor;
    //public FloorChunk activeChunk;
    //public SlagGearData gear1;
    //public List<GearUpgrade> gear1Upgrades;
    
    //public List<RelicData> relics;

    //public List<Objective> objectives;
    //public List<int> objectiveProg;

    public RunData(Grid _currentFloor, FloorChunk _activeChunk) { // List<Unit> units, List<RelicData> _relics, List<Objective> _objectives
        floorDict = new();
        if (_currentFloor) {
            for(int x = 0; x <= 7; x++) {
                for (int y = 0; y <= 7; y++) {
                    List<string> contents = new();

                    Tile t = _currentFloor.tiles.Find(t => t.coord == new Vector2(x,y));
                    switch(t.tileType) {
                        default: case Tile.TileType.Bone: contents.Add("BONE");
                            if (t is TileBulb tb) {
                                if (tb.bulb is SupportBulbData s) {
                                    if (s.supportType == SupportBulbData.SupportType.Heal) contents.Add("HEAL BULB");
                                    else contents.Add("SURGE BULB");
                                } else
                                    contents.Add("STUN BULB");
                                
                            }
                        break;
                        case Tile.TileType.Blood: contents.Add("BLOOD"); break;
                        case Tile.TileType.Bile: contents.Add("BILE"); break;
                    }

                    foreach (GridElement ge in _currentFloor.CoordContents(new Vector2(x,y))) {
                        contents.Add(ge.name);
                    }

                    floorDict.Add(new Vector2(x,y), contents.ToArray());
                }
            }
        }

        //currentFloor = _currentFloor;
        //activeChunk = _activeChunk;
        

        //gear1 = (SlagGearData)units[0].equipment[1];
        //gear1Upgrades = gear1.upgrades;
    }

}
