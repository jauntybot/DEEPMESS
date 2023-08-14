using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossUnit : EnemyUnit
{

    [SerializeField] Unit prevTarget;

    bool secondPhase = false;



    public override IEnumerator DestroyElement(DamageType dmgType)
    {
        manager.scenario.player.nail.ToggleNailState(Nail.NailState.Primed);

        yield return base.DestroyElement(dmgType);
    }

    public override bool ValidCommand(Vector2 target, EquipmentData equip) {
        if (equip == null) return false;
        if (validActionCoords.Count == 0) return false;
        if (!validActionCoords.Contains(target)) return false;
        if (grid.CoordContents(target)[0] == prevTarget) return false;
        if (energyCurrent < equip.energyCost && equip is not MoveData) return false;
        else if (moved && equip is MoveData) return false;
        else if (usedEquip && (equip is PerFloorEquipmentData && equip is not HammerData)) return false;

        return true;
    }

    
    public override IEnumerator CalculateAction() {
// First attack scan
        UpdateAction(equipment[1]);
        foreach (Vector2 coord in validActionCoords) {
            if (ValidCommand(coord, selectedEquipment)) {
                manager.SelectUnit(this);
                GridElement target = null;
                foreach (GridElement ge in grid.CoordContents(coord))
                    target = ge;
                grid.DisplayValidCoords(validActionCoords, selectedEquipment.gridColor);
                yield return new WaitForSecondsRealtime(0.5f);
                Coroutine co = StartCoroutine(selectedEquipment.UseEquipment(this, target));
                prevTarget = (Unit)target;
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
                grid.DisplayValidCoords(validActionCoords, selectedEquipment.gridColor);
                yield return new WaitForSecondsRealtime(0.5f);
                Coroutine co = StartCoroutine(selectedEquipment.UseEquipment(this, target));
                prevTarget = (Unit)target;
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

    public override Vector2 SelectOptimalCoord(Pathfinding path) {
        switch (pathfinding) {
            case Pathfinding.ClosestCoord:
                closestUnit = null;
                List<Unit> playerUnits = new List<Unit>();
                for (int i = manager.scenario.player.units.Count - 1; i >= 0; i--) 
                    playerUnits.Add(manager.scenario.player.units[i]);
                playerUnits.Remove(prevTarget);
                foreach (Unit unit in playerUnits) {
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



}
