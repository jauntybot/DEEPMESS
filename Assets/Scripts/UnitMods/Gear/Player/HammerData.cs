using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[CreateAssetMenu(menuName = "Gear/HammerData")]
[System.Serializable]
public class HammerData : GearData {
    
    public int dmg = 1;
    public int dmgMod = 0;
    public GridElement secondTarget;
    public GameObject hammer;
    public Nail nail;
    public enum Action { Lob, Strike };
    public Action action;
    GridElement.DamageType dmgType = GridElement.DamageType.Melee;
    [SerializeField] GameObject meleeVFX;
    public SFX throwSFX, catchSFX, nailSFX, shieldSFX, meleeSFX;
   
    public void AssignHammer(GameObject h, Nail d) {
        hammer = h;
        nail = d;
    }
    
    public virtual void EquipGear(Unit user, bool first) {
        base.EquipGear(user);
        dmgMod = 0;
    }

    public override void EquipGear(Unit user) {
        if (user is PlayerUnit pu)
            pu.ui.UpdateEquipmentButtons();
        dmgMod = 0;
    }

    public override List<Vector2> TargetEquipment(GridElement user, int mod = 0) {
        if (firstTarget == null) {
            return TargetThrow((PlayerUnit)user, user.coord);
        } else {
// POWER TIER I - Target an additional enemy before Slag
//             if (upgrades[UpgradePath.Shunt] == 1 && secondTarget == null) {
//                 return TargetThrow((PlayerUnit)user, firstTarget.coord);
// // Target Slags
//             } else {
                Unit unit = (Unit)user;

                List<GridElement> targets = new(); targets.Add(user);
                List<Vector2> _coords = EquipmentAdjacency.OfTypeOnBoardAdjacency(targets, user.grid);        
                unit.grid.DisplayValidCoords(_coords, gridColor);


                List<Vector2> allCoords = new();
                foreach (Tile sqr in user.grid.tiles)
                    allCoords.Add(sqr.coord);
                unit.inRangeCoords = allCoords;

                foreach (Unit u in unit.manager.units) {
                    if (u.conditions.Contains(Unit.Status.Disabled)) {
                        if (firstTarget != u && _coords.Contains(u.coord))
                            _coords.Remove(u.coord);
                    }
                }
                return _coords;
            //}
        }        
    }

