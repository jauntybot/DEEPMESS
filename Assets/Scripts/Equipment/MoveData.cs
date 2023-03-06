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
        yield return user.StartCoroutine(MoveToCoord((Unit)user, target.coord));
        
    }

    public IEnumerator MoveToCoord(Unit unit, Vector2 moveTo) 
    {
        float timer = 0;

// Check for pickups
        if (unit is PlayerUnit pu) {
            if (pu.grid.CoordContents(moveTo) is EquipmentPickup equip) {
   
// Spawn new hammer and assign it to equipment data
                PlayerManager manager = (PlayerManager)pu.manager;
                manager.SpawnHammer(pu, equip.equipment);
                pu.StartCoroutine(equip.DestroyElement());
   
            }
        }
// exposed UpdateElement() functionality to selectively update sort order
        if (unit.grid.SortOrderFromCoord(moveTo) > unit.grid.SortOrderFromCoord(unit.coord))
            unit.UpdateSortOrder(moveTo);
        unit.coord = moveTo;

        AudioManager.PlaySound(AudioAtlas.Sound.moveSlide,moveTo);
// Lerp units position to target
        while (timer < animDur) {
            yield return null;
            unit.transform.position = Vector3.Lerp(unit.transform.position, FloorManager.instance.currentFloor.PosFromCoord(moveTo), timer/animDur);
            timer += Time.deltaTime;
        }
        
        unit.UpdateElement(moveTo);
        yield return new WaitForSecondsRealtime(0.25f);
        if (!unit.targeted) unit.TargetElement(false);
    }

}
