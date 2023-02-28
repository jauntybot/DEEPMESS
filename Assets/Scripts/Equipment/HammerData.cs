using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Hammer")]
[System.Serializable]
public class HammerData : EquipmentData
{

    public GameObject hammer;
    public Nail nail;
    public enum Action { Lob, Strike };
    public Action action;

    
    public void AssignHammer(GameObject h, Nail d) {
        hammer = h;
        nail = d;
    }
    
    public override List<Vector2> TargetEquipment(GridElement user) {
        switch (action) {
            default:
                return base.TargetEquipment(user);
            case Action.Lob:
                return base.TargetEquipment(user);
            case Action.Strike:
                List<Vector2> validCoords = EquipmentAdjacency.GetAdjacent(user, this, targetTypes);
                user.grid.DisplayValidCoords(validCoords, gridColor);
                if (user is PlayerUnit pu) pu.canvas.ToggleEquipmentDisplay(false);
                for (int i = validCoords.Count - 1; i >= 0; i--) {
                    if (FloorManager.instance.currentFloor.CoordContents(validCoords[i]) is GridElement ge) {
                        bool remove = true;
                        foreach(GridElement target in targetTypes) {
                            if (ge.GetType() == target.GetType())
                                remove = false;
                        }
                        if (remove) 
                            validCoords.Remove(validCoords[i]);
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
                yield return user.StartCoroutine(StrikeNail((PlayerUnit)user, target));
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
        if (passTo.gfx[0].sortingOrder > passer.gfx[0].sortingOrder)
            hammer.GetComponentInChildren<SpriteRenderer>().sortingOrder = passTo.gfx[0].sortingOrder;
        float timer = 0;
        AudioManager.PlaySound(AudioAtlas.Sound.hammerPass, passer.transform.position);
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
        passer.gfx.Remove(hammer.GetComponentInChildren<SpriteRenderer>());
        hammer.transform.parent = passTo.transform;
        passTo.gfx.Add(hammer.GetComponentInChildren<SpriteRenderer>());

        passTo.canvas.UpdateEquipmentDisplay();
        passer.canvas.UpdateEquipmentDisplay();
    }

    public IEnumerator StrikeNail(PlayerUnit user, GridElement target) {
        
        PlayerManager manager = (PlayerManager)user.manager;
        if (target.gfx[0].sortingOrder > user.gfx[0].sortingOrder)
            hammer.GetComponentInChildren<SpriteRenderer>().sortingOrder = target.gfx[0].sortingOrder;
        AudioManager.PlaySound(AudioAtlas.Sound.hammerPass, user.transform.position);
    
// Lerp hammer to target
        float timer = 0;
        while (timer < animDur / 2) {
            hammer.transform.position = Vector3.Lerp(hammer.transform.position, FloorManager.instance.currentFloor.PosFromCoord(target.coord), timer/animDur);
            yield return null;
            timer += Time.deltaTime;
        }
        AudioManager.PlaySound(AudioAtlas.Sound.attackStrike, user.transform.position);
// Attack target if unit
        if (target is EnemyUnit) {
            target.StartCoroutine(target.TakeDamage(manager.hammerCharge));
            manager.hammerCharge = 0;
        }
        manager.ChargeHammer(1);
// Assign a random unit to pass the hammer to
        PlayerUnit passTo = (PlayerUnit)manager.units[Random.Range(0, manager.units.Count - 1)];
        if (passTo.gfx[0].sortingOrder > user.gfx[0].sortingOrder)
            hammer.GetComponentInChildren<SpriteRenderer>().sortingOrder = passTo.gfx[0].sortingOrder;    
// Lerp hammer to random unit
        timer = 0;
        while (timer < animDur) {
            hammer.transform.position = Vector3.Lerp(hammer.transform.position, FloorManager.instance.currentFloor.PosFromCoord(passTo.coord), timer/animDur);
            yield return null;
            timer += Time.deltaTime;
        }
    
        for (int i = user.equipment.Count - 1; i >= 0; i--) {
            if (user.equipment[i] is HammerData) {
                passTo.equipment.Add(user.equipment[i]);

                user.equipment.Remove(user.equipment[i]);
            }
        }
        user.gfx.Remove(hammer.GetComponentInChildren<SpriteRenderer>());
        hammer.transform.parent = passTo.transform;
        passTo.gfx.Add(hammer.GetComponentInChildren<SpriteRenderer>());

        passTo.canvas.UpdateEquipmentDisplay();
        user.canvas.UpdateEquipmentDisplay();

        if (target is Nail)
            manager.TriggerDescent();
    }

}
