using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Relics;
using UnityEngine;

[System.Serializable]
public class RunData {
    
    public int startCavity;
    public Dictionary<String, List<int>> unitHP;
    public Dictionary<String, List<String>> unitUpgrades;
    public List<String> godThoughts;

    public RunData() {}

    public RunData(int cavityIndex, List<Unit> playerUnits, List<Relic> relics) { // List<Unit> units, List<RelicData> _relics, List<Objective> _objectives
        startCavity = cavityIndex;
        unitUpgrades = new();
        unitHP = new();
        foreach (Unit u in playerUnits) {
            if (u is PlayerUnit) {
                unitHP.Add(u.name, new() { u.hpCurrent, u.hpMax } );
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
    }
}
