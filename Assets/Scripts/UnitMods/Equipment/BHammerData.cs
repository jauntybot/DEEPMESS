using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/BHammer")]
[System.Serializable]
public class BHammerData : HammerData
{

   
    public override List<Vector2> TargetEquipment(GridElement user, int mod = 0) {

        if (firstTarget == null) {
            List<Vector2> validCoords = EquipmentAdjacency.GetAdjacent(user, range + mod, this, targetTypes);
            user.grid.DisplayValidCoords(validCoords, gridColor);
            if (user is PlayerUnit pu) pu.ui.ToggleEquipmentButtons();
            for (int i = validCoords.Count - 1; i >= 0; i--) {
                bool occupied = false;
                foreach (GridElement ge in FloorManager.instance.currentFloor.CoordContents(validCoords[i])) {
                    if (ge is not GroundElement) occupied = true;
                    bool remove = true;
                    foreach(GridElement target in targetTypes) {
                        if (ge.GetType() == target.GetType()) {
                            if (ge is Nail n) {
                                if (n.nailState == Nail.NailState.Primed)
                                    remove = false;
                                else
                                    user.grid.sqrs.Find(sqr => sqr.coord == n.coord).ToggleValidCoord(false);
                            } else {
                                remove = false;
                                if (ge is EnemyUnit)
                                    ge.elementCanvas.ToggleStatsDisplay(true);
                            }
                        }
                    }
                    if (remove || !occupied) {
                        if (validCoords.Count >= i)
                            validCoords.Remove(validCoords[i]);
                    }
                } 
            }
            return validCoords;
        } else {
            List<GridElement> targets = new List<GridElement>(); targets.Add(user);
            return EquipmentAdjacency.OfTypeOnBoardAdjacency(user, targets, user.coord);        
        }        
    }

    public override void UntargetEquipment(GridElement user)
    {
        base.UntargetEquipment(user);
        firstTarget = null;
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

    public override IEnumerator ThrowHammer(PlayerUnit user, GridElement target, Unit passTo) {
        
        user.manager.DeselectUnit();

        PlayerManager manager = (PlayerManager)user.manager;
        if (target.gfx[0].sortingOrder > user.gfx[0].sortingOrder)
            hammer.GetComponentInChildren<SpriteRenderer>().sortingOrder = target.gfx[0].sortingOrder;
        AudioManager.PlaySound(AudioAtlas.Sound.hammerPass, user.transform.position);
        user.SwitchAnim(PlayerUnit.AnimState.Idle);
        hammer.SetActive(true);

// Lerp hammer to target
        float timer = 0;
        float throwDur = animDur * Vector2.Distance(user.coord, target.coord);
        Vector3 startPos = hammer.transform.position;
        Vector3 endPos = FloorManager.instance.currentFloor.PosFromCoord(target.coord);
        while (timer < throwDur) {
            hammer.transform.position = Vector3.Lerp(startPos, endPos, timer/throwDur);
            yield return null;
            timer += Time.deltaTime;
        }

        AudioManager.PlaySound(AudioAtlas.Sound.attackStrike, user.transform.position);

// Attack target if unit
        if (target is EnemyUnit) 
            target.StartCoroutine(target.TakeDamage(1));
        else if (target is Nail n) {
            if (n.nailState == Nail.NailState.Primed)
                n.ToggleNailState(Nail.NailState.Buried);
        }
// Lerp hammer to passTo unit
        if (passTo.gfx[0].sortingOrder > hammer.GetComponentInChildren<SpriteRenderer>().sortingOrder)  
            hammer.GetComponentInChildren<SpriteRenderer>().sortingOrder = passTo.gfx[0].sortingOrder;
        timer = 0;
        throwDur = animDur * Vector2.Distance(target.coord, passTo.coord);
        startPos = hammer.transform.position;
        endPos = FloorManager.instance.currentFloor.PosFromCoord(passTo.coord);
        while (timer < throwDur) {
            hammer.transform.position = Vector3.Lerp(startPos, endPos, timer/throwDur);
            yield return null;
            timer += Time.deltaTime;
        }
    
        PassHammer((PlayerUnit)user, (PlayerUnit)passTo);
        hammer.SetActive(false);

        if (target is Nail) {
            yield return new WaitForSecondsRealtime(0.25f);
            manager.TriggerDescent();
        }

    }
}
