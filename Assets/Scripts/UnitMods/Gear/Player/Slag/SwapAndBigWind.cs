using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Gear/Slag/Swap-BigWind")]
[System.Serializable]
public class SwapAndBigWind : SlagGearData
{
    enum MoveType { Swap, MoveAll, Throw };
    [SerializeField] MoveType moveType;
    Vector2 dir;
    [SerializeField] List<GridElement> firstTargets;

    public override List<Vector2> TargetEquipment(GridElement user, int mod = 0) {
        switch(moveType) {
            default:
            case MoveType.Swap:
            case MoveType.MoveAll:
                return base.TargetEquipment(user, mod);
            case MoveType.Throw:
                if (firstTarget == null) {
                    List<Vector2> validCoords = EquipmentAdjacency.OrthagonalAdjacency(user.coord, 1, firstTargets, firstTargets);
                    Unit u = (Unit)user;
                    u.inRangeCoords = validCoords;
                    user.grid.DisplayValidCoords(validCoords, gridColor);
                    for (int i = validCoords.Count - 1; i >= 0; i--) {
                        bool occupied = false;
                        bool remove = false;
                        foreach(GridElement ge in user.grid.CoordContents(validCoords[i])) {
                            occupied = true; remove = true;
                            foreach(GridElement target in firstTargets)
                            if (ge.GetType() == target.GetType()) {
                                remove = false;
                                if (ge is EnemyUnit)
                                    ge.elementCanvas.ToggleStatsDisplay(true);
                            }
                         
                        }
                        if (remove || !occupied) {
                            if (validCoords.Count >= i)
                                validCoords.Remove(validCoords[i]);
                        }
                    }
                    return validCoords;
                } else {
                    List<Vector2> validCoords = EquipmentAdjacency.GetAdjacent(user.coord, range + mod, this);
                    user.grid.DisplayValidCoords(validCoords, gridColor);
                    if (user is PlayerUnit u) u.ui.ToggleEquipmentButtons();
                    return validCoords;
                }           
        }
    }

    public override void UntargetEquipment(GridElement user)
    {
        base.UntargetEquipment(user);
        if (multiselect && firstTarget) {
            firstTarget.UpdateElement(firstTarget.coord);
            firstTarget = null;
        }
    }

    public override IEnumerator UseGear(GridElement user, GridElement target = null) {
        switch (moveType) {
            default: 
                yield return base.UseGear(user);
                yield return user.StartCoroutine(SwapUnits((Unit)user, (Unit)target)); 
                break;
            case MoveType.Swap: 
                yield return base.UseGear(user);
                yield return user.StartCoroutine(SwapUnits((Unit)user, (Unit)target)); 
                break;
            case MoveType.MoveAll:
                yield return base.UseGear(user);
                yield return user.StartCoroutine(MoveAllUnits(user, target.coord)); 
                break;
            case MoveType.Throw: 
                if (firstTarget == null) {
                    SpriteRenderer sr = Instantiate(vfx, user.grid.PosFromCoord(user.coord), Quaternion.identity).GetComponent<SpriteRenderer>();
                    sr.sortingOrder = user.grid.SortOrderFromCoord(user.coord);
                    firstTarget = target;
                    contextualAnimGO = target.gameObject;
                    Unit unit = (Unit)user;
                    unit.grid.DisableGridHighlight();
                    unit.inRangeCoords = TargetEquipment(user);
                    unit.validActionCoords = unit.inRangeCoords;
                    unit.grid.DisplayValidCoords(unit.validActionCoords, gridColor);
                    user.PlaySound(selectSFX);
                    yield return user.StartCoroutine(GrabUnit((Unit)user, (Unit)firstTarget));
                } else {
                    SpriteRenderer sr = Instantiate(vfx, user.grid.PosFromCoord(user.coord), Quaternion.identity).GetComponent<SpriteRenderer>();
                    sr.sortingOrder = user.grid.SortOrderFromCoord(user.coord);
                    Coroutine co = user.StartCoroutine(ThrowUnit((Unit)user, (Unit)firstTarget, target.coord));
                    firstTarget = null;
                    yield return base.UseGear(user);
                    yield return co;
                }
            break;
        }

        
    }

    public IEnumerator SwapUnits(Unit unit1, Unit unit2) {
        Instantiate(vfx, unit1.grid.PosFromCoord(unit1.coord), Quaternion.identity);
        Instantiate(vfx, unit1.grid.PosFromCoord(unit2.coord), Quaternion.identity);

        Vector2 to1 = unit2.coord; Vector2 to2 = unit1.coord;
        yield return null;
        unit1.UpdateElement(to1); unit2.UpdateElement(to2);

    }

    public IEnumerator GrabUnit(Unit grabber, Unit grabbed) {
        Vector3 origin = grabber.grid.PosFromCoord(grabber.coord);
        Vector3 target = grabbed.grid.PosFromCoord(grabbed.coord);
        Vector3 dif = target + (origin - target)/2;
        float timer = 0;
        while (timer < animDur/2) {

            grabbed.transform.position = Vector3.Lerp(target, dif, timer/(animDur/2));

            yield return null;
            timer += Time.deltaTime;
        }

    }

    public IEnumerator ThrowUnit(Unit thrower, Unit thrown, Vector2 coord) {
        Vector3 to = thrower.grid.PosFromCoord(coord);
        Vector3 origin = thrown.transform.position;
        float h = 0.25f + Vector2.Distance(thrower.coord, coord) / 2;

        float throwDur = 0.25f + animDur * Vector2.Distance(thrower.coord, coord);
        float timer = 0;
        while (timer < throwDur) {

            thrown.transform.position = Util.SampleParabola(origin, to, h, timer/throwDur);
            yield return null;
            timer += Time.deltaTime;
        }
        thrown.UpdateElement(coord);
        //thrown.StartCoroutine(thrown.TakeDamage(1));
    }

    public IEnumerator MoveAllUnits(GridElement user, Vector2 selection) {

        dir = (selection - user.coord).normalized;
        Animator anim = Instantiate(vfx).GetComponent<Animator>();
        anim.SetInteger("X", (int)dir.x); anim.SetInteger("Y", (int)dir.y);
        List<Unit> unitsToMove = new();
        foreach (GridElement ge in user.grid.gridElements) {
            if (ge is Unit u) {
                if (u is not Nail && u is not PlayerUnit && u is not Anvil) 
                    unitsToMove.Add(u);
            }
        }
        unitsToMove.Sort(SortByDir);
        Dictionary<Unit, Vector2> targetPositions = new();
        int moveRange = 1;
        foreach (Unit u in unitsToMove) {
            bool blocked = false;
            Vector2 frontier = u.coord;
            for (int i = 0; i <= moveRange; i++) {
                foreach (GridElement ge in user.grid.CoordContents(frontier + dir)) {
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
                foreach (KeyValuePair<Unit, Vector2> entry in targetPositions) {
                    if (frontier + dir == entry.Value) {
                        blocked = true;
                    }
                }
                if (frontier.x + dir.x < 0 || frontier.x + dir.x > 7 || frontier.y + dir.y < 0 || frontier.y + dir.y > 7) {
                    blocked = true;
                }
                if (i == moveRange) blocked = true;
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
