using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Move")]
[System.Serializable]
public class MoveData : EquipmentData
{



    public override List<Vector2> TargetEquipment(GridElement user) {
        List<Vector2> validCoords = EquipmentAdjacency.GetAdjacent(user.coord, this);

        return validCoords;
    }

    public override IEnumerator UseEquipment(GridElement user, GridElement target = null)
    {
        yield return base.UseEquipment(user);
        yield return user.StartCoroutine(MoveToCoord(user, target.coord));
        
    }

    public IEnumerator MoveToCoord(GridElement ge, Vector2 moveTo) 
    {
        float timer = 0;
        ge.elementCanvas.UpdateStatsDisplay();
        ge.coord = moveTo;
        while (timer < animDur) {
            yield return null;
            ge.transform.position = Vector3.Lerp(ge.transform.position, FloorManager.instance.currentFloor.PosFromCoord(moveTo), timer/animDur);
            timer += Time.deltaTime;
        }
        
        ge.UpdateElement(moveTo);
        yield return new WaitForSecondsRealtime(0.25f);
        if (!ge.targeted) ge.TargetElement(false);
    }

}
