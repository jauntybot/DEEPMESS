using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Attack/Strike")]
[System.Serializable]
public class AttackData : EquipmentData {
    public int dmg;
    [SerializeField] GameObject strikeVFX, thornsVFX;

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

    
    public override IEnumerator UseEquipment(GridElement user, GridElement target = null) {
        yield return base.UseEquipment(user);
        yield return user.StartCoroutine(AttackElement(user, target));
        
    }

    public IEnumerator AttackElement(GridElement user, GridElement target) {
        float timer = 0;
        Vector2 dir = target.coord - user.coord;
        
        user.elementCanvas.UpdateStatsDisplay();
        Debug.Log(dir);
        GameObject vfx = Instantiate(strikeVFX, FloorManager.instance.currentFloor.PosFromCoord(target.coord - dir/2), Quaternion.identity);
        vfx.GetComponent<SpriteRenderer>().sortingOrder = user.gfx[0].sortingOrder++;
        vfx.GetComponent<Animator>().SetInteger("X", (int)dir.x);
        vfx.GetComponent<Animator>().SetInteger("Y", (int)dir.y);

        int thornDmg = 0;
        if (target is Nail n) {
            if (n.manager.scenario.tutorial != null && n.manager.scenario.tutorial.isActiveAndEnabled && !n.manager.scenario.tutorial.nailDamageEncountered && n.manager.scenario.floorManager.floorSequence.activePacket.packetType != FloorPacket.PacketType.Tutorial) {
                n.manager.scenario.tutorial.StartCoroutine(n.manager.scenario.tutorial.NailDamage());
            }
            CameraController.instance.StartCoroutine(CameraController.instance.ScreenShake(0.125f, 0.5f));
            thornDmg++;
        }
// SHIELD SPECIAL TIER II -- Deal thorns damage to attacking unit
        if (target.shield && target.shield.thorns) thornDmg++;
        if (thornDmg > 0) {
            GameObject go = Instantiate(thornsVFX, target.transform.position, Quaternion.identity);
            go.GetComponent<SpriteRenderer>().sortingOrder = target.gfx[0].sortingOrder++;
        }

        while (timer < animDur/2) {
            yield return null;
            user.transform.position = Vector3.Lerp(user.transform.position, FloorManager.instance.currentFloor.PosFromCoord(target.coord - dir/2), timer/animDur);
            timer += Time.deltaTime;
        }

        timer = 0;
        while (timer < animDur/2) {
            yield return null;
            user.transform.position = Vector3.Lerp(user.transform.position, FloorManager.instance.currentFloor.PosFromCoord(user.coord), timer/animDur);
            timer += Time.deltaTime;
        }

        List<Coroutine> cos = new();
   
        if (thornDmg > 0) cos.Add(user.StartCoroutine(user.TakeDamage(thornDmg, GridElement.DamageType.Melee, target)));
        cos.Add(target.StartCoroutine(target.TakeDamage(dmg, GridElement.DamageType.Melee, user, this)));
        
        for (int i = cos.Count - 1; i >= 0; i--)
            yield return cos[i];

        Debug.Log("Attack finished");
    }

}
