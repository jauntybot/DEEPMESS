using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Attack/Strike")]
[System.Serializable]
public class AttackData : EquipmentData {
    public int dmg;

    public override List<Vector2> TargetEquipment(GridElement user, int mod = 0) {
        List<Vector2> validCoords = EquipmentAdjacency.GetAdjacent(user.coord, range + mod, this, targetTypes);
        user.grid.DisplayValidCoords(validCoords, gridColor);

        for (int i = validCoords.Count - 1; i >= 0; i--) {
            bool remove = false;
            foreach (GridElement ge in FloorManager.instance.currentFloor.CoordContents(validCoords[i])) {
                remove = true;
                foreach(GridElement target in targetTypes) {
                    if (ge.GetType() == target.GetType()) {
                        remove = false;
                        if (ge is Unit u) {
                            if (u.conditions.Contains(Unit.Status.Disabled)) remove = true;
                        }
                        ge.elementCanvas.ToggleStatsDisplay(true);

                    }
                }
            } 
            if (remove)
                validCoords.Remove(validCoords[i]);
        }
        return validCoords;
    }

    
    public override IEnumerator UseEquipment(GridElement user, GridElement target = null)
    {
        yield return base.UseEquipment(user);
        yield return user.StartCoroutine(AttackElement(user, target));
        
    }

    public IEnumerator AttackElement(GridElement user, GridElement target) 
    {
        float timer = 0;
        
        user.elementCanvas.UpdateStatsDisplay();
        while (timer < animDur/2) {
            yield return null;
            user.transform.position = Vector3.Lerp(user.transform.position, target.transform.position, timer/animDur);
            timer += Time.deltaTime;
        }
        timer = 0;
        while (timer < animDur/2) {
            yield return null;
            user.transform.position = Vector3.Lerp(user.transform.position, FloorManager.instance.currentFloor.PosFromCoord(user.coord), timer/animDur);
            timer += Time.deltaTime;
        }

        List<Coroutine> cos = new();
        if (target is Nail n) {
            if (n.manager.scenario.tutorial != null && n.manager.scenario.tutorial.isActiveAndEnabled && !n.manager.scenario.tutorial.nailDamageEncountered && n.manager.scenario.floorManager.floorSequence.activePacket.packetType != FloorPacket.PacketType.Tutorial) {
                n.manager.scenario.tutorial.StartCoroutine(n.manager.scenario.tutorial.NailDamage());
            }
            CameraController.instance.StartCoroutine(CameraController.instance.ScreenShake(0.125f, 0.5f));
            cos.Add(user.StartCoroutine(user.TakeDamage(1, GridElement.DamageType.Melee, n)));
        }
        if (target.shield && target.shield.thorns) cos.Add(user.StartCoroutine(user.TakeDamage(1, GridElement.DamageType.Melee, target)));
        cos.Add(target.StartCoroutine(target.TakeDamage(dmg, GridElement.DamageType.Melee, user)));
        
        for (int i = cos.Count - 1; i >= 0; i--)
            yield return cos[i];

        Debug.Log("Attack finished");
    }

}
