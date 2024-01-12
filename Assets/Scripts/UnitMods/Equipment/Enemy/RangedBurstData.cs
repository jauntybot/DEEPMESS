using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Attack/RangedBurst")]
[System.Serializable]
public class RangedBurstData : EquipmentData {
    
    public int dmg;
    [SerializeField] GameObject projectilePrefab;

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

    
    public override IEnumerator UseEquipment(GridElement user, GridElement target = null) {
        yield return base.UseEquipment(user);
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
                if (_coord.x < 0 || _coord.x > 7) break;
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
                if (_coord.y < 0 || _coord.y > 7) break;
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
        
        List<Coroutine> cos = new();
        float dur = animDur * Mathf.Abs((user.coord - dest).magnitude);
        float timer = 0;

        Vector3 startPos = user.grid.PosFromCoord(user.coord);        
        Vector3 endPos = user.grid.PosFromCoord(dest);

        while (timer < dur) {
            yield return null;
            timer += Time.deltaTime;

            projectile.transform.position = Vector3.Lerp(startPos, endPos, timer/dur);
        }

        projectile.transform.position = endPos;
        if (target) {
            int thornDmg = 0;
            if (target is Nail n) {
                if (n.manager.scenario.tutorial != null && n.manager.scenario.tutorial.isActiveAndEnabled && !n.manager.scenario.tutorial.nailDamageEncountered && n.manager.scenario.floorManager.floorSequence.activePacket.packetType != FloorPacket.PacketType.Tutorial) {
                    n.manager.scenario.tutorial.StartCoroutine(n.manager.scenario.tutorial.NailDamage());
                }
                CameraController.instance.StartCoroutine(CameraController.instance.ScreenShake(0.125f, 0.5f));
                thornDmg++;
            }
            if (target.shield && target.shield.thorns) thornDmg++;

            if (thornDmg > 0) cos.Add(user.StartCoroutine(user.TakeDamage(thornDmg, GridElement.DamageType.Melee, target)));
            cos.Add(target.StartCoroutine(target.TakeDamage(dmg, GridElement.DamageType.Melee, user, this)));
        }

        Destroy(projectile);

        for (int i = cos.Count - 1; i >= 0; i--)
            yield return cos[i];
    }

}
