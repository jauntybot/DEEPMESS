using System;
using System.Collections.Generic;
using Relics;


[System.Serializable]
public class RunData {
    
    public int startCavity;
    public Dictionary<String, List<int>> unitHP;
    public string hammerUnit;
    public Dictionary<String, string> bulbs;
    public Dictionary<String, List<String>> unitUpgrades;
    public List<String> godThoughts;
    public Dictionary<String, int> objectives;
    public List<int> objectiveIndices;
    public int slimeBux;

    public RunData() {}

    public RunData(int cavityIndex, List<Unit> playerUnits, List<Relic> relics, List<Objective> activeObjectives, List<int> _objectiveIndices, int bux) { 
        startCavity = cavityIndex;
        unitUpgrades = new();
        unitHP = new();
        bulbs = new();
        foreach (Unit u in playerUnits) {
            if (u is PlayerUnit) {
                unitHP.Add(u.name, new() { u.hpCurrent, u.hpMax } );

                if (u.equipment.Find(e => e is HammerData)) hammerUnit = u.name;
                if (u.equipment.Find(e => e is BulbEquipmentData)) bulbs.Add(u.name, u.equipment.Find(e => e is BulbEquipmentData).name);

                List<String> upgrades = new();
                SlagGearData gear = (SlagGearData)u.equipment[1];
                foreach (GearUpgrade gu in gear.slottedUpgrades) {
                    if (gu != null)
                        upgrades.Add(gu.name);
                    else
                        upgrades.Add("Empty");
                }
                unitUpgrades.Add(gear.name, upgrades);
            } else if (u is Nail) {
                unitHP.Add(u.name, new() { u.hpCurrent, u.hpMax } );
            }
        }
        
        godThoughts = new();
        foreach (Relic relic in relics) {
            godThoughts.Add(relic.data.name);
        }

        objectives = new();
        foreach (Objective obj in activeObjectives) {
            objectives.Add(obj.name, obj.progress);
        }
        objectiveIndices = new(_objectiveIndices);
        slimeBux = bux;
    }
}
