using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Slag/BigGrab")]
[System.Serializable]
public class BigGrabData : SlagEquipmentData {
 
    int carryCapacity;
    List<GridElement> defaultFilters;
    [SerializeField] List<GridElement> upgradeTargets;
    [SerializeField] List<GridElement> firstTargets;

    public override List<Vector2> TargetEquipment(GridElement user, int mod = 0) {
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
                    foreach(GridElement target in firstTargets) {
                        if (ge.GetType() == target.GetType()) {
                            ge.elementCanvas.ToggleStatsDisplay(true);
                            if (ge.hpCurrent <= carryCapacity)
                                remove = false;
                        }
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

    public override void UntargetEquipment(GridElement user) {
        base.UntargetEquipment(user);
        if (multiselect && firstTarget) {
            firstTarget.UpdateElement(firstTarget.coord);
            firstTarget = null;
        }
    }

    public override IEnumerator UseEquipment(GridElement user, GridElement target = null) {
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
            yield return base.UseEquipment(user);
            yield return co;
        }
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
        bool collisionChecked = false;
        while (timer < throwDur) {
            thrown.transform.position = Util.SampleParabola(origin, to, h, timer/throwDur);
            yield return null;
            timer += Time.deltaTime;
            if (timer >= throwDur/2 && !collisionChecked) {
                collisionChecked = true;
            }
        }
        if (thrower.grid.CoordContents(coord).Count > 0) {
            GridElement subGE = null;
            foreach (GridElement ge in thrower.grid.CoordContents(coord)) {
                subGE = ge;
                ge.StartCoroutine(ge.CollideFromBelow(thrown));
            }
            thrown.StartCoroutine(thrown.CollideFromAbove(subGE));
        }
        thrown.UpdateElement(coord);
    }

    public override void UpgradeEquipment(Unit user, UpgradePath targetPath) {
        base.UpgradeEquipment(user, targetPath);
        if (targetPath ==  UpgradePath.Unit) {
            if (upgrades[targetPath] == 0) {
                slag.hpMax = 3;
                if (slag.hpCurrent > slag.hpMax) slag.hpCurrent = slag.hpMax;
                slag.elementCanvas.InstantiateMaxPips();
                slag.ui.overview.InstantiateMaxPips();
            } else if (upgrades[targetPath] == 1) {
                slag.hpMax = 4;
                slag.hpCurrent ++;
                slag.elementCanvas.InstantiateMaxPips();
                slag.ui.overview.InstantiateMaxPips();
            } else if (upgrades[targetPath] == 2) {
                slag.hpMax = 5;
                slag.hpCurrent ++;
                slag.elementCanvas.InstantiateMaxPips();
                slag.ui.overview.InstantiateMaxPips();
            }
            
        } else if (targetPath == UpgradePath.Power) {
            if (upgrades[targetPath] == 0) {
                range = 1;
                filters = defaultFilters;
            } else if (upgrades[targetPath] == 1) {
                range = 2;
            } else if (upgrades[targetPath] == 2) {
                range = 4;
            } else if (upgrades[targetPath] == 3) {
                filters = new();
            }
        } else if (targetPath == UpgradePath.Special) {
            if (upgrades[targetPath] == 0) {
                foreach (GridElement ge in upgradeTargets) {
                    if (firstTargets.Contains(ge)) firstTargets.Remove(ge);
                }
                carryCapacity = 1;
            } else if (upgrades[targetPath] == 1) {
                carryCapacity = 2;
            } else if (upgrades[targetPath] == 2) {
                foreach (GridElement ge in upgradeTargets) 
                    firstTargets.Add(ge);
            } else if (upgrades[targetPath] == 3) {
                carryCapacity = 4;
            }
        }
    }
}
