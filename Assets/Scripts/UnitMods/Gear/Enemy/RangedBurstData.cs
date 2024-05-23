using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Gear/Attack/RangedBurst")]
[System.Serializable]
public class RangedBurstData : EnemyAttackData {
    
    [SerializeField] GameObject projectilePrefab, thornsVFX, thornsInflictedVFX;

    public override List<Vector2> TargetEquipment(GridElement user, int mod = 0) {
        List<Vector2> validCoords = EquipmentAdjacency.GetAdjacent(user.coord, range + mod, this, targetTypes);
        user.grid.DisplayValidCoords(validCoords, gridColor);

        for (int i = validCoords.Count - 1; i >= 0; i--) {
            bool remove = false;
            foreach (GridElement ge in FloorManager.instance.currentFloor.CoordContents(validCoords[i])) {
                remove = true;
                foreach(GridElement target in targetTypes) {
                    if (ge.GetType() == target.GetType()) {
                        remove = false;
                        if (ge is Unit u) {
                            if (u.conditions.Contains(Unit.Status.Disabled)) remove = true;
                        }
                        if (ge.elementCanvas)
                            ge.elementCanvas.ToggleStatsDisplay(true);

                    }
                }
            } 
            if (remove)
                validCoords.Remove(validCoords[i]);
        }
        return validCoords;
    }

    
    public override IEnumerator UseGear(GridElement user, GridElement target = null) {
        yield return base.UseGear(user);
        yield return user.StartCoroutine(BurstAttack(user));
    }

    public IEnumerator BurstAttack(GridElement user)  {
        user.elementCanvas.UpdateStatsDisplay();
        List<Coroutine> cos = new();

// X-Axis 
        for (int x = -1; x <= 1; x += 2) {
            GameObject projectile = Instantiate(projectilePrefab, user.transform);
            projectile.GetComponentInChildren<Animator>().SetInteger("X", x);
            projectile.GetComponentInChildren<Animator>().SetInteger("Y", 0);
            SpriteRenderer sr = projectile.GetComponentInChildren<SpriteRenderer>();
            
            GridElement tar = null;
            Vector2 coord = user.coord;
            for (int i = 1; i <= range; i++) {
                Vector2 _coord = user.coord + new Vector2(i*x, 0);
                //if (_coord.x < 0 || _coord.x > 7) break;
                coord = _coord;
                if (user.grid.CoordContents(coord).Count > 0) {
                    tar = user.grid.CoordContents(coord)[0];
                    break;
                }
            }
            if (user.grid.SortOrderFromCoord(coord) > user.grid.SortOrderFromCoord(user.coord))
                sr.sortingOrder = user.grid.SortOrderFromCoord(coord);
            else
                sr.sortingOrder = user.grid.SortOrderFromCoord(user.coord);

            cos.Add(user.StartCoroutine(LaunchProjectile(projectile, user, coord, tar)));
        }     
// Y-Axis 
        for (int y = -1; y <= 1; y += 2) {
            GameObject projectile = Instantiate(projectilePrefab, user.transform);
            projectile.GetComponentInChildren<Animator>().SetInteger("X", 0);
            projectile.GetComponentInChildren<Animator>().SetInteger("Y", y);
            SpriteRenderer sr = projectile.GetComponentInChildren<SpriteRenderer>();
            
            GridElement tar = null;
            Vector2 coord = user.coord;
            for (int i = 1; i <= range; i++) {
                Vector2 _coord = user.coord + new Vector2(0, i*y);
                //if (_coord.y < 0 || _coord.y > 7) break;
                coord = _coord;
                if (user.grid.CoordContents(coord).Count > 0) {
                    tar = user.grid.CoordContents(coord)[0];
                    break;
                }
            }
            if (user.grid.SortOrderFromCoord(coord) > user.grid.SortOrderFromCoord(user.coord))
                sr.sortingOrder = user.grid.SortOrderFromCoord(coord);
            else
                sr.sortingOrder = user.grid.SortOrderFromCoord(user.coord);

            cos.Add(user.StartCoroutine(LaunchProjectile(projectile, user, coord, tar)));
        } 
        
        for (int i = cos.Count - 1; i >= 0; i--)
            yield return cos[i];
    }

    IEnumerator LaunchProjectile(GameObject projectile, GridElement user, Vector2 dest, GridElement target = null) { 
        OnEquipmentUse evt = ObjectiveEvents.OnEquipmentUse;
        evt.data = this; evt.user = user; evt.target = target;
        ObjectiveEventManager.Broadcast(evt);

        List<Coroutine> cos = new();
        float dur = animDur * Mathf.Abs((user.coord - dest).magnitude);
        float timer = 0;

        Vector3 startPos = user.grid.PosFromCoord(user.coord);        
        Vector3 endPos = user.grid.PosFromCoord(dest);
        
        int thornDmg = 0;
        if (target) {
            if (target is Nail n) {
                if (n.manager.scenario.floorManager.tutorial != null && n.manager.scenario.floorManager.tutorial.isActiveAndEnabled && !n.manager.scenario.floorManager.tutorial.nailDamageEncountered) {
                    n.manager.scenario.floorManager.tutorial.StartCoroutine(n.manager.scenario.floorManager.tutorial.NailDamage());
                }
                CameraController.instance.StartCoroutine(CameraController.instance.ScreenShake(0.125f, 0.5f));
                thornDmg++;
            }
            if (target.shield) {
                if (target.shield.liveWired && user is Unit u) {
                    u.ApplyCondition(Unit.Status.Stunned);
                }
                if (target.shield.thorns) thornDmg++;
            }

        }

        while (timer < dur) {
            yield return null;
            timer += Time.deltaTime;

            projectile.transform.position = Vector3.Lerp(startPos, endPos, timer/dur);
        }

        if (target) {
            if (thornDmg > 0) {
                GameObject go = Instantiate(thornsVFX, target.transform.position, Quaternion.identity);
                go.GetComponent<SpriteRenderer>().sortingOrder = target.gfx[0].sortingOrder++;
                go = Instantiate(thornsInflictedVFX, user.transform.position, Quaternion.identity);
                go.GetComponent<SpriteRenderer>().sortingOrder = user.gfx[0].sortingOrder++;
                cos.Add(user.StartCoroutine(user.TakeDamage(thornDmg, GridElement.DamageType.Melee, target, target.shield ? target.shield.data : null)));
            }
            evt = ObjectiveEvents.OnEquipmentUse;
            evt.data = this; evt.user = user; evt.target = target;
            ObjectiveEventManager.Broadcast(evt);
            cos.Add(target.StartCoroutine(target.TakeDamage(dmg + dmgMod, GridElement.DamageType.Melee, user, this)));
            projectile.transform.position = endPos;
        }


        Destroy(projectile);

        for (int i = cos.Count - 1; i >= 0; i--)
            yield return cos[i];
    }

}
