using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Equipment/Attack/Detonate")]
public class SelfDetonate : EquipmentData
{


    public override List<Vector2> TargetEquipment(GridElement user, int mod = 0) {
        List<Vector2> validCoords = EquipmentAdjacency.GetAdjacent(user, range + mod, this, targetTypes);


        
        return validCoords;
    }


    public override IEnumerator UseEquipment(GridElement user, GridElement target = null)
    {
        yield return base.UseEquipment(user, target);
        
        EnemyDetonateUnit u = (EnemyDetonateUnit)user;
        if (!u.primed) {
            u.PrimeSelf();
        } else {

            //animation stuff

// Apply damage to units in AOE
            List<Vector2> aoe = EquipmentAdjacency.GetAdjacent(user, range, this, targetTypes);
            List<GridElement> affected = new List<GridElement>();
            foreach (Vector2 coord in aoe) {
                if (user.grid.CoordContents(coord).Count > 0) {
                    foreach (GridElement ge in user.grid.CoordContents(coord)) {
                        if (ge is Unit tu) {
                            tu.StartCoroutine(tu.TakeDamage(2));
                        }
                    }
                }
            }
            user.StartCoroutine(user.TakeDamage(user.hpCurrent));
            yield return new WaitForSecondsRealtime(0.25f);
        }
    }


}
