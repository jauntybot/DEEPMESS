using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/BHammer")]
[System.Serializable]
public class BHammerData : HammerData
{

   
    public override List<Vector2> TargetEquipment(GridElement user, int mod = 0) {

        if (firstTarget == null) {
            PlayerUnit pu = (PlayerUnit)user;
            pu.SwitchAnim(PlayerUnit.AnimState.Idle);
            hammer.SetActive(true);
            pu.ui.ToggleEquipmentButtons();
            List<Vector2> validCoords = EquipmentAdjacency.GetAdjacent(user, range + mod, this, targetTypes);
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
            return validCoords;
        } else {
            List<GridElement> targets = new List<GridElement>(); targets.Add(user);
            List<Vector2> _coords = EquipmentAdjacency.OfTypeOnBoardAdjacency(user, targets, user.coord);        
            
            Unit unit = (Unit)user;
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
            Debug.Log("Click lob");
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
            Debug.Log("Click strike");
            firstTarget = target;
            Unit unit = (Unit)user;
            unit.grid.DisableGridHighlight();
            unit.validActionCoords = TargetEquipment(user);
            unit.grid.DisplayValidCoords(unit.validActionCoords, gridColor);
            foreach (Unit u in unit.manager.units) {
                if (u is PlayerUnit)
                    u.TargetElement(true);
            }
            yield return null;
        }
    }

    public override void EquipEquipment(GridElement user)
    {
        base.EquipEquipment(user);
    }

    public override IEnumerator ThrowHammer(PlayerUnit user, GridElement target = null, Unit passTo = null) {
        
        user.manager.DeselectUnit();

        AudioManager.PlaySound(AudioAtlas.Sound.hammerPass, user.transform.position);
        user.SwitchAnim(PlayerUnit.AnimState.Idle);
        hammer.SetActive(true);

        Vector2 prevCoord = user.coord;

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

            AudioManager.PlaySound(AudioAtlas.Sound.attackStrike, user.transform.position);

    // Attack target if unit
            if (target is EnemyUnit) 
                target.StartCoroutine(target.TakeDamage(1));
            else if (target is Nail n) {
                if (n.nailState == Nail.NailState.Primed)
                    n.ToggleNailState(Nail.NailState.Buried);
            } else if (target is PlayerUnit pu) {
                if (pu.conditions.Contains(Unit.Status.Disabled))
                    pu.Stabilize();
            }
        }
// Lerp hammer to passTo unit
        if (passTo != null) {
            if (passTo.gfx[0].sortingOrder > hammer.GetComponentInChildren<SpriteRenderer>().sortingOrder)  
                hammer.GetComponentInChildren<SpriteRenderer>().sortingOrder = passTo.gfx[0].sortingOrder;
            float timer = 0;
            float throwDur = animDur * Vector2.Distance(prevCoord, passTo.coord);
            Vector3 startPos = hammer.transform.position;
            Vector3 endPos = FloorManager.instance.currentFloor.PosFromCoord(passTo.coord);
            while (timer < throwDur) {
                hammer.transform.position = Vector3.Lerp(startPos, endPos, timer/throwDur);
                yield return null;
                timer += Time.deltaTime;
            }
            hammer.transform.position = endPos;
            
            PassHammer((PlayerUnit)user, (PlayerUnit)passTo);
            hammer.SetActive(false);
        }
        if (target is Nail) {
            PlayerManager manager = (PlayerManager)user.manager;
            yield return new WaitForSecondsRealtime(0.25f);
            manager.TriggerDescent();
        }
    }
}
