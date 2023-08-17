using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : Unit {

    public enum Pathfinding { ClosestCoord, FurthestWithinAttackRange, Random };

    [Header("Enemy Unit")]
    public Pathfinding pathfinding;
    [SerializeField] protected Unit closestUnit;


    public virtual IEnumerator ScatterTurn() {
        UpdateAction(equipment[0], moveMod);
        Vector2 targetCoord = SelectOptimalCoord(EnemyUnit.Pathfinding.Random);
        if (Mathf.Sign(targetCoord.x) == 1) {
            manager.SelectUnit(this);
            manager.currentGrid.DisplayValidCoords(validActionCoords, selectedEquipment.gridColor);
            yield return new WaitForSecondsRealtime(0.5f);
            Coroutine co = StartCoroutine(selectedEquipment.UseEquipment(this, manager.currentGrid.tiles.Find(sqr => sqr.coord == targetCoord)));
            manager.currentGrid.UpdateSelectedCursor(false, Vector2.one * -32);
            manager.currentGrid.DisableGridHighlight();
            yield return co;
            manager.DeselectUnit();
        }
    }

    public virtual IEnumerator CalculateAction() {
// First attack scan
        UpdateAction(equipment[1]);
        foreach (Vector2 coord in validActionCoords) {
            if (ValidCommand(coord, selectedEquipment)) {
                manager.SelectUnit(this);
                GridElement target = null;
                foreach (GridElement ge in grid.CoordContents(coord))
                    target = ge;
                yield return new WaitForSecondsRealtime(0.5f);
                Coroutine co = StartCoroutine(selectedEquipment.UseEquipment(this, target));
                grid.UpdateSelectedCursor(false, Vector2.one * -32);
                grid.DisableGridHighlight();
                yield return co;
                yield return new WaitForSecondsRealtime(0.125f);
                usedEquip = true;
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
                Coroutine co = StartCoroutine(selectedEquipment.UseEquipment(this, grid.tiles.Find(sqr => sqr.coord == targetCoord)));
                grid.UpdateSelectedCursor(false, Vector2.one * -32);
                grid.DisableGridHighlight();
                yield return co;
                manager.DeselectUnit();
            }
        }

// Second attack scan
        UpdateAction(equipment[1]);
        foreach (Vector2 coord in validActionCoords) {
            if (ValidCommand(coord, selectedEquipment)) {
                manager.SelectUnit(this);
                GridElement target = null;
                foreach (GridElement ge in grid.CoordContents(coord))
                    target = ge;
                yield return new WaitForSecondsRealtime(0.5f);
                Coroutine co = StartCoroutine(selectedEquipment.UseEquipment(this, target));
                grid.UpdateSelectedCursor(false, Vector2.one * -32);
                grid.DisableGridHighlight();
                yield return co;
                yield return new WaitForSecondsRealtime(0.125f);
                usedEquip = true;
                manager.DeselectUnit();
            }
        }
        grid.DisableGridHighlight();
    }


// PROBLEM FUNCTION
    public virtual Vector2 SelectOptimalCoord(Pathfinding pathfinding) {
        switch (pathfinding) {
            case Pathfinding.ClosestCoord:
                closestUnit = null;
                // int shortestPathCount = 64;
                // Dictionary<Vector2, Vector2> shortestPath = new();

                // foreach (Unit unit in manager.scenario.player.units) {
                //     if (!unit.conditions.Contains(Status.Disabled)) {
                //         List<Vector2> targetCoords = EquipmentAdjacency.GetAdjacent(unit.coord, equipment[1].range, equipment[0]);
                //         Debug.Log(targetCoords.Count);
                //         string coords = "";
                //         foreach (Vector2 c in targetCoords) {
                //             coords += c + ", " ;
                //             Dictionary<Vector2, Vector2> fromTo = new Dictionary<Vector2, Vector2>(); 
                //             fromTo = EquipmentAdjacency.SteppedCoordAdjacency(coord, c, equipment[0]);
                //             if (fromTo.Count < shortestPathCount) {
                //                 shortestPath = fromTo;
                //                 shortestPathCount = fromTo.Count; 
                //             }
                //         }
                //         Debug.Log(targetCoords.Count + " " + coords);
                //     }
                // }
                // Vector2 targetCoord = coord;
                // for (int i = 0; i < equipment[0].range; i++) {
                //     targetCoord = shortestPath[targetCoord];
                // }
                // return targetCoord;
                foreach (Unit unit in manager.scenario.player.units) {
                    if (!unit.conditions.Contains(Status.Disabled)) {
                        if (closestUnit == null || Vector2.Distance(unit.coord, coord) < Vector2.Distance(closestUnit.coord, coord))
                            closestUnit = unit;
                    }
                }

                Vector2 closestCoord = coord;
                foreach(Vector2 c in validActionCoords) {
                    if (Vector2.Distance(c, closestUnit.coord) < Vector2.Distance(closestCoord, closestUnit.coord)) 
                        closestCoord = c;
                }
                return closestCoord;
                
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
