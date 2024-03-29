using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/CascadeMove")]
public class CascadeMoveData : MoveData {

    public override List<Vector2> TargetEquipment(GridElement user, int mod = 0)
    {
        List<Vector2> validCoords = EquipmentAdjacency.GetAdjacent(user.coord, range + mod, this);
        List<Vector2> invalidCoords = new();
        Unit unit = (Unit)user;
        foreach (Unit u in unit.manager.units) {
            if (u is not Nail) {
                invalidCoords.Add(u.coord);
                validCoords.Remove(u.coord);
            }
        }
        user.grid.DisplayValidCoords(invalidCoords, 3);
        user.grid.DisplayValidCoords(validCoords, gridColor, true);
        unit.inRangeCoords = validCoords;
        return validCoords;
    }

    public override IEnumerator UseEquipment(GridElement user, GridElement target = null) {
        yield return user.StartCoroutine(MoveToCoord((Unit)user, target.coord));
    }

    public override IEnumerator MoveToCoord(Unit unit, Vector2 moveTo) {       

// Build frontier dictionary for stepped lerp
        Dictionary<Vector2, Vector2> fromTo = new();
        if (animType == AnimType.Stepped) 
            fromTo = EquipmentAdjacency.SteppedCoordAdjacency(unit.coord, moveTo, this);
        else if (animType == AnimType.Lerp)
            fromTo.Add(unit.coord, moveTo);

        unit.coord = moveTo;
        yield return null;
        unit.UpdateElement(moveTo, unit, this);

        yield return new WaitForSecondsRealtime(0.25f);
        if (!unit.targeted) unit.TargetElement(false);
    }

}
