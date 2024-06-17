using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Relics;
using UnityEngine;

[System.Serializable]
public class RunData {
    
    public Dictionary<String, List<int>> unitHP;
    public Dictionary<String, List<String>> unitUpgrades;

    public RunData(List<Unit> playerUnits) { // List<Unit> units, List<RelicData> _relics, List<Objective> _objectives
        unitUpgrades = new();
        unitHP = new();
        foreach (Unit u in playerUnits) {
            if (u is PlayerUnit) {
                unitHP.Add(u.name, new() { u.hpCurrent, u.hpMax } );
                List<String> upgrades = new();
                SlagGearData gear = (SlagGearData)u.equipment[1];
                foreach (GearUpgrade gu in gear.upgrades) {
                    upgrades.Add(gu.name);
                }
                unitUpgrades.Add(gear.name, upgrades);
            }
        }
    }

}
