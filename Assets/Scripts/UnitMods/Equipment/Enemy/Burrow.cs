using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Attack/Burrow")]
public class Burrow : EquipmentData {

    public int dmg;
    [SerializeField] GameObject thornsVFX, thornsInflictedVFX;

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
        yield return base.UseEquipment(user);
        yield return user.StartCoroutine(BurrowOnSelf(user));
        
    }

    public IEnumerator BurrowOnSelf(GridElement user) {
// Apply damage to units in AOE
        SpriteRenderer sr = Instantiate(vfx, user.grid.PosFromCoord(user.coord), Quaternion.identity).GetComponent<SpriteRenderer>();
        sr.sortingOrder = user.grid.SortOrderFromCoord(user.coord);
        List<Vector2> aoe = EquipmentAdjacency.GetAdjacent(user.coord, range, this, targetTypes);
        List<Coroutine> affectedCo = new();
        List<GridElement> affected = new();
        List<GridElement> thornSources = new();
        int thornDmg = 0;
        foreach (Vector2 coord in aoe) {
            if (user.grid.CoordContents(coord).Count > 0) {
                foreach (GridElement ge in user.grid.CoordContents(coord)) {
                    if ((ge is Unit || ge is Wall) && ge != user) {
                        affected.Add(ge);
                        if (ge is Nail) {
                            thornDmg++;
                            if (!thornSources.Contains(ge))
                                thornSources.Add(ge);
                        }
                        if (ge.shield && ge.shield.thorns) {
                            thornDmg++;
                            if (!thornSources.Contains(ge))
                                thornSources.Add(ge);
                        }                                
                    }
                }
            }
        }
        if (thornDmg > 0) {
            foreach (GridElement ge in thornSources) 
                affectedCo.Add(user.StartCoroutine(user.TakeDamage(thornDmg/thornSources.Count, GridElement.DamageType.Melee, ge, ge.shield ? ge.shield.data : null)));
        }
        
        foreach (GridElement ge in affected) 
            affectedCo.Add(ge.StartCoroutine(ge.TakeDamage(dmg)));
        
        
        for (int i = affectedCo.Count - 1; i >= 0; i--) {
            if (affectedCo[i] != null) {
                yield return affectedCo[i];
            }
            else
                affectedCo.RemoveAt(i);
        }
        
        FloorManager.instance.Descend(false, true, user.coord);
    }

}
