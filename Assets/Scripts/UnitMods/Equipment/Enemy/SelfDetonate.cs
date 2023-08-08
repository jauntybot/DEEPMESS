using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Equipment/Attack/Detonate")]
public class SelfDetonate : EquipmentData
{
    [SerializeField] private SFX selfDetonateSFX, chargeSFX;


    public override List<Vector2> TargetEquipment(GridElement user, int mod = 0) {
        List<Vector2> validCoords = EquipmentAdjacency.GetAdjacent(user.coord, range + mod, this, targetTypes);
        bool valid = false;
        for (int i = validCoords.Count - 1; i >= 0; i--) {
            if (user.grid.CoordContents(validCoords[i]).Count > 0) {
                foreach (GridElement ge in user.grid.CoordContents(validCoords[i])) {
                    if (filters.Find(g => g.GetType() == ge.GetType()) != null) {
                        valid = true;
                    }
                }
            }
            if (!valid) validCoords.Remove(validCoords[i]);
        }

        return validCoords;
    }


    public override IEnumerator UseEquipment(GridElement user, GridElement target = null)
    {
        yield return base.UseEquipment(user, target);
        
        EnemyDetonateUnit u = (EnemyDetonateUnit)user;
        if (!u.primed) {
            u.PrimeSelf();
            user.PlaySound(chargeSFX);
        } else {
            u.Explode();
            u.primed = false;
            user.PlaySound(selfDetonateSFX);

// Apply damage to units in AOE
            List<Vector2> aoe = EquipmentAdjacency.GetAdjacent(user.coord, range, this, targetTypes);
            List<Coroutine> affectedCo = new List<Coroutine>();
            foreach (Vector2 coord in aoe) {
                if (user.grid.CoordContents(coord).Count > 0) {
                    foreach (GridElement ge in user.grid.CoordContents(coord)) {
                        if (ge is Unit tu && ge != user) {
                            affectedCo.Add(tu.StartCoroutine(tu.TakeDamage(2)));
                        }
                    }
                }
            }
            
            for (int i = affectedCo.Count - 1; i >= 0; i--) {
                if (affectedCo[i] != null) {
                    yield return affectedCo[i];
                    Debug.Log("yielding");
                }
                else
                    affectedCo.RemoveAt(i);
            }
            yield return new WaitForSecondsRealtime(0.25f);
            Debug.Log("EquipCo Finish");
        }
    }


}
