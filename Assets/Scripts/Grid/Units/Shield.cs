using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour {

    [HideInInspector] public Unit unit;
    public SpriteRenderer gfx;
    public bool thorns;

    public void Init(Unit target, ShieldData data) {
        unit = target;
        thorns = data.upgrades[SlagEquipmentData.UpgradePath.Power] >= 2;
        gfx.sortingOrder = unit.grid.SortOrderFromCoord(unit.coord);
    }
}
