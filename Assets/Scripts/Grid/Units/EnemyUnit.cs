using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : Unit {

    public enum Pathfinding { ClosestCoord, FurthestWithinAttackRange, Random };

    [Header("Enemy Unit")]
    public Pathfinding pathfinding;
    [SerializeField] Unit closestUnit;

    public virtual IEnumerator CalculateAction() {
// First attack scan
        UpdateAction(equipment[1]);
        foreach (Vector2 coord in validActionCoords) 
        {
            if (ValidCommand(coord, selectedEquipment)) {
                manager.SelectUnit(this);
                GridElement target = null;
                foreach (GridElement ge in grid.CoordContents(coord))
                    target = ge;
                grid.DisplayValidCoords(validActionCoords, selectedEquipment.gridColor);
                yield return new WaitForSecondsRealtime(0.5f);
                Coroutine co = StartCoroutine(selectedEquipment.UseEquipment(this, target));
                grid.UpdateSelectedCursor(false, Vector2.one * -32);
                grid.DisableGridHighlight();
                yield return co;
                yield return new WaitForSecondsRealtime(0.125f);
                manager.DeselectUnit();
                yield break;
            }
        }
// Move scan
        if (!moved) {
            UpdateAction(equipment[0], moveMod);
            Vector2 targetCoord = SelectOptimalCoord(pathfinding);
            if (Mathf.Sign(targetCoord.x) == 1) {
                manager.SelectUnit(this);
                grid.DisplayValidCoords(validActionCoords, selectedEquipment.gridColor);
                yield return new WaitForSecondsRealtime(0.5f);
                Coroutine co = StartCoroutine(selectedEquipment.UseEquipment(this, grid.sqrs.Find(sqr => sqr.coord == targetCoord)));
                grid.UpdateSelectedCursor(false, Vector2.one * -32);
                grid.DisableGridHighlight();
                yield return co;
                manager.DeselectUnit();
            }
        }

// Second attack scan
        UpdateAction(equipment[1]);
        foreach (Vector2 coord in validActionCoords) 
        {
            if (ValidCommand(coord, selectedEquipment)) {       
                manager.SelectUnit(this);   
                GridElement target = null;
                foreach (GridElement ge in grid.CoordContents(coord))
                    target = ge;
                grid.DisplayValidCoords(validActionCoords, selectedEquipment.gridColor);
                yield return new WaitForSecondsRealtime(0.5f);
                Coroutine co = StartCoroutine(selectedEquipment.UseEquipment(this, target));
                grid.UpdateSelectedCursor(false, Vector2.one * -32);
                grid.DisableGridHighlight();
                yield return co;            
                yield return new WaitForSecondsRealtime(0.125f);
                manager.DeselectUnit();
            }
        }
        grid.DisableGridHighlight();
    }


    public virtual Vector2 SelectOptimalCoord(Pathfinding path) {
        Vector2 coord = Vector2.zero;
        
        switch (path) {
            case Pathfinding.ClosestCoord:
                closestUnit = manager.scenario.player.units[0];
                if (closestUnit) {
                    foreach (Unit unit in manager.scenario.player.units) {
                        if (!unit.conditions.Contains(Status.Disabled)) {
                            if (Vector2.Distance(unit.coord, coord) < Vector2.Distance(closestUnit.coord, coord))
                                closestUnit = unit;
                        }
                    }
                    Vector2 closestCoord = Vector2.one * -32;
                    foreach(Vector2 c in validActionCoords) {
                        if (Vector2.Distance(c, closestUnit.coord) < Vector2.Distance(closestCoord, closestUnit.coord)) 
                            closestCoord = c;
                    }
// If there is a valid closest coord
                    if (Mathf.Sign(closestCoord.x) == 1) {
                        return closestCoord;
                    }
                    return Vector2.one * -32;
                }
            break;
            case Pathfinding.FurthestWithinAttackRange:
                closestUnit = manager.scenario.player.units[0];
                if (closestUnit) {
                    foreach (Unit unit in manager.scenario.player.units) {
                        if (!unit.conditions.Contains(Status.Disabled)) {
                            if (Vector2.Distance(unit.coord, coord) < Vector2.Distance(closestUnit.coord, coord))
                                closestUnit = unit;
                        }
                    }
                    Vector2 furthestCoord = closestUnit.coord;
                    foreach(Vector2 c in validActionCoords) {
                        Vector2 dir = Vector2.zero;
                        List<Vector2> validFurthestCoords = new List<Vector2>();
// Check in four directions
                        for (int i = 0; i < 4; i++) {
                            switch (i) {case 0: dir = Vector2.down; break; case 1: dir = Vector2.left; break; case 2: dir = Vector2.up; break; case 3: dir = Vector2.right; break;}
                            
                            for (int r = 1; r <= selectedEquipment.range; r++) {
                                Vector2 farCoord = closestUnit.coord + r * dir;
                                if (validActionCoords.Contains(farCoord) &&
                                Vector2.Distance(farCoord, closestUnit.coord) > Vector2.Distance(furthestCoord, closestUnit.coord)) 
                                    furthestCoord = farCoord;
                            }

                        }
                    }
// If there is a valid closest coord
                    if (furthestCoord != closestUnit.coord) {
                        return furthestCoord;
                    } else {
                        return SelectOptimalCoord(Pathfinding.ClosestCoord);
                    }
                } else {
                    return SelectOptimalCoord(Pathfinding.ClosestCoord);
                }
            case Pathfinding.Random:
                if (validActionCoords.Count > 0) {
                    int rndIndex = Random.Range(0, validActionCoords.Count - 1);
                    return validActionCoords[rndIndex];
                } else
                    return Vector2.one * -32;
        }

        return coord;
    }

    public override IEnumerator TakeDamage(int dmg, DamageType dmgType = DamageType.Unspecified, GridElement source = null)
    {
        int modifiedDmg = conditions.Contains(Status.Weakened) ? dmg * 2 : dmg;
        if (hpCurrent - modifiedDmg <= hpMax/2) 
            gfxAnim.SetBool("Damaged", true);

        yield return base.TakeDamage(dmg, dmgType, source);
    }

    public override IEnumerator DestroyElement(DamageType dmgType)
    {
        switch(dmgType) {
            case DamageType.Unspecified:
                gfxAnim.SetTrigger("Split");
            break;
            case DamageType.Bile:
                gfxAnim.SetTrigger("Melt");
            break;
            case DamageType.Gravity:
                gfxAnim.SetTrigger("Crush");
            break;
            case DamageType.Melee:
                gfxAnim.SetTrigger("Split");
            break;
        }

        return base.DestroyElement(dmgType);
    }
}
