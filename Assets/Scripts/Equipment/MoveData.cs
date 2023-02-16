using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Move")]
[System.Serializable]
public class MoveData : EquipmentData
{


    public override IEnumerator UseEquipment(GridElement user, GridElement target = null)
    {
        yield return base.UseEquipment(user);
        yield return user.StartCoroutine(MoveToCoord(user, target.coord));
        
    }

    public IEnumerator MoveToCoord(GridElement ge, Vector2 moveTo) 
    {
        float timer = 0;
        ge.elementCanvas.UpdateStatsDisplay();
        
// exposed UpdateElement code to selectively update sort order
        if (ge.grid.SortOrderFromCoord(moveTo) > ge.grid.SortOrderFromCoord(ge.coord))
            ge.UpdateSortOrder(moveTo);
        ge.coord = moveTo;
        AudioManager.PlaySound(AudioAtlas.Sound.moveSlide,moveTo);
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
