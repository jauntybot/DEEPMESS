using System.Collections;
using System.Collections.Generic;
using Relics;
using UnityEngine;

[System.Serializable]
public class RunData {
    

    public FloorDefinition currentFloor;
    public FloorChunk activeChunk;
    public SlagGearData gear1;
    public List<GearUpgrade> gear1Upgrades;
    
    public List<Relic> relics;

    public List<Objective> objectives;
    public List<int> objectiveProg;

    public RunData(FloorDefinition _currentFloor, FloorChunk _activeChunk, List<Unit> units, List<Relic> _relics) { //, List<Objective> _objectives
        currentFloor = _currentFloor;
        activeChunk = _activeChunk;

        gear1 = (SlagGearData)units[0].equipment[1];
        gear1Upgrades = gear1.upgrades;
    }

}
