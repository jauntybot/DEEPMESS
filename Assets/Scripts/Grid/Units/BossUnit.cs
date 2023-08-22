using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossUnit : EnemyUnit
{

    [SerializeField] Unit prevTarget = null;

    public override IEnumerator DestroyElement(DamageType dmgType)
    {
        manager.scenario.player.nail.ToggleNailState(Nail.NailState.Primed);

        yield return base.DestroyElement(dmgType);
    }

    public override bool ValidCommand(Vector2 target, EquipmentData equip) {
        if (equip == null) return false;
        if (validActionCoords.Count == 0) return false;
        if (!validActionCoords.Contains(target)) return false;
        //if (grid.CoordContents(target).Count > 0 && grid.CoordContents(target)[0] == prevTarget) return false;
        if (energyCurrent < equip.energyCost && equip is not MoveData) return false;
        else if (moved && equip is MoveData) return false;
        else if (usedEquip && (equip is PerFloorEquipmentData && equip is not HammerData)) return false;

        return true;
    }

    public override Vector2 SelectOptimalCoord(Pathfinding pathfinding) {
        switch (pathfinding) {
            case Pathfinding.ClosestCoord:
                // int shortestPathCount = 64;
                // Dictionary<Vector2, Vector2> shortestPath = new Dictionary<Vector2, Vector2>();
                // Vector2 targetCoord = coord;
                // Debug.Log("First while loop");
                // while (!shortestPath.ContainsKey(coord)) {
                //     foreach (Unit unit in manager.scenario.player.units) {
                //         if (!unit.conditions.Contains(Status.Disabled)) {
                //             List<Vector2> targetCoords = EquipmentAdjacency.GetAdjacent(unit.coord, equipment[1].range, equipment[0]);
                //             foreach (Vector2 c in targetCoords) {
                //                 Dictionary<Vector2, Vector2> fromTo = new Dictionary<Vector2, Vector2>(); 
                //                 fromTo = EquipmentAdjacency.ClosestSteppedCoordAdjacency(coord, c, equipment[0]);
                //                 if (fromTo != null && fromTo.Count < shortestPathCount) {
                //                     shortestPath = fromTo;
                //                     shortestPathCount = fromTo.Count;
                //                     targetCoord = c;
                //                 }
                //             }
                //         }
                //     }
                // }

                // if (targetCoord != coord) {
                //     grid.tiles.Find(t => t.coord == targetCoord).ToggleValidCoord(true, Color.blue, true);
                //     string coords = "MoveTo Coord: " + targetCoord + ", ";
                //     targetCoord = coord;
                //     for (int i = 1; i <= equipment[0].range; i++) {
                //         targetCoord = shortestPath[targetCoord];
                //         coords += i + ": " + targetCoord + ", ";
                //     }
                //     Debug.Log(coords);
                //     grid.tiles.Find(t => t.coord == targetCoord).ToggleValidCoord(true, Color.white, true);
                // }
                
                // return targetCoord;

// Old logic
                closestUnit = null;
                List<Unit> playerUnits = new();
                foreach (Unit unit in manager.scenario.player.units) 
                    playerUnits.Add(unit);
                if (prevTarget)
                    playerUnits.Remove(prevTarget);
                foreach (Unit unit in manager.scenario.player.units) {
                    if (!unit.conditions.Contains(Status.Disabled)) {
                        if (closestUnit == null || Vector2.Distance(unit.coord, coord) < Vector2.Distance(closestUnit.coord, coord))
                            closestUnit = unit;
                    }
                }
                prevTarget = closestUnit;

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
                } else return coord;
        }
        return coord;
    }

}