    List<Vector2> TargetThrow(PlayerUnit pu, Vector2 origin) {
        pu.SwitchAnim(PlayerUnit.AnimState.Idle);
        hammer.SetActive(true);
        pu.ui.ToggleEquipmentButtons();
        List<Vector2> validCoords = EquipmentAdjacency.GetAdjacent(origin, range, this, targetTypes);
        pu.inRangeCoords = EquipmentAdjacency.GetAdjacent(origin, range, this);
        foreach (Vector2 coord in validCoords)
            pu.inRangeCoords.Add(coord);
        pu.grid.DisplayValidCoords(validCoords, gridColor);
        for (int i = validCoords.Count - 1; i >= 0; i--) {
            bool occupied = false;
            bool remove = false;
            foreach (GridElement ge in FloorManager.instance.currentFloor.CoordContents(validCoords[i])) {
                occupied = true;
                Tile tile = pu.grid.tiles.Find(sqr => sqr.coord == ge.coord);
                if (ge is PlayerUnit u) {
                    if (u.conditions.Contains(Unit.Status.Disabled) && tile.tileType == Tile.TileType.Bile) 
                        remove = true;
                } else {
                    foreach(GridElement target in targetTypes) {
                        if (ge.GetType() == target.GetType()) {
                            if (ge is Nail n) {
                                if (n.nailState == Nail.NailState.Primed)
                                    remove = false;
                                else {
                                    tile.ToggleValidCoord(false);
                                    remove = true;
                                }
                            } else {
                                remove = false;
                                if (ge is EnemyUnit)
                                    ge.elementCanvas.ToggleStatsDisplay(true);
                            }
                        }
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

    public override void UntargetEquipment(GridElement user) {
        base.UntargetEquipment(user);
        Debug.Log("untarget equip");
        firstTarget = null;
        secondTarget = null;
        PlayerUnit pu = (PlayerUnit)user;
        pu.SwitchAnim(PlayerUnit.AnimState.Hammer);
        hammer.SetActive(false);
    }

    public override IEnumerator UseGear(GridElement user, GridElement target = null) {
// Second input, throw hammer
        if (firstTarget != null) {
            if (user is PlayerUnit pu) {
                PlayerManager manager = (PlayerManager)pu.manager;
                manager.undoableMoves = new Dictionary<Unit, Vector2>();
                manager.undoOrder = new List<Unit>();
            }

            Debug.Log(secondTarget);
            OnEquipmentUse evt = ObjectiveEvents.OnEquipmentUse;
            evt.data = this; evt.user = user; evt.target = firstTarget; evt.secondTarget = target;
            ObjectiveEventManager.Broadcast(evt);

            user.elementCanvas.UpdateStatsDisplay();
            yield return user.StartCoroutine(LaunchHammer((PlayerUnit)user, firstTarget, (PlayerUnit)target));    
            user.energyCurrent -= energyCost;

// First input, setup for second input
        } else {
            firstTarget = target;
            Unit unit = (Unit)user;
            unit.grid.DisableGridHighlight();

            unit.validActionCoords = TargetEquipment(user);
            
            foreach (Unit u in unit.manager.units) {
                if (u is PlayerUnit)
                    u.TargetElement(true);
            }
            if (selectSFX)
                user.PlaySound(selectSFX);
        }
    }

    public virtual IEnumerator LaunchHammer(PlayerUnit user, GridElement target = null, Unit passTo = null) {
        
        user.manager.DeselectUnit();
    
        user.PlaySound(throwSFX);

        user.SwitchAnim(PlayerUnit.AnimState.Idle);
        hammer.SetActive(true);


        Coroutine targetCo = null;
        Coroutine pushCo = null;
        Vector2 hammerPos = new();

// Throw hammer at first target
        if (target != null) 
            yield return user.StartCoroutine(ThrowHammer(user, passTo, target, result => hammerPos = result, co1 => targetCo = co1, Color32 => pushCo = Color32));

        if (throwSFX)
            user.PlaySound(throwSFX);

// POWER TIER I - Ricochet hammer to second target before passing to slag
        // if (target2 != null)  {
        //     Debug.Log("second throw");
        //     yield return user.StartCoroutine(ThrowHammer(user, target2, result => hammerPos = result, co1 => targetCo = co1, Color32 => pushCo = Color32));
        // }

        if (throwSFX)
            user.PlaySound(throwSFX);
// Lob hammer to passTo unit
        if (passTo != null) {
            if (passTo.gfx[0].sortingOrder > hammer.GetComponentInChildren<SpriteRenderer>().sortingOrder)  
                hammer.GetComponentInChildren<SpriteRenderer>().sortingOrder = passTo.gfx[0].sortingOrder;
            float timer = 0;
            float throwDur = 0.25f + animDur * Vector2.Distance(hammerPos, passTo.coord);
            Vector3 startPos = hammer.transform.position;
            Vector3 endPos = user.grid.PosFromCoord(passTo.coord);
            float h = 0.25f + Vector2.Distance(hammerPos, passTo.coord) / 2;
            while (timer < throwDur) {
                hammer.transform.position = Util.SampleParabola(startPos, endPos, h, timer/throwDur);
                yield return null;
                timer += Time.deltaTime;
            }
            hammer.transform.position = endPos;

            user.PlaySound(catchSFX);
            PassHammer((PlayerUnit)user, (PlayerUnit)passTo);
                 
            hammer.SetActive(false);
        }
        hammer.transform.localPosition = new();
        
        if (targetCo != null)
            yield return targetCo;
        if (pushCo != null)
            yield return targetCo;
    }

    IEnumerator ThrowHammer(Unit user, Unit passTo, GridElement target, Action<Vector2> hammerPos, Action<Coroutine> dmgCo, Action<Coroutine> pushCo) {
        Vector2 prevCoord = user.coord;
        if (target.gfx[0].sortingOrder > user.gfx[0].sortingOrder)
                hammer.GetComponentInChildren<SpriteRenderer>().sortingOrder = target.gfx[0].sortingOrder;

        float timer = 0;
        float throwDur = animDur * Vector2.Distance(prevCoord, target.coord);
        Vector3 startPos = hammer.transform.position;
        Vector3 endPos = FloorManager.instance.currentFloor.PosFromCoord(target.coord);
        while (timer < throwDur) {
            hammer.transform.position = Vector3.Lerp(startPos, endPos, timer/throwDur);
            yield return null;
            timer += Time.deltaTime;
        }
        hammer.transform.position = endPos;
        prevCoord = target.coord;                  

// Attack target if unit
        if (target is EnemyUnit || target is Wall) {
            target.PlaySound(useSFX);
            
            Instantiate(vfx, user.grid.PosFromCoord(target.coord) + new Vector3(0, 1, 0), Quaternion.identity);


            dmgCo(target.StartCoroutine(target.TakeDamage(dmg + dmgMod, dmgType, user, sourceEquip: this)));
        }
// Trigger descent if nail
        else if (target is Nail n) {
            target.PlaySound(nailSFX);
            if (n.nailState == Nail.NailState.Primed)
                n.ToggleNailState(Nail.NailState.Buried);
            Instantiate(vfx, user.grid.PosFromCoord(target.coord) + new Vector3(0, 1, 0), Quaternion.identity);
            PlayerManager manager = (PlayerManager)user.manager;
            manager.TriggerDescent();
        } 
// Revive slag
        else if (target is PlayerUnit pu) { 
            user.PlaySound(catchSFX);
            if (pu.conditions.Contains(Unit.Status.Disabled)) 
                pu.Stabilize();
        }

        hammerPos(prevCoord);

    }

    public virtual void PassHammer(PlayerUnit sender, PlayerUnit reciever) {
        if (reciever.conditions.Contains(Unit.Status.Disabled)) {
            List<Unit> possiblePasses = new();
            foreach (Unit u in sender.pManager.units) {
                if (u is PlayerUnit && !u.conditions.Contains(Unit.Status.Disabled) && u != this)
                    possiblePasses.Add(u);
            }
            if (possiblePasses.Count > 0) {                
                reciever.StartCoroutine(LaunchHammer(reciever, null, possiblePasses[UnityEngine.Random.Range(0, possiblePasses.Count)]));   
            }
        }
        List<GearData> toAdd = new();
        for (int i = sender.equipment.Count - 1; i >= 0; i--) {
            if (sender.equipment[i] is HammerData h) {
                toAdd.Add(h);

                sender.equipment.Remove(h);
            }
        }
        for (int i = toAdd.Count - 1; i >= 0; i--) {
            reciever.equipment.Add(toAdd[i]);
            toAdd[i].EquipGear(reciever);
        }

        sender.gfx.Remove(hammer.GetComponentInChildren<SpriteRenderer>());
        hammer.transform.parent = reciever.transform;
        reciever.gfx.Add(hammer.GetComponentInChildren<SpriteRenderer>());
        reciever.SwitchAnim(PlayerUnit.AnimState.Hammer);

        reciever.ui.UpdateEquipmentButtons();
        sender.ui.UpdateEquipmentButtons();
    }
}
