using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Slag/BigGrab")]
[System.Serializable]
public class BigGrabData : SlagEquipmentData {
 
    List<GridElement> defaultFilters;
    [SerializeField] List<GridElement> firstTargets;
    [SerializeField] List<GridElement> upgradeTargets;

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
        }
        else return null;
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

// Nested TargetEquipment functionality inside of first UseEquipment
            List<Vector2> validCoords = EquipmentAdjacency.GetAdjacent(user.coord, range, this);

// SPECIAL TIER I -- Throw backwards
            Vector2 dir = user.coord - firstTarget.coord;
            if (upgrades[UpgradePath.Special] == 0) {
                if (dir.x != 0) {
                    int sign = (int)Mathf.Sign(dir.x);
                    for (int i = 1; i <= range; i++) {
                        Vector2 adj = new Vector2(user.coord.x + i*sign, user.coord.y);
                        if (validCoords.Contains(adj)) 
                            validCoords.Remove(adj);
                    }
                } else {
                    int sign = (int)Mathf.Sign(dir.y);
                    for (int i = 1; i <= range; i++) {
                        Vector2 adj = new Vector2(user.coord.x, user.coord.y + i*sign);
                        if (validCoords.Contains(adj)) 
                            validCoords.Remove(adj);
                    }
                }
            }
// SPECIAL TIER II -- Throw sideways
            if (upgrades[UpgradePath.Special] <= 1) {
                if (dir.x != 0) {
                    for (int i = -range; i <= range; i++) {
                        Vector2 adj = new Vector2(user.coord.x, user.coord.y + i);
                        if (validCoords.Contains(adj)) 
                            validCoords.Remove(adj);
                    }
                } else {
                    for (int i = -range; i <= range; i++) {
                        Vector2 adj = new Vector2(user.coord.x + i, user.coord.y);
                        if (validCoords.Contains(adj))
                            validCoords.Remove(adj);
                    }
                }
            }
            user.grid.DisplayValidCoords(validCoords, gridColor);
            unit.inRangeCoords = validCoords;
// UNIT TIER II -- Throw onto occupied spaces
            if (upgrades[UpgradePath.Unit] < 2) {
                for (int i = validCoords.Count - 1; i >= 0; i--) {
                    bool remove = false;
                    foreach(GridElement ge in user.grid.CoordContents(validCoords[i])) {
                        remove = true;
                    }
                    if (remove) {
                        if (validCoords.Count >= i)
                            validCoords.Remove(validCoords[i]);
                    }
                }
            }
            unit.validActionCoords = validCoords;

            if (user is PlayerUnit u) u.ui.ToggleEquipmentButtons();
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
        if (upgrades[UpgradePath.Unit] >= 1) thrown.StartCoroutine(thrown.TakeDamage(1, GridElement.DamageType.Fall, thrower));
        thrown.UpdateElement(coord);
    }

    public override void UpgradeEquipment(UpgradePath targetPath) {
        base.UpgradeEquipment(targetPath);
        if (targetPath ==  UpgradePath.Unit) {
            if (upgrades[targetPath] == 1) {
                slag.hpMax ++;
                slag.hpCurrent ++;
                slag.elementCanvas.InstantiateMaxPips();
                slag.ui.overview.InstantiateMaxPips();
            }
        } else if (targetPath == UpgradePath.Power) {
            if (upgrades[targetPath] == 0) {
                foreach (GridElement ge in upgradeTargets) {
                    if (firstTargets.Contains(ge)) firstTargets.Remove(ge);
                }
                range = 3;
            } else if (upgrades[targetPath] == 1) {
                slag.hpMax ++;
                slag.hpCurrent ++;
                slag.elementCanvas.InstantiateMaxPips();
                slag.ui.overview.InstantiateMaxPips();

                range += 2;
            } else if (upgrades[targetPath] == 2) {
                foreach (GridElement ge in upgradeTargets)
                    firstTargets.Add(ge);
            }
        }
    }
}
