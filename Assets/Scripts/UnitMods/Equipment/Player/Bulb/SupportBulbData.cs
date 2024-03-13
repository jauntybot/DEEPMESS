using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Bulb/Support")]
public class SupportBulbData : BulbEquipmentData
{

    public enum SupportType { Heal, Surge };
    public SupportType supportType;

    public override List<Vector2> TargetEquipment(GridElement user, int mod = 0) {
        PlayerUnit pu = (PlayerUnit)user;
        List<Vector2> validCoords = EquipmentAdjacency.GetAdjacent(user.coord, range + pu.bulbMod, this);

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
                    if (supportType == SupportType.Surge) {
                        if (ge is Nail || ge is PlayerUnit tar && (tar.conditions.Contains(Unit.Status.Disabled) || (!tar.moved && tar.energyCurrent >= tar.energyMax)))
                            validCoords.Remove(validCoords[i]);
                    }
                }
            } else 
                validCoords.Remove(validCoords[i]);
        }
        return validCoords;
    }


    public override IEnumerator UseEquipment(GridElement user, GridElement target = null) {        
        yield return base.UseEquipment(user, target);
        Unit tar = (Unit)target;
        
        switch (supportType) {
            default:
            case SupportType.Heal:
                if (!tar.conditions.Contains(Unit.Status.Disabled))
                    target.StartCoroutine(target.TakeDamage(-2, GridElement.DamageType.Heal, user));
                else {
                    if (tar is PlayerUnit pu)
                        pu.Stabilize();
                }

            break;
            case SupportType.Surge:
                tar.moved = false; tar.energyCurrent = tar.energyMax;
            break;
        }
    }
}
