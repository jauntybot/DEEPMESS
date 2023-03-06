using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Placement")]
[System.Serializable]   
public class PlacementData : EquipmentData
{
    public enum PlacementType { MoveAndPlace, PlaceAdjacent};
    [Header("Placement Equipment")]
    public PlacementType placementType;
    [SerializeField] GameObject prefab;
    public int count;


    public override List<Vector2> TargetEquipment(GridElement user)
    {
        return base.TargetEquipment(user);

    }



    public override IEnumerator UseEquipment(GridElement user, GridElement target = null)
    {
        yield return base.UseEquipment(user, target);
        PlayerUnit pu = (PlayerUnit)user;
        Unit placed = Instantiate(prefab, pu.manager.transform).GetComponent<Unit>();
        placed.StoreInGrid(pu.grid);
        placed.UpdateElement(pu.coord);
        
        pu.manager.units.Add(placed);
        placed.manager = pu.manager;
        placed.manager.SubscribeElement(placed);
        placed.selectable = true;

        count--;
        foreach (Unit u in pu.manager.units) {
            if (u is PlayerUnit _pu)
                _pu.ui.UpdateEquipmentButtons();
        }
        yield return user.StartCoroutine(MoveToCoord(pu, target.coord));
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
