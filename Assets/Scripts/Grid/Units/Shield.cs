using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour {

    [HideInInspector] public Unit unit;
    public SpriteRenderer gfx;
    public bool buoyant, thorns, healing;

    public void Init(Unit target, ShieldData data) {
        unit = target;
// UNIT TIER I - Prevents liquid tile effects
        if (data.upgrades[SlagEquipmentData.UpgradePath.Unit] >= 1) {
            buoyant = true;
        }
// UNIT TIER II - Heals users
        healing = data.upgrades[SlagEquipmentData.UpgradePath.Unit] >= 2; 
// SPECIAL TIER II - Damages attackers
        thorns = data.upgrades[SlagEquipmentData.UpgradePath.Special] >= 2;

        
        gfx.sortingOrder = unit.grid.SortOrderFromCoord(unit.coord);
    }
}
