using System.Collections;
using System.Collections.Generic;
using Mono.Cecil;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Slag/Anvil")]
[System.Serializable]   
public class AnvilData : SlagEquipmentData {
    [SerializeField] GameObject prefab;

    public override IEnumerator UseEquipment(GridElement user, GridElement target = null) {
        yield return base.UseEquipment(user, target);
        PlayerUnit pu = (PlayerUnit)user;

        for (int i = pu.manager.units.Count - 1; i >= 0; i--) {
            if (pu.manager.units[i] is Anvil) {
                pu.manager.units[i].StartCoroutine(pu.manager.units[i].DestroySequence());
            }
        }
            
        
        GridElement toPlace = prefab.GetComponent<GridElement>();
// Ternary to either spawn a Unit or instantiate a GridElement
        GridElement placed = (toPlace is Unit u) ? 
            (GridElement)pu.manager.SpawnUnit(target.coord, u) : 
            Instantiate(prefab, user.grid.neutralGEContainer.transform).GetComponent<GridElement>();
        if (toPlace is not Unit) placed.StoreInGrid(pu.grid);
        if (placed.GetComponent<NestedFadeGroup.NestedFadeGroup>())
            placed.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = 1;

        placed.UpdateElement(pu.coord);            
        yield return user.StartCoroutine(MoveToCoord(pu, target.coord));
    }

public IEnumerator MoveToCoord(Unit unit, Vector2 moveTo) {
        float timer = 0;
        
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
