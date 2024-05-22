using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Gear/Slag/BigGrab")]
[System.Serializable]
public class BigGrabData : SlagGearData {
    List<GridElement> defaultFilters;
    public List<GridElement> firstTargets;
    public List<GridElement> upgradeTargets;

    public int grabRange;
    public bool flexibleSlime, maim, impact, landscaper, ontoOccupied;

    public override void EquipGear(Unit user) {
        base.EquipGear(user);
        flexibleSlime = false;
        maim = false;
        impact = false;
        landscaper = false;
        ontoOccupied = false;
        range = 3;
        grabRange = 1;
    }

    public override List<Vector2> TargetEquipment(GridElement user, int mod = 0) {
        if (firstTarget == null) {
            List<Vector2> validCoords = EquipmentAdjacency.OrthagonalAdjacency(user.coord, grabRange, firstTargets, firstTargets);
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
                            if (ge.elementCanvas)
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

    public override IEnumerator UseGear(GridElement user, GridElement target = null) {
        if (firstTarget == null) {          
            SpriteRenderer sr = Instantiate(vfx, user.grid.PosFromCoord(user.coord), Quaternion.identity).GetComponent<SpriteRenderer>();
            sr.sortingOrder = user.grid.SortOrderFromCoord(user.coord);
            firstTarget = target;
            contextualAnimGO = target.gameObject;
            Unit unit = (Unit)user;
            unit.grid.DisableGridHighlight();
            Vector2 dir = user.coord - target.coord;

// Nested TargetEquipment functionality inside of first UseGear
            List<Vector2> validCoords = EquipmentAdjacency.GetAdjacent(user.coord, range, this);

// SPECIAL TIER I -- Throw backwards
            if (!flexibleSlime) {
                if (dir.x != 0) {
                    int sign = (int)Mathf.Sign(dir.x);
                    for (int i = 1; i <= range; i++) {
                        Vector2 adj = new Vector2(user.coord.x + i*sign, user.coord.y);
                        if (validCoords.Contains(adj)) 
                            validCoords.Remove(adj);
                    }
                    for (int i = -range; i <= range; i++) {
                        Vector2 adj = new Vector2(user.coord.x, user.coord.y + i);
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
            if (!ontoOccupied) {
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

            yield return user.StartCoroutine(GrabUnit((Unit)user, firstTarget));
        } else {
            SpriteRenderer sr = Instantiate(vfx, user.grid.PosFromCoord(user.coord), Quaternion.identity).GetComponent<SpriteRenderer>();
            sr.sortingOrder = user.grid.SortOrderFromCoord(user.coord);
            
            Coroutine co = user.StartCoroutine(ThrowUnit((Unit)user, firstTarget, target.coord));
            
            OnEquipmentUse evt = ObjectiveEvents.OnEquipmentUse;
            evt.data = this; evt.user = user; evt.target = firstTarget;
            ObjectiveEventManager.Broadcast(evt);
            
            firstTarget = null;
            yield return base.UseGear(user);
            yield return co;
        }
    }

    public IEnumerator GrabUnit(Unit grabber, GridElement grabbed) {
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

    public IEnumerator ThrowUnit(Unit thrower, GridElement thrown, Vector2 coord) {
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
        List<Coroutine> cos = new();

// UNIT TIER II - Throw onto occupied spaces
        if (thrower.grid.CoordContents(coord).Count > 0) {
            GridElement subGE = null;
            foreach (GridElement ge in thrower.grid.CoordContents(coord)) {
                subGE = ge;
                cos.Add(ge.StartCoroutine(ge.CollideFromBelow(thrown, thrower, this)));
            }
            if (thrown is Unit u)
                cos.Add(u.StartCoroutine(u.CollideFromAbove(subGE, 0, thrower, this)));
        }

        if (thrown is EnemyUnit eu) {
            eu.ApplyCondition(Unit.Status.Stunned);
            if (maim) eu.moveMod = -1;
        }
        
// Deal damage on throw
        if (impact) cos.Add(thrown.StartCoroutine(thrown.TakeDamage(1, GridElement.DamageType.Fall, thrower, this)));


        if (thrown is Unit u2)
            u2.UpdateElement(coord, thrower, this);
        else
            thrown.UpdateElement(coord);

        for (int i = cos.Count - 1; i >= 0; i--) {
            if (cos[i] != null) yield return cos[i];
            else cos.RemoveAt(i);
        }
    }

    public override void UpgradeGear(GearUpgrade upgrade, int slotIndex) {
        base.UpgradeGear(upgrade, slotIndex);
    //     if (targetPath ==  UpgradePath.Sludge) {
    //         if (upgrades[targetPath] == 1) {
    //             slag.hpMax ++;
    //             slag.elementCanvas.InstantiateMaxPips();
    //             //slag.ui.overview.hPPips.UpdatePips();
    //             slag.StartCoroutine(slag.TakeDamage(-1, GridElement.DamageType.Heal));
    //         }
    //     } else if (targetPath == UpgradePath.Shunt) {
    //         if (upgrades[targetPath] == 0) {
    //             foreach (GridElement ge in upgradeTargets) {
    //                 if (firstTargets.Contains(ge)) firstTargets.Remove(ge);
    //             }
    //             range = 3;
    //         } else if (upgrades[targetPath] == 1) {
    //             slag.hpMax ++;
    //             slag.elementCanvas.InstantiateMaxPips();
    //             //slag.ui.overview.hPPips.UpdatePips();
    //             slag.StartCoroutine(slag.TakeDamage(-1, GridElement.DamageType.Heal));

    //             range += 2;
    //         } else if (upgrades[targetPath] == 2) {
    //             foreach (GridElement ge in upgradeTargets)
    //                 firstTargets.Add(ge);
    //         }
    //     }
    }
}
