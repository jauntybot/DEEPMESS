using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Consumable/Placeable")]
[System.Serializable]   
public class PlacementData : ConsumableEquipmentData
{
    public enum PlacementType { MoveAndPlace, PlaceAdjacent};
    [Header("Placement Equipment")]
    public PlacementType placementType;
    [SerializeField] GameObject prefab;


    public override List<Vector2> TargetEquipment(GridElement user, int mod)
    {
        return base.TargetEquipment(user);

    }



    public override IEnumerator UseEquipment(GridElement user, GridElement target = null)
    {
        yield return base.UseEquipment(user, target);
        PlayerUnit pu = (PlayerUnit)user;
        
        GridElement toPlace = prefab.GetComponent<GridElement>();
        GridElement placed = (toPlace is Unit u) ? 
            (GridElement)pu.manager.SpawnUnit(target.coord, u) : 
            Instantiate(prefab, user.grid.neutralGEContainer.transform).GetComponent<GridElement>();
        if (toPlace is not Unit) placed.StoreInGrid(pu.grid);
        if (placed.GetComponent<NestedFadeGroup.NestedFadeGroup>())
            placed.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = 1;

        switch(placementType) {
            default: break;
            case PlacementType.MoveAndPlace:
                placed.UpdateElement(pu.coord);            
                yield return user.StartCoroutine(MoveToCoord(pu, target.coord));
            break;
            case PlacementType.PlaceAdjacent:
                placed.UpdateElement(user.coord);
                Vector3 origin = user.grid.PosFromCoord(user.coord);
                Vector3 dest = user.grid.PosFromCoord(target.coord);
                float h = 0.25f + Vector2.Distance(user.coord, target.coord) / 2;
                float throwDur = 0.25f + animDur * Vector2.Distance(user.coord, target.coord);
                float timer = 0;
                while (timer < throwDur) {
                    placed.transform.position = Util.SampleParabola(origin, dest, h, timer/throwDur);

                    yield return new WaitForSecondsRealtime(1/Util.fps);
                    timer += Time.deltaTime;    
                }
                placed.UpdateElement(target.coord);
                if (target != null)
                    placed.OnSharedSpace(target);
                
            break;
        }
    }

public IEnumerator MoveToCoord(Unit unit, Vector2 moveTo) 
    {
        float timer = 0;

// Check for shared space  
        foreach (GridElement ge in unit.grid.CoordContents(moveTo)) {
            ge.OnSharedSpace(unit);
        }
        
// exposed UpdateElement() functionality to selectively update sort order
        if (unit.grid.SortOrderFromCoord(moveTo) > unit.grid.SortOrderFromCoord(unit.coord))
            unit.UpdateSortOrder(moveTo);
        unit.coord = moveTo;

// Lerp units position to target
        Vector3 toPos = FloorManager.instance.currentFloor.PosFromCoord(moveTo);
        while (timer < animDur) {
            yield return null;
            unit.transform.position = Vector3.Lerp(unit.transform.position, toPos, timer/animDur);
            timer += Time.deltaTime;
        }
        
        unit.UpdateElement(moveTo);
        yield return new WaitForSecondsRealtime(0.25f);
        if (!unit.targeted) unit.TargetElement(false);
    }

}
