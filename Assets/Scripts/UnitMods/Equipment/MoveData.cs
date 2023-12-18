using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Move")]
[System.Serializable]
public class MoveData : EquipmentData
{
    protected enum AnimType { Lerp, Stepped }
    [SerializeField] protected AnimType animType;


    public override List<Vector2> TargetEquipment(GridElement user, int mod = 0) {
        List<Vector2> validCoords = EquipmentAdjacency.GetAdjacent(user.coord, range + mod, this);
        Unit u = (Unit)user;
        u.inRangeCoords = validCoords;
        for (int i = validCoords.Count - 1; i >= 0; i--) {
            if (user.grid.tiles.Find(sqr => sqr.coord == validCoords[i]).tileType == Tile.TileType.Bile)
                validCoords.RemoveAt(i);
        }
        user.grid.DisplayValidCoords(validCoords, gridColor);
        return validCoords;
    }
    public override IEnumerator UseEquipment(GridElement user, GridElement target = null) {
        if (user is Unit unit)
            unit.moved = true;
        user.elementCanvas.UpdateStatsDisplay();

        user.PlaySound(useSFX);

        user.grid.UpdateSelectedCursor(false, Vector2.one * -32);

        if (user is PlayerUnit pu) AddToUndoDict((Unit)user, target.coord);
        yield return user.StartCoroutine(MoveToCoord((Unit)user, target.coord));
    }

    public virtual IEnumerator MoveToCoord(Unit unit, Vector2 moveTo, bool undo = false) {       
// Build frontier dictionary for stepped lerp
        Dictionary<Vector2, Vector2> fromTo = new();
        if (animType == AnimType.Stepped) 
            fromTo = EquipmentAdjacency.SteppedCoordAdjacency(unit.coord, moveTo, this);
        else if (animType == AnimType.Lerp)
            fromTo.Add(unit.coord, moveTo);

        Vector2 current = unit.coord;
        unit.coord = moveTo;
        
// Lerp units position to target
        while (!Vector2.Equals(current, moveTo)) {
// exposed UpdateElement() functionality to selectively update sort order
            if (unit.grid.SortOrderFromCoord(fromTo[current]) > unit.grid.SortOrderFromCoord(current))
                unit.UpdateSortOrder(fromTo[current]);
            Vector3 toPos = FloorManager.instance.currentFloor.PosFromCoord(fromTo[current]);
            float timer = 0;
            while (timer < animDur) {
                yield return null;
                unit.transform.position = Vector3.Lerp(unit.transform.position, toPos, timer/animDur);
                timer += Time.deltaTime;
            }
            current = fromTo[current];
            yield return null;
        }        
        unit.UpdateElement(moveTo);
    }

// Add move to undo dictionary if player unit
    void AddToUndoDict(Unit unit, Vector2 moveTo) {
        PlayerManager manager = (PlayerManager)unit.manager;
        if (manager.undoableMoves.ContainsKey(unit)) {
            manager.undoableMoves.Remove(unit);
            manager.undoOrder.Remove(unit);
        }
        manager.undoableMoves.Add(unit, unit.coord);
        manager.undoOrder.Add(unit);

        Tile tile = unit.grid.tiles.Find(s => s.coord == moveTo);
        if (tile is TileBulb tb && !unit.equipment.Find(e => e is BulbEquipmentData)) 
            manager.harvestedByMove.Add(unit, tb);
        
        UIManager.instance.ToggleUndoButton(true);
    }
}
