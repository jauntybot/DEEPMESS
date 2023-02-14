using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Hammer")]
[System.Serializable]
public class HammerData : EquipmentData
{

    public GameObject hammer;
    public Drill drill;
    public enum Action { Lob, Strike };
    public Action action;

    
    public void AssignHammer(GameObject h, Drill d) {
        hammer = h;
        drill = d;
    }
    
    public override List<Vector2> TargetEquipment(GridElement user) {
        switch (action) {
            default:
                return base.TargetEquipment(user);
            case Action.Lob:
                return base.TargetEquipment(user);
            case Action.Strike:
                List<GridElement> targetLast = new List<GridElement>();
                targetLast.Add(drill);
                List<Vector2> validCoords = EquipmentAdjacency.GetAdjacent(user, this, targetLast);
                user.grid.DisplayValidCoords(validCoords, gridColor);
                if (user is PlayerUnit pu) pu.canvas.ToggleEquipmentDisplay(false);
                for (int i = validCoords.Count - 1; i >= 0; i--) {
                    if (FloorManager.instance.currentFloor.CoordContents(validCoords[i]) is Unit u) {
                        foreach(GridElement target in targetLast) {
                            if (u.GetType() != target.GetType())
                                validCoords.Remove(validCoords[i]);
                            else if (u is Drill d) {
                                PlayerUnit playerUnit = (PlayerUnit)user;
                                PlayerManager manager = (PlayerManager)playerUnit.manager;
                                if (manager.hammerCharge < manager.descentChargeReq) {
                                    validCoords.Remove(validCoords[i]);
                                }
                            }
                        }
                    } else 
                        validCoords.Remove(validCoords[i]);
                }
            return validCoords;
        }
    }

    public override IEnumerator UseEquipment(GridElement user, GridElement target = null)
    {
        yield return base.UseEquipment(user, target);
        switch (action) {
            case Action.Lob:
                yield return user.StartCoroutine(LobHammer((PlayerUnit)user, (PlayerUnit)target));
            break;
            case Action.Strike:
                yield return user.StartCoroutine(StrikeHammer(user, target));
            break;
        }
    }

    public override void EquipEquipment(GridElement user)
    {
        base.EquipEquipment(user);
    }

    public IEnumerator LobHammer(PlayerUnit passer, PlayerUnit passTo) {

        PlayerManager manager = (PlayerManager)passer.manager;
        manager.ChargeHammer(1);

        float timer = 0;

        while (timer < animDur) {
            hammer.transform.position = Vector3.Lerp(hammer.transform.position, FloorManager.instance.currentFloor.PosFromCoord(passTo.coord), timer/animDur);
            yield return null;
            timer += Time.deltaTime;
        }

        for (int i = passer.equipment.Count - 1; i >= 0; i--) {
            if (passer.equipment[i] is HammerData) {
                passTo.equipment.Add(passer.equipment[i]);
                
                passer.equipment.Remove(passer.equipment[i]);
            }
        }
        hammer.transform.parent = passTo.transform;

        passTo.canvas.UpdateEquipmentDisplay();
        passer.canvas.UpdateEquipmentDisplay();
    }

    public IEnumerator StrikeHammer(GridElement user, GridElement target) {
        
        float timer = 0;

        Vector2 attackLerp = (target.coord - user.coord)/2;
        user.elementCanvas.UpdateStatsDisplay();
        while (timer < animDur/2) {
            yield return null;
            hammer.transform.position = Vector3.Lerp(hammer.transform.position, target.transform.position, timer/animDur);
            timer += Time.deltaTime;
        }
        timer = 0;
        while (timer < animDur/2) {
            yield return null;
            hammer.transform.position = Vector3.Lerp(hammer.transform.position, FloorManager.instance.currentFloor.PosFromCoord(user.coord), timer/animDur);
            timer += Time.deltaTime;
        }

        if (target is Drill) {
            PlayerUnit pu = (PlayerUnit)user;
            PlayerManager manager = (PlayerManager)pu.manager;
            manager.TriggerDescent();
        }
    }

}
