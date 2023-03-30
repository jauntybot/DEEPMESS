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
    
    public override List<Vector2> TargetEquipment(GridElement user, int mod = 0) {

        List<Vector2> validCoords = EquipmentAdjacency.GetAdjacent(user, range + mod, this, targetTypes);
        user.grid.DisplayValidCoords(validCoords, gridColor);
        if (user is PlayerUnit pu) pu.ui.ToggleEquipmentPanel(false);
        for (int i = validCoords.Count - 1; i >= 0; i--) {
            bool occupied = false;
            foreach (GridElement ge in FloorManager.instance.currentFloor.CoordContents(validCoords[i])) {
                occupied = true;
                bool remove = true;
                foreach(GridElement target in targetTypes) {
                    if (ge.GetType() == target.GetType()) {
                        remove = false;
                        if (ge is EnemyUnit)
                            ge.elementCanvas.ToggleStatsDisplay(true);
                    }
                }
                if (remove) 
                    validCoords.Remove(validCoords[i]);
            } 
            if (!occupied)
                validCoords.Remove(validCoords[i]);
        }
        return validCoords;
        
    }

    public override IEnumerator UseEquipment(GridElement user, GridElement target = null)
    {
        yield return base.UseEquipment(user, target);
        switch (action) {
            case Action.Strike:
                yield return user.StartCoroutine(ThrowHammer((PlayerUnit)user, target));
            break;
        }
    }

    public override void EquipEquipment(GridElement user)
    {
        base.EquipEquipment(user);
    }

    public IEnumerator ThrowHammer(PlayerUnit user, GridElement target) {
        
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
        if (target is PlayerUnit pu) {
            PassHammer((PlayerUnit)user, pu);
        } else {
            AudioManager.PlaySound(AudioAtlas.Sound.attackStrike, user.transform.position);
// Attack target if unit
            if (target is EnemyUnit) {
                target.StartCoroutine(target.TakeDamage(3));
                manager.hammerCharge = 0;
            }
            manager.ChargeHammer(1);
// Assign a random unit to pass the hammer to
            Unit passTo = manager.units[Random.Range(0, manager.units.Count - 1)];
            while (passTo is not PlayerUnit) passTo = manager.units[Random.Range(0, manager.units.Count - 1)];
            if (passTo.gfx[0].sortingOrder > user.gfx[0].sortingOrder)
                hammer.GetComponentInChildren<SpriteRenderer>().sortingOrder = passTo.gfx[0].sortingOrder;    
// Lerp hammer to random unit
            timer = 0;
            while (timer < animDur) {
                hammer.transform.position = Vector3.Lerp(hammer.transform.position, FloorManager.instance.currentFloor.PosFromCoord(passTo.coord), timer/animDur);
                yield return null;
                timer += Time.deltaTime;
            }
        
            PassHammer((PlayerUnit)user, (PlayerUnit)passTo);

            if (target is Nail)
                manager.TriggerDescent();
        }
    }

    public void PassHammer(PlayerUnit sender, PlayerUnit reciever) {
        for (int i = sender.equipment.Count - 1; i >= 0; i--) {
                if (sender.equipment[i] is HammerData) {
                    reciever.equipment.Insert(3, sender.equipment[i]);

                    sender.equipment.Remove(sender.equipment[i]);
                }
            }
            sender.gfx.Remove(hammer.GetComponentInChildren<SpriteRenderer>());
            hammer.transform.parent = reciever.transform;
            reciever.gfx.Add(hammer.GetComponentInChildren<SpriteRenderer>());

            reciever.ui.UpdateEquipmentButtons();
            sender.ui.UpdateEquipmentButtons();
    }
}
