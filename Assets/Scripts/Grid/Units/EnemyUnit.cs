using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : Unit {

    public enum Pathfinding { ClosestCoord, FurthestWithinAttackRange, Random };

    [Header("Enemy Unit")]
    public Pathfinding pathfinding;
    [SerializeField] Unit closestTkn;


    public virtual Vector2 SelectOptimalCoord(Pathfinding path) {
        Vector2 coord = Vector2.zero;
        
        switch (path) {
            case Pathfinding.ClosestCoord:
                closestTkn = manager.scenario.player.units[0];
                if (closestTkn) {
                    foreach (Unit tkn in manager.scenario.player.units) {
                        if (Vector2.Distance(tkn.coord, coord) < Vector2.Distance(closestTkn.coord, coord))
                            closestTkn = tkn;
                    }
                    Vector2 closestCoord = Vector2.one * -32;
                    foreach(Vector2 c in validActionCoords) {
                        if (Vector2.Distance(c, closestTkn.coord) < Vector2.Distance(closestCoord, closestTkn.coord)) 
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
                closestTkn = manager.scenario.player.units[0];
                if (closestTkn) {
                    foreach (Unit tkn in manager.scenario.player.units) {
                        if (Vector2.Distance(tkn.coord, coord) < Vector2.Distance(closestTkn.coord, coord))
                            closestTkn = tkn;
                    }
                    Vector2 furthestCoord = closestTkn.coord;
                    foreach(Vector2 c in validActionCoords) {
                        Vector2 dir = Vector2.zero;
                        List<Vector2> validFurthestCoords = new List<Vector2>();
// Check in four directions
                        for (int i = 0; i < 4; i++) {
                            switch (i) {case 0: dir = Vector2.down; break; case 1: dir = Vector2.left; break; case 2: dir = Vector2.up; break; case 3: dir = Vector2.right; break;}
                            
                            for (int r = 1; r <= selectedEquipment.range; r++) {
                                Vector2 farCoord = closestTkn.coord + r * dir;
                                if (validActionCoords.Contains(farCoord) &&
                                Vector2.Distance(farCoord, closestTkn.coord) > Vector2.Distance(furthestCoord, closestTkn.coord)) 
                                    furthestCoord = farCoord;
                            }

                        }
                    }
// If there is a valid closest coord
                    if (furthestCoord != closestTkn.coord) {
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
    
}
