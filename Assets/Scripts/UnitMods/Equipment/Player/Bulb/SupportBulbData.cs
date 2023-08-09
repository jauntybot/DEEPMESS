using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Bulb/Support")]
public class SupportBulbData : BulbEquipmentData
{

    public enum SupportType { Heal, Surge };
    public SupportType supportType;

    public override List<Vector2> TargetEquipment(GridElement user, int mod = 0)
    {

        List<Vector2> validCoords = EquipmentAdjacency.GetAdjacent(user.coord, range + mod, this);

        if (user is PlayerUnit u) {
            u.ui.ToggleEquipmentButtons();
            u.inRangeCoords = validCoords;
        }

        user.grid.DisplayValidCoords(validCoords, gridColor);
        for (int i = validCoords.Count - 1; i >= 0; i--) {
            if (user.grid.CoordContents(validCoords[i]).Count != 0) {
                foreach(GridElement ge in user.grid.CoordContents(validCoords[i])) {
                    if (ge is not PlayerUnit && ge is not Nail)
                        validCoords.Remove(validCoords[i]);
                    else if (ge is PlayerUnit pu && (pu.conditions.Contains(Unit.Status.Disabled) || (!pu.moved && pu.energyCurrent >= pu.energyMax)))
                        validCoords.Remove(validCoords[i]);
                }
            } else 
                validCoords.Remove(validCoords[i]);
        }
        return validCoords;
    }


    public override IEnumerator UseEquipment(GridElement user, GridElement target = null)
    {        
        yield return base.UseEquipment(user, target);
        
        switch (supportType) {
            default:
            case SupportType.Heal:
                target.StartCoroutine(target.TakeDamage(-2));

            break;
            case SupportType.Surge:
                PlayerUnit pu = (PlayerUnit)target;
                pu.moved = false; pu.energyCurrent = pu.energyMax;
            break;
        }
    }
}
