using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour {

    [HideInInspector] public Unit unit;
    public ShieldData data;
    public SpriteRenderer gfx;
    [SerializeField] SFX destroySFX;
    Animator anim;
    public bool buoyant, thorns, liveWired, aerodynamics, healing;

    public void Init(Unit target, ShieldData _data) {
        data = _data;
        anim = gfx.GetComponent<Animator>();
        unit = target;


// // UNIT TIER II - Heals users
//         healing = data.upgrades[SlagGearData.UpgradePath.Sludge] >= 2; 

        buoyant = data.buyoant;
        thorns = data.thorns;
        liveWired = data.liveWired;
        aerodynamics = data.aerodynamics;

        gfx.sortingOrder = unit.grid.SortOrderFromCoord(unit.coord)+1;
    }

    public void DestroySelf(GridElement source = null) {
        unit.PlaySound(destroySFX);
        if (data.activeShields.Contains(this))
            data.activeShields.Remove(this);
        anim.SetTrigger("Destroy");
    }
}
