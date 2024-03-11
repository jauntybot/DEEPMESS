using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Equipment/Bulb/Debuff")]
public class DebuffBulbData : BulbEquipmentData
{

    public enum DebuffType { Stun };
    public DebuffType debuffType;


    public override List<Vector2> TargetEquipment(GridElement user, int mod = 0) {
        return base.TargetEquipment(user, mod);

    }

    public override IEnumerator UseEquipment(GridElement user, GridElement target = null) {
        yield return base.UseEquipment(user, target);

        switch (debuffType) {
            default:
            case DebuffType.Stun:
                List<Vector2> aoe = EquipmentAdjacency.OrthagonalAdjacency(target.coord, 1, null, null);
                aoe.Add(target.coord);
                foreach(Vector2 coord in aoe) {
                    if (coord != target.coord) {
                        GameObject bulb = Instantiate(bulbPrefab, user.grid.PosFromCoord(coord), Quaternion.identity);
                        bulb.GetComponent<SpriteRenderer>().sortingOrder = user.grid.SortOrderFromCoord(coord);
                        bulb.GetComponent<Animator>().SetTrigger("Apply");
                    }
                    if (user.grid.CoordContents(coord).Count > 0) {
                        foreach (GridElement ge in user.grid.CoordContents(coord)) {
                            if (ge is Unit u && ge is not Anvil && ge is not Nail) {
                                u.ApplyCondition(Unit.Status.Stunned);
                            }
                        }
                    }
                }

            break;
        }

    }


}
