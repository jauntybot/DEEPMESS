using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Attack")]
[System.Serializable]
public class AttackData : EquipmentData
{
    
    public Unit.Owner targetOwner;
    public int dmg;

    public override List<Vector2> TargetEquipment(GridElement user) {
        List<Vector2> validCoords = EquipmentAdjacency.GetAdjacent(user.coord, this);
        for (int i = validCoords.Count - 1; i >= 0; i--) {
            if (FloorManager.instance.currentFloor.CoordContents(validCoords[i]) is Unit u) {
                if (u.owner != targetOwner) validCoords.Remove(validCoords[i]);
                else u.TargetElement(true);
            } else validCoords.Remove(validCoords[i]);
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
        Debug.Log("Equipment use");
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

        target.StartCoroutine(target.TakeDamage(dmg));
        Debug.Log("Equipment used");
    }

}
