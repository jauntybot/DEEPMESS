using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Gear/Slag/Shield")]
[System.Serializable]   
public class ShieldData : SlagGearData {

    [SerializeField] GameObject shieldPrefab;
    public List<Shield> activeShields = new();
    public int activeShieldLimit;

    public int shieldHP;
    public bool buyoant, thorns, liveWired, aerodynamics, pinchClosed;
    [SerializeField] GameObject pinchClosedVFX, pinchClosedInflictedVFX;

    public override void EquipGear(Unit user) {
        base.EquipGear(user);
        activeShields = new();
        buyoant = false;
        thorns = false;
        liveWired = false;
        aerodynamics = false;
        pinchClosed = false;
        activeShieldLimit = 0;
        range = 1;
        shieldHP = 1;
    }

    public override List<Vector2> TargetEquipment(GridElement user, int mod) {
        List<Vector2> validCoords = new();
// Shield any unit
        //if (upgrades[UpgradePath.Shunt] == 3)
        //    validCoords = EquipmentAdjacency.OfTypeOnBoardAdjacency(targetTypes, user.grid);
        //else 
            validCoords = EquipmentAdjacency.GetAdjacent(user.coord, range + mod, this, targetTypes, user.grid);
            
        validCoords.Add(user.coord);
        user.grid.DisplayValidCoords(validCoords, gridColor);

        if (user is PlayerUnit u) {
          u.ui.ToggleEquipmentButtons();
          u.inRangeCoords = validCoords;  
        } 
        for (int i = validCoords.Count - 1; i >= 0; i--) {
            if (user.grid.CoordContents(validCoords[i]).Count != 0) {
                bool remove = true;
                foreach(GridElement ge in user.grid.CoordContents(validCoords[i])) {
                    foreach (GridElement tar in targetTypes)
                        if (ge.GetType() == tar.GetType()) remove = false;
                    
                }
                if (remove) validCoords.RemoveAt(i);
            } else
                validCoords.RemoveAt(i);
        }
        return validCoords;
    }

    public override IEnumerator UseGear(GridElement user, GridElement target = null) {
        yield return base.UseGear(user, target);

        OnEquipmentUse evt = ObjectiveEvents.OnEquipmentUse;
        evt.data = this; evt.user = user; evt.target = target;
        ObjectiveEventManager.Broadcast(evt);

        PlayerUnit pu = (PlayerUnit)user;
// SPECIAL TIER I - Increase shield limit
        //int shieldLimit = upgrades[UpgradePath.Scab] >= 1 ? 1 : 0;
// Destory instances exceeding shield limit
        if (activeShields.Count > activeShieldLimit) {
            Shield s = activeShields[0];
            s.unit.RemoveShield();
            activeShields.Remove(s);
        } 
// Instantiate new shield obj
        Shield shield = Instantiate(shieldPrefab, target.transform).GetComponent<Shield>();
        shield.Init((Unit)target, this);

        if (aerodynamics && target is Unit u) u.moveMod++;
        if (buyoant && target is Unit un && un.conditions.Contains(Unit.Status.Restricted)) un.RemoveCondition(Unit.Status.Restricted);

        activeShields.Add(shield);
        target.ApplyShield(shield);

        if (pinchClosed) {
            GameObject go = Instantiate(pinchClosedVFX, user.transform.position, Quaternion.identity);
            go.GetComponent<SpriteRenderer>().sortingOrder = target.gfx[0].sortingOrder++;
            List<Vector2> targetCoords = EquipmentAdjacency.OrthagonalAdjacency(target.coord, 1);
            for (int i = targetCoords.Count - 1; i >= 0; i--) {
                if (user.grid.CoordContents(targetCoords[i]).Count > 0) {
                    foreach(GridElement ge in user.grid.CoordContents(targetCoords[i])) {
                        if (ge is EnemyUnit eu) {
                            go = Instantiate(pinchClosedInflictedVFX, eu.transform.position, Quaternion.identity);
                            go.GetComponent<SpriteRenderer>().sortingOrder = eu.gfx[0].sortingOrder++;
                            eu.StartCoroutine(eu.TakeDamage(1, GridElement.DamageType.Melee, user, this));
                        }
                    }
                }
            }
        }
    }
}
