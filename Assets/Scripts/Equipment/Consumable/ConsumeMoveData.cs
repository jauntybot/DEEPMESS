using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Consumable/Move")]
[System.Serializable]
public class ConsumeMoveData : ConsumableEquipmentData
{
    enum MoveType { Swap, MoveAll};
    [SerializeField] MoveType moveType;
    Vector2 dir;

    public override IEnumerator UseEquipment(GridElement user, GridElement target = null) {
        yield return base.UseEquipment(user);
        switch (moveType) {
            default: yield return user.StartCoroutine(SwapUnits((Unit)user, (Unit)target)); break;
            case MoveType.Swap: yield return user.StartCoroutine(SwapUnits((Unit)user, (Unit)target)); break;
            case MoveType.MoveAll: yield return user.StartCoroutine(MoveAllUnits(user, target.coord)); break;
        }

        
    }

    public IEnumerator SwapUnits(Unit unit1, Unit unit2) {

        Vector2 to1 = unit2.coord; Vector2 to2 = unit1.coord;
        yield return null;
        unit1.UpdateElement(to1); unit2.UpdateElement(to2);

    }

    public IEnumerator MoveAllUnits(GridElement user, Vector2 selection) {

        dir = selection - user.coord;
        List<Unit> unitsToMove = new List<Unit>();
        foreach (GridElement ge in user.grid.gridElements) {
            if (ge is Unit u) {
                if (u is not Nail) 
                    unitsToMove.Add(u);
            }
        }
        unitsToMove.Sort(SortByDir);
        Dictionary<Unit, Vector2> targetPositions = new Dictionary<Unit, Vector2>();
        foreach (Unit u in unitsToMove) {
            bool blocked = false;
            Vector2 frontier = u.coord;
            for (int i = 0; i <= 7; i++) {
                foreach (GridElement ge in user.grid.CoordContents(frontier + dir)) {
                    if (ge is not GroundElement) {
                        if (ge is Unit u2) {
                            if (targetPositions.ContainsKey(u2)) {
                                if (targetPositions[u2] == frontier + dir) {
                                    blocked = true;                         
                                } 
                            } else {
                                blocked = true;
                            }
                        } 
                        else {
                            blocked = true;
                        }
                    }
                }
                foreach (KeyValuePair<Unit, Vector2> entry in targetPositions) {
                    if (frontier + dir == entry.Value) {
                        blocked = true;
                    }
                }
                if (frontier.x + dir.x < 0 || frontier.x + dir.x > 7 || frontier.y + dir.y < 0 || frontier.y + dir.y > 7) {
                    blocked = true;
                }
                if (blocked) {
                    targetPositions.Add(u, frontier);
                    break;
                }
                frontier += dir;
            }
        }

        foreach (KeyValuePair<Unit, Vector2> entry in targetPositions) {
            if (user.grid.SortOrderFromCoord(entry.Value) > user.grid.SortOrderFromCoord(entry.Key.coord))
                entry.Key.UpdateSortOrder(entry.Value);
        }

        float timer = 0;

        while (timer < animDur) {
            yield return null;
            foreach (KeyValuePair<Unit, Vector2> entry in targetPositions) {
                entry.Key.transform.position = Vector3.Lerp(entry.Key.transform.position, user.grid.PosFromCoord(entry.Value), timer/animDur);
            }
            timer += Time.deltaTime;
        }
        foreach (KeyValuePair<Unit, Vector2> entry in targetPositions) 
            entry.Key.UpdateElement(entry.Value);
        

        foreach (KeyValuePair<Unit, Vector2> entry in targetPositions) {

        }
        yield return null;
    }

    int SortByDir(GridElement ge1, GridElement ge2) {

        if (dir.x < 0) {
            if ((dir.x * 8) - ge1.coord.x < (dir.x * 8) - ge2.coord.x) return 1;
            else if ((dir.x * 8) - ge1.coord.x == (dir.x * 8) - ge2.coord.x) return 0;
            else return -1;
        }
        else if (dir.x > 0) {
            if ((dir.x * 8) - ge1.coord.x > (dir.x * 8) - ge2.coord.x) return 1;
            else if ((dir.x * 8) - ge1.coord.x == (dir.x * 8) - ge2.coord.x) return 0;
            else return -1;
        }
        else if (dir.y < 0) {
            if ((dir.y * 8) - ge1.coord.y < (dir.y * 8) - ge2.coord.y) return 1;
            else if ((dir.y * 8) - ge1.coord.y == (dir.y * 8) - ge2.coord.y) return 0;
            else return -1;
        } 
        else if (dir.y > 0) {
            if ((dir.y * 8) - ge1.coord.y > (dir.y * 8) - ge2.coord.y) return 1;
            else if ((dir.y * 8) - ge1.coord.y == (dir.y * 8) - ge2.coord.y) return 0;
            else return -1;
        }
        return 0;
    }

}
