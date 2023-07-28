using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Attack/Strike")]
[System.Serializable]
public class AttackData : EquipmentData
{
    

    public int dmg;

    public override List<Vector2> TargetEquipment(GridElement user, int mod = 0) {
        List<Vector2> validCoords = EquipmentAdjacency.GetAdjacent(user.coord, range + mod, this, targetTypes);
        user.grid.DisplayValidCoords(validCoords, gridColor);
        if (user is PlayerUnit pu) pu.ui.ToggleEquipmentPanel(false);
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
                        Debug.Log(ge.name);
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
        Vector2 attackLerp = (target.coord - user.coord)/2;
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

        Coroutine co = null; Coroutine co2 = null;
        if (target is Nail) co = user.StartCoroutine(user.TakeDamage(1));
        co2 = target.StartCoroutine(target.TakeDamage(dmg));
        yield return co;
        if (co2 != null)
            yield return co2;
        
    }

}
