using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.Universal;
using UnityEngine;

public class Anvil : Unit {


    [Header("ANVIL UNIT")]
    [HideInInspector] public AnvilData data;
    [SerializeField] GameObject explosionVFX;
    [SerializeField] SFX detonateSFX;
    [SerializeField] List<GridElement> targetTypes;

    protected override void Start() {
// Manual override of Base.Start to exclude hp initialization
        audioSource = GetComponent<AudioSource>();
        hitbox = GetComponent<PolygonCollider2D>();
        hitbox.enabled = false;

        hpCurrent = hpMax;
        energyCurrent = energyMax;

// If first serialized GFX has an animator set Unit anim to it 
        if (gfx[0].GetComponent<Animator>()) {
            gfxAnim = gfx[0].GetComponent<Animator>();
            gfxAnim.keepAnimatorStateOnDisable = true;
        }
        if (conditionDisplay) conditionDisplay.Init(this);
    }

    public virtual void Init(AnvilData _data) {
        data = _data;
// SPECIAL TIERS -- Increase anvil max HP
        if (data.upgrades[SlagEquipmentData.UpgradePath.Scab] == 1) hpMax = 2;
        if (data.upgrades[SlagEquipmentData.UpgradePath.Scab] >= 2) hpMax = 3;
        hpCurrent = hpMax;
        if (elementCanvas == null)  {
            elementCanvas = GetComponentInChildren<ElementCanvas>();
            elementCanvas.Initialize(this);
        } else
            elementCanvas.InstantiateMaxPips();

    }

    public override IEnumerator CollideFromAbove(GridElement subGE, int hardLand = 0) {
        yield return StartCoroutine(TakeDamage(hpCurrent));

    }

    public override IEnumerator DestroySequence(DamageType dmgType = DamageType.Unspecified, GridElement source = null, EquipmentData sourceEquip = null) {
        if (!destroyed) {
// POWER TIER I - Detonate anvil
            if (!(data.upgrades[SlagEquipmentData.UpgradePath.Shunt] == 0 || dmgType == DamageType.Unspecified))
                yield return Detonate(data.upgrades[SlagEquipmentData.UpgradePath.Shunt] == 2 ? 2 : 1);
            yield return base.DestroySequence(dmgType, source, sourceEquip);
       }
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
                            if (!((ge is PlayerUnit || ge is Nail) && data.upgrades[SlagEquipmentData.UpgradePath.Shunt] == 2))
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
