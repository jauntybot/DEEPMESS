
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyUnit : Unit {

    public enum Pathfinding { ClosestCoord, FurthestWithinAttackRange, Random };

    [Header("Enemy Unit")]
    public Pathfinding pathfinding;
    [SerializeField] protected Unit closestUnit;
    public GameObject splatterPrefab;
    [SerializeField] SFX stunnedSFX;

    public virtual IEnumerator ScatterTurn() {
        if (!conditions.Contains(Status.Stunned)) {
            UpdateAction(equipment[0], moveMod);
            Vector2 targetCoord = SelectOptimalCoord(EnemyUnit.Pathfinding.Random);
            if (Mathf.Sign(targetCoord.x) == 1) {
                //manager.SelectUnit(this);
                manager.currentGrid.DisplayValidCoords(validActionCoords, selectedEquipment.gridColor);
                yield return new WaitForSecondsRealtime(0.5f);
                Coroutine co = StartCoroutine(selectedEquipment.UseGear(this, manager.currentGrid.tiles.Find(sqr => sqr.coord == targetCoord)));
                manager.currentGrid.UpdateSelectedCursor(false, Vector2.one * -32);
                manager.currentGrid.DisableGridHighlight();
                yield return co;
                manager.DeselectUnit();
            }
        } else {
            moved = true;
            //energyCurrent = 0;
            yield return new WaitForSecondsRealtime(0.25f);
        }

        manager.unitActing = false;
    }

    public virtual IEnumerator CalculateAction() {
        if (!conditions.Contains(Status.Stunned)) {
            yield return StartCoroutine(AttackScan());
            if (energyCurrent > 0) {
                if (!moved) yield return StartCoroutine(MoveScan());
                yield return StartCoroutine(AttackScan());
            }
        } else {
            moved = true;
            //energyCurrent = 0;
            yield return new WaitForSecondsRealtime(0.125f);
            RemoveCondition(Status.Stunned);
            yield return new WaitForSecondsRealtime(0.25f);
        }
       
        grid.DisableGridHighlight();
        manager.unitActing = false;
    }

    protected virtual IEnumerator MoveScan() {
        UpdateAction(equipment[0], moveMod);
        Vector2 targetCoord = SelectOptimalCoord(pathfinding);
        //while (!Input.GetMouseButtonDown(0)) yield return null;
        //manager.SelectUnit(this);
        yield return new WaitForSecondsRealtime(0.5f);
        Coroutine co = StartCoroutine(selectedEquipment.UseGear(this, grid.tiles.Find(sqr => sqr.coord == targetCoord)));
        grid.UpdateSelectedCursor(false, Vector2.one * -32);
        grid.DisableGridHighlight();
        yield return co;
        //manager.DeselectUnit();
    }

    protected virtual IEnumerator AttackScan() {
        UpdateAction(equipment[1]);
        foreach (Vector2 coord in validActionCoords) {
            if (ValidCommand(coord, selectedEquipment)) {
                //manager.SelectUnit(this);
                GridElement target = null;
                foreach (GridElement ge in grid.CoordContents(coord))
                    target = ge;
                yield return new WaitForSecondsRealtime(0.5f);
                Coroutine co = StartCoroutine(selectedEquipment.UseGear(this, target));
                grid.UpdateSelectedCursor(false, Vector2.one * -32);
                grid.DisableGridHighlight();
                yield return co;
                yield return new WaitForSecondsRealtime(0.125f);
                //manager.DeselectUnit();
            }
        }
    }


// PROBLEM FUNCTION
    public virtual Vector2 SelectOptimalCoord(Pathfinding pathfinding) {
        switch (pathfinding) {
            case Pathfinding.ClosestCoord:
                Vector2 targetCoord = coord;
                
                Dictionary<Vector2, Vector2> shortestPath = ClosestCoord(1);
                if (shortestPath != null && shortestPath.Count > 0) {
                    targetCoord = shortestPath.ElementAt(shortestPath.Count-1).Value;
                }
                foreach (KeyValuePair<Vector2, Vector2> entry in shortestPath)
                    //grid.tiles.Find(t => t.coord == entry.Value).ToggleValidCoord(true, Color.white, true);

// There is a valid target coord, not own coord
                if (targetCoord != coord) {
                    targetCoord = coord;
                    for (int i = 1; i <= equipment[0].range; i++) {
                        if (shortestPath.ContainsKey(targetCoord) && grid.CoordContents(shortestPath[targetCoord]).Count == 0)
                            targetCoord = shortestPath[targetCoord];
                        else break;
                    }
                    //grid.tiles.Find(t => t.coord == targetCoord).ToggleValidCoord(true, Color.yellow, true);
                }
                
            return targetCoord;
        
//Old logic
            //     closestUnit = null;
            //     foreach (Unit unit in manager.scenario.player.units) {
            //         if (grid.gridElements.Contains(unit) && unit.hpCurrent != 0 && !unit.conditions.Contains(Status.Disabled) && equipment[1].targetTypes.Find(t => t.GetType() == unit.GetType())) {
            //             if (closestUnit == null || Vector2.Distance(unit.coord, coord) < Vector2.Distance(closestUnit.coord, coord))
            //                 closestUnit = unit;
            //         }
            //     }

            //     Vector2 closestCoord = coord;
            //     foreach(Vector2 c in validActionCoords) {
            //         if (Vector2.Distance(c, closestUnit.coord) < Vector2.Distance(closestCoord, closestUnit.coord)) 
            //             closestCoord = c;
            //     }
            //     return closestCoord;
                
            case Pathfinding.Random:
                if (validActionCoords.Count > 0) {
                    int rndIndex = Random.Range(0, validActionCoords.Count - 1);
                    return validActionCoords[rndIndex];
                } else return coord;
        }
// Fallout, returns own coord
        return coord;
    }

    Dictionary<Vector2, Vector2> ClosestCoord(int range) {
        int shortestPathCount = 64;
        Dictionary<Vector2, Vector2> shortestPath = new();
        foreach (Unit unit in manager.scenario.player.units) {
            if (!unit.conditions.Contains(Status.Disabled)) {
// Adjacent coords to player unit
                List<Vector2> targetCoords = EquipmentAdjacency.BoxAdjacency(unit.coord, range);

                foreach (Vector2 c in targetCoords) {
                    Dictionary<Vector2, Vector2> fromTo = new(); 
                    fromTo = EquipmentAdjacency.ClosestSteppedCoordAdjacency(coord, c, equipment[0]);
                    if (fromTo != null && fromTo.Count < shortestPathCount) {
                        shortestPath = fromTo;
                        shortestPathCount = fromTo.Count;
                        //grid.tiles.Find(t => t.coord == c).ToggleValidCoord(true, Color.blue, true);
                    } else {
                        //  grid.tiles.Find(t => t.coord == c).ToggleValidCoord(true, Color.red, true);
                    }
                }
            }
        }

        return shortestPath;
    }

    public override IEnumerator TakeDamage(int dmg, DamageType dmgType = DamageType.Unspecified, GridElement source = null, GearData sourceEquip = null) {
        if (!destroyed) {
            int modifiedDmg = conditions.Contains(Status.Weakened) ? dmg * 2 : dmg;
            if (hpCurrent - modifiedDmg <= hpMax/2) 
                gfxAnim.SetBool("Damaged", true);

            Vector2 dir = Vector2.zero;
            if (source) dir = (coord - source.coord).normalized;
            Splatter(dir);

            yield return base.TakeDamage(dmg, dmgType, source, sourceEquip);
        }
    }

    public override IEnumerator DestroySequence(DamageType dmgType = DamageType.Unspecified, GridElement source = null, GearData sourceEquip = null) {
        switch(dmgType) {
            case DamageType.Unspecified:
                gfxAnim.SetTrigger("Split");
            break;
            case DamageType.Bile:
                gfxAnim.SetTrigger("Melt");
            break;
            case DamageType.Crush:
                gfxAnim.SetTrigger("Crush");
            break;
            case DamageType.Melee:
                gfxAnim.SetTrigger("Split");
            break;
        }
        
        Splatter(Vector2.zero);

        yield return base.DestroySequence(dmgType, source, sourceEquip);
        
    }

    public override IEnumerator CollideFromAbove(GridElement subGE, int hardLand = 0) {
        if (manager.scenario.floorManager.tutorial.isActiveAndEnabled && !manager.scenario.floorManager.tutorial.collisionEncountered && manager.scenario.floorManager.floorSequence.activePacket.packetType != FloorChunk.PacketType.Tutorial)
            manager.scenario.floorManager.tutorial.StartCoroutine(manager.scenario.floorManager.tutorial.DescentDamage());
        
        if (subGE is PlayerUnit)
            yield return StartCoroutine(DestroySequence(DamageType.Fall, subGE));
        else {
            yield return StartCoroutine(TakeDamage(1, DamageType.Fall, subGE));
        }
    }

    public void Splatter(Vector2 dir) {
        Tile tileParent = grid.tiles.Find(t => t.coord == coord+dir);
        if (tileParent == null) tileParent = grid.tiles.Find(t => t.coord == coord);
        GameObject obj = Instantiate(splatterPrefab, tileParent.transform);
        BloodSplatter splatter = obj.GetComponent<BloodSplatter>();
        splatter.Init(this, dir);
        
    }

    public override void ApplyCondition(Status s, bool undo = false) {
        base.ApplyCondition(s);
        if (s == Status.Stunned) PlaySound(stunnedSFX);
    }

    public override void RemoveCondition(Status s, bool undo = false) {
        base.RemoveCondition(s);
        if (s == Status.Stunned) PlaySound(stunnedSFX);
    }

    public override void PlaySound(SFX sfx = null) {
        audioSource.pitch = Random.Range(0.8f, 1f);
        base.PlaySound(sfx);
    }
}
