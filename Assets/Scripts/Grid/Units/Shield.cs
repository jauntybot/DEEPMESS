using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour {

    [HideInInspector] public Unit unit;
    [HideInInspector] public ShieldData data;
    public SpriteRenderer gfx;
    Animator anim;
    public bool buoyant, thorns, healing;

    public void Init(Unit target, ShieldData data) {
        anim = gfx.GetComponent<Animator>();
        unit = target;
// UNIT TIER I - Prevents liquid tile effects
        buoyant = data.upgrades[SlagEquipmentData.UpgradePath.Sludge] >= 1;
// UNIT TIER II - Heals users
        healing = data.upgrades[SlagEquipmentData.UpgradePath.Sludge] >= 2; 
// SPECIAL TIER II - Damages attackers
        thorns = data.upgrades[SlagEquipmentData.UpgradePath.Scab] >= 2;

        
        gfx.sortingOrder = unit.grid.SortOrderFromCoord(unit.coord);
    }

    public void DestroySelf(GridElement source = null) {
        if (data.activeShields.Contains(this))
            data.activeShields.Remove(this);
        anim.SetTrigger("Destroy");
    }
}
