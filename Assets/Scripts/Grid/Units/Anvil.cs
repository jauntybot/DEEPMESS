using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.Universal;
using UnityEngine;

public class Anvil : Unit {


    [Header("ANVIL UNIT")]
    AnvilData data;
    [SerializeField] GameObject explosionVFX, wallPrefab;
    [SerializeField] SFX detonateSFX;
    [SerializeField] List<GridElement> targetTypes;
    bool reinforcedBottom, explode, liveWire, crystalize;

    public virtual void Init(int _hp, AnvilData _data) {
        data = _data;
// SPECIAL TIERS -- Increase anvil max HP
        // if (data.upgrades[SlagGearData.UpgradePath.Scab] == 1) hpMax = 2;
        // if (data.upgrades[SlagGearData.UpgradePath.Scab] >= 2) hpMax = 3;
        hpMax = _hp;
        hpCurrent = hpMax;
        if (elementCanvas == null)  {
            elementCanvas = GetComponentInChildren<ElementCanvas>();
            elementCanvas.Initialize(this);
        } else
            elementCanvas.InstantiateMaxPips();

            reinforcedBottom = data.reinforcedBottom;
            explode = data.explode;
            liveWire = data.liveWire;
            crystalize = data.crystalize;
    }

    public override IEnumerator CollideFromAbove(GridElement subGE, int hardLand = 0) {
        if (!reinforcedBottom)
            yield return base.CollideFromAbove(subGE, hardLand);
    }

    public override IEnumerator TakeDamage(int dmg, DamageType dmgType = DamageType.Unspecified, GridElement source = null, GearData sourceEquip = null) {
        if (liveWire && source is EnemyUnit u && dmgType != DamageType.Fall && dmgType != DamageType.Crush) {
            u.ApplyCondition(Status.Stunned);
        }
        yield return base.TakeDamage(dmg, dmgType, source, sourceEquip);
    }

    public override IEnumerator DestroySequence(DamageType dmgType = DamageType.Unspecified, GridElement source = null, GearData sourceEquip = null) {
// POWER TIER I - Detonate anvil
        if (explode)
            yield return Detonate(1);
        if (crystalize) {
            Wall wall = Instantiate(wallPrefab).GetComponent<GridElement>().GetComponent<Wall>();
            wall.transform.SetParent(FloorManager.instance.currentFloor.neutralGEContainer.transform);

            wall.StoreInGrid(FloorManager.instance.currentFloor);
            wall.UpdateElement(coord);
        }
        yield return base.DestroySequence(dmgType, source, sourceEquip);
       
    }

    public virtual IEnumerator Detonate(int dmg) {
            StartCoroutine(ExplosionVFX());
            PlaySound(detonateSFX);
            gfx[0].enabled = false;
// Apply damage to units in AOE
            List<Vector2> aoe = EquipmentAdjacency.BoxAdjacency(coord, 1);
            grid.DisplayValidCoords(aoe, 2);
            yield return new WaitForSecondsRealtime(0.5f);
            List<Coroutine> affectedCo = new();
            grid.DisableGridHighlight();
            foreach (Vector2 coord in aoe) {
                if (grid.CoordContents(coord).Count > 0) {
                    foreach (GridElement ge in grid.CoordContents(coord)) {
                        if ((ge is Unit || ge is Wall) && ge != this) {
// POWER TIER II - Remove friendly fire
                            // if (!((ge is PlayerUnit || ge is Nail) && data.upgrades[SlagGearData.UpgradePath.Shunt] == 2))
                            affectedCo.Add(ge.StartCoroutine(ge.TakeDamage(dmg, DamageType.Explosion, this, data)));                        
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
            yield return new WaitForSecondsRealtime(0.15f);
    }

    IEnumerator ExplosionVFX() {
        // Explosion VFX
        GameObject go = Instantiate(explosionVFX, grid.PosFromCoord(coord), Quaternion.identity);
        go.GetComponentInChildren<SpriteRenderer>().sortingOrder = grid.SortOrderFromCoord(coord);
        float t = 0;
        while (t <= 0.125f) { t += Time.deltaTime; yield return null; }
        List<Vector2> secondWave = new();
        for (int x = -1; x <= 1; x++) {
            for (int y = -1; y <= 1; y++) {
                Vector2 c = coord + new Vector2(x,y);
                if (c == coord || c.x < 0 || c.x > 7 || c.y < 0 || c.y > 7) continue;
                if (x != 0 && y != 0) {
                    secondWave.Add(c);
                    continue;
                }
                GameObject g = Instantiate(explosionVFX, grid.PosFromCoord(c), Quaternion.identity);
                g.GetComponentInChildren<SpriteRenderer>().sortingOrder = grid.SortOrderFromCoord(c);
            }
        }
        t = 0;
        while (t <= 0.125f) { t += Time.deltaTime; yield return null; }
        foreach (Vector2 c in secondWave) {
            GameObject g = Instantiate(explosionVFX, grid.PosFromCoord(c), Quaternion.identity);
            g.GetComponentInChildren<SpriteRenderer>().sortingOrder = grid.SortOrderFromCoord(c);
        }
    }

    public override void EnableSelection(bool state) {}
}
