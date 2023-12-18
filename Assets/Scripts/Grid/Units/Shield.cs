using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour {

    [HideInInspector] public Unit unit;
    public SpriteRenderer gfx;
    public bool thorns, healing;

    public void Init(Unit target, ShieldData data) {
        unit = target;
// SPECIAL TIER II - Damages attackers
        thorns = data.upgrades[SlagEquipmentData.UpgradePath.Special] >= 2;
// UNIT TIER II - Heals users
        healing = data.upgrades[SlagEquipmentData.UpgradePath.Unit] >= 1; 
        
        gfx.sortingOrder = unit.grid.SortOrderFromCoord(unit.coord);
    }
}
