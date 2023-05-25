using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/BHammer")]
[System.Serializable]
public class HammerData : EquipmentData
{
    public GameObject hammer;
    public Nail nail;
    public enum Action { Lob, Strike };
    public Action action;
    GridElement.DamageType dmgType = GridElement.DamageType.Melee;
    public SFX throwSFX, catchSFX, nailSFX, shellSFX;
   
    public void AssignHammer(GameObject h, Nail d) {
        hammer = h;
        nail = d;
    }

    public override List<Vector2> TargetEquipment(GridElement user, int mod = 0) {

        if (firstTarget == null) {
            PlayerUnit pu = (PlayerUnit)user;
            pu.SwitchAnim(PlayerUnit.AnimState.Idle);
            hammer.SetActive(true);
            pu.ui.ToggleEquipmentButtons();
            pu.inRangeCoords = EquipmentAdjacency.GetAdjacent(user, range + mod, this, targetTypes);
            List<Vector2> validCoords = EquipmentAdjacency.GetAdjacent(user, range + mod, this, targetTypes);
            Debug.Log(pu.inRangeCoords.Count);
            user.grid.DisplayValidCoords(validCoords, gridColor);
            for (int i = validCoords.Count - 1; i >= 0; i--) {
                bool occupied = false;
                bool remove = false;
                foreach (GridElement ge in FloorManager.instance.currentFloor.CoordContents(validCoords[i])) {
                    if (ge is not GroundElement) occupied = true;
                    if (ge is PlayerUnit u) {
                        if (u.conditions.Contains(Unit.Status.Disabled)) 
                            remove = false;
                    } else {
                        foreach(GridElement target in targetTypes) {
                            if (ge.GetType() == target.GetType()) {
                                if (ge is Nail n) {
                                    if (n.nailState == Nail.NailState.Primed)
                                        remove = false;
                                    else {
                                        user.grid.sqrs.Find(sqr => sqr.coord == n.coord).ToggleValidCoord(false);
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
            Debug.Log(pu.inRangeCoords.Count);
            return validCoords;
        } else {
            List<GridElement> targets = new List<GridElement>(); targets.Add(user);
            List<Vector2> _coords = EquipmentAdjacency.OfTypeOnBoardAdjacency(user, targets, user.coord);        
            
            Unit unit = (Unit)user;

            List<Vector2> allCoords = new List<Vector2>();
            foreach (GridSquare sqr in user.grid.sqrs)
                allCoords.Add(sqr.coord);
            unit.inRangeCoords = allCoords;

            foreach (Unit u in unit.manager.units) {
                if (u.conditions.Contains(Unit.Status.Disabled)) {
                    if (firstTarget != u && _coords.Contains(u.coord))
                        _coords.Remove(u.coord);
                }
            }
            return _coords;
        }        
    }

    public override void UntargetEquipment(GridElement user)
    {
        base.UntargetEquipment(user);
        firstTarget = null;
        PlayerUnit pu = (PlayerUnit)user;
        pu.SwitchAnim(PlayerUnit.AnimState.Hammer);
        hammer.SetActive(false);
    }

    public override IEnumerator UseEquipment(GridElement user, GridElement target = null)
    {
        if (firstTarget != null) {
        // REPLACE w/ BASE.USEEQUIPMENT IF BROUGHT BACK INTO WORKING SCRIPTS
            user.energyCurrent -= energyCost;
            if (user is PlayerUnit pu) {
                PlayerManager manager = (PlayerManager)pu.manager;
                manager.undoableMoves = new Dictionary<Unit, Vector2>();
                manager.undoOrder = new List<Unit>();
            }
            user.elementCanvas.UpdateStatsDisplay();
            yield return user.StartCoroutine(ThrowHammer((PlayerUnit)user, firstTarget, (PlayerUnit)target));
        } else {
            firstTarget = target;
            Unit unit = (Unit)user;
            unit.grid.DisableGridHighlight();

            unit.validActionCoords = TargetEquipment(user);
            unit.grid.DisplayValidCoords(unit.validActionCoords, gridColor);
            foreach (Unit u in unit.manager.units) {
                if (u is PlayerUnit)
                    u.TargetElement(true);
            }
            if (selectSFX)
                user.PlaySound(selectSFX);
                
            yield return null;
        }
    }

    public virtual IEnumerator ThrowHammer(PlayerUnit user, GridElement target = null, Unit passTo = null) {
        
        user.manager.DeselectUnit();
    
        
        user.PlaySound(throwSFX);

        user.SwitchAnim(PlayerUnit.AnimState.Idle);
        hammer.SetActive(true);

        Vector2 prevCoord = user.coord;

        Coroutine targetCo = null;
// Lerp hammer to target
        if (target != null) {
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
            if (target is EnemyUnit) {
                if (target.shell)
                    target.PlaySound(shellSFX);
                else 
                    target.PlaySound(useSFX);
                
                Instantiate(vfx, user.grid.PosFromCoord(target.coord) + new Vector3(0, 1, 0), Quaternion.identity);
                targetCo = target.StartCoroutine(target.TakeDamage(1, dmgType));
            }
// Trigger descent if nail
            else if (target is Nail n) {
                
                target.PlaySound(nailSFX);
                if (n.nailState == Nail.NailState.Primed)
                    n.ToggleNailState(Nail.NailState.Buried);
                Instantiate(vfx, user.grid.PosFromCoord(target.coord) + new Vector3(0, 1, 0), Quaternion.identity);
            } else if (target is PlayerUnit pu) {
                
                user.PlaySound(catchSFX);
                if (pu.conditions.Contains(Unit.Status.Disabled))
                    pu.Stabilize();
            }
        }

        if (throwSFX)
            user.PlaySound(throwSFX);

// Lerp hammer to passTo unit
        if (passTo != null) {
            if (passTo.gfx[0].sortingOrder > hammer.GetComponentInChildren<SpriteRenderer>().sortingOrder)  
                hammer.GetComponentInChildren<SpriteRenderer>().sortingOrder = passTo.gfx[0].sortingOrder;
            float timer = 0;
            float throwDur = 0.25f + animDur * Vector2.Distance(prevCoord, passTo.coord);
            Vector3 startPos = hammer.transform.position;
            Vector3 endPos = FloorManager.instance.currentFloor.PosFromCoord(passTo.coord);
            float h = 0.25f + Vector2.Distance(prevCoord, passTo.coord) / 2;
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
        if (targetCo != null)
            yield return targetCo;
        if (target is Nail) {
            PlayerManager manager = (PlayerManager)user.manager;
            yield return new WaitForSecondsRealtime(0.25f);
            manager.TriggerDescent(user.grid.enemy is TutorialEnemyManager);
        }

    }

    
    public virtual void PassHammer(PlayerUnit sender, PlayerUnit reciever) {
        List<EquipmentData> toAdd = new List<EquipmentData>();
        for (int i = sender.equipment.Count - 1; i >= 0; i--) {
            if (sender.equipment[i] is HammerData) {
                toAdd.Add(sender.equipment[i]);

                sender.equipment.Remove(sender.equipment[i]);
            }
        }
        for (int i = toAdd.Count - 1; i >= 0; i--) 
            reciever.equipment.Add(toAdd[i]);

        sender.gfx.Remove(hammer.GetComponentInChildren<SpriteRenderer>());
        hammer.transform.parent = reciever.transform;
        reciever.gfx.Add(hammer.GetComponentInChildren<SpriteRenderer>());
        reciever.SwitchAnim(PlayerUnit.AnimState.Hammer);

        reciever.ui.UpdateEquipmentButtons();
        sender.ui.UpdateEquipmentButtons();
    }


}
