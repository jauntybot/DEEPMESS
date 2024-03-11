using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Equipment/Attack/Detonate")]
public class SelfDetonate : EnemyAttackData {
    [SerializeField] private SFX selfDetonateSFX, chargeSFX;


    public override List<Vector2> TargetEquipment(GridElement user, int mod = 0) {
        List<Vector2> validCoords = EquipmentAdjacency.GetAdjacent(user.coord, range + mod, this, targetTypes);
        user.grid.DisplayValidCoords(validCoords, gridColor);
        
        bool valid = false;
        for (int i = validCoords.Count - 1; i >= 0; i--) {
            if (user.grid.CoordContents(validCoords[i]).Count > 0) {
                foreach (GridElement ge in user.grid.CoordContents(validCoords[i])) {
                    if (filters.Find(g => g.GetType() == ge.GetType()) != null) {
                        valid = true;
                        if (ge is Unit u) {
                            if (u.conditions.Contains(Unit.Status.Disabled)) valid = false;
                        }
                    }
                }
            }
            if (!valid) validCoords.Remove(validCoords[i]);
        }

        return validCoords;
    }


    public override IEnumerator UseEquipment(GridElement user, GridElement target = null) {
        yield return base.UseEquipment(user, target);

        
        EnemyDetonateUnit u = (EnemyDetonateUnit)user;
        if (!u.primed) {
            u.PrimeSelf();
            user.PlaySound(chargeSFX);
        } else {
// No target event for objectives
            OnEquipmentUse evt = ObjectiveEvents.OnEquipmentUse;
            evt.data = this; evt.user = user;
            ObjectiveEventManager.Broadcast(evt);

            u.StartCoroutine(u.Explode());
            u.primed = false;
            user.PlaySound(selfDetonateSFX);

// Apply damage to units in AOE
            List<Vector2> aoe = EquipmentAdjacency.GetAdjacent(user.coord, range, this, targetTypes);
            List<Coroutine> affectedCo = new();
            foreach (Vector2 coord in aoe) {
                if (user.grid.CoordContents(coord).Count > 0) {
                    foreach (GridElement ge in user.grid.CoordContents(coord)) {
                        if ((ge is Unit || ge is Wall) && ge != user ) {
                            evt = ObjectiveEvents.OnEquipmentUse;
                            evt.data = this; evt.user = user; evt.target = ge;
                            ObjectiveEventManager.Broadcast(evt);
                            affectedCo.Add(ge.StartCoroutine(ge.TakeDamage(dmg + dmgMod, GridElement.DamageType.Explosion, user, this)));
                        }
                    }
                }
            }
            
            for (int i = affectedCo.Count - 1; i >= 0; i--) {
                if (affectedCo[i] != null) {
                    yield return affectedCo[i];
                }
                else
                    affectedCo.RemoveAt(i);
            }
        }
    }


}
