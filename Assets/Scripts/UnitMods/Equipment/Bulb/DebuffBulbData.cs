using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Equipment/Bulb/Debuff")]
public class DebuffBulbData : BulbEquipmentData
{

    public enum DebuffType { Weaken };
    public DebuffType debuffType;


    public override List<Vector2> TargetEquipment(GridElement user, int mod = 0)
    {


        return base.TargetEquipment(user, mod);


    }

    public override IEnumerator UseEquipment(GridElement user, GridElement target = null)
    {
        yield return base.UseEquipment(user, target);

        switch (debuffType) {
            default:
            case DebuffType.Weaken:
                List<Vector2> aoe = EquipmentAdjacency.OrthagonalAdjacency(target, 1, null, null);
                aoe.Add(target.coord);
                foreach (Vector2 v in aoe) Debug.Log("AOE: " + v);
                foreach(Vector2 coord in aoe) {
                    if (user.grid.CoordContents(coord).Count > 0) {
                        foreach (GridElement ge in user.grid.CoordContents(coord)) {
                            if (ge is Unit u) {
                                u.ApplyCondition(Unit.Status.Weakened);
                                if (ge != target) {
                                    GameObject bulb = Instantiate(bulbPrefab, ge.transform.position, Quaternion.identity, ge.transform);
                                    bulb.GetComponent<SpriteRenderer>().sortingOrder = user.grid.SortOrderFromCoord(ge.coord);
                                    bulb.GetComponent<Animator>().SetTrigger("Apply");
                                }
                            }
                        }
                    }
                }

            break;
        }

    }


}
