using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.Universal;
using UnityEngine;

public class Anvil : Unit {


    [Header("ANVIL UNIT")]
    [HideInInspector] public AnvilData data;
    [SerializeField] Animator explosion;
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
        if (data.upgrades[SlagEquipmentData.UpgradePath.Special] == 1) hpMax = 2;
        if (data.upgrades[SlagEquipmentData.UpgradePath.Special] >= 2) hpMax = 3;
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
            if (!(data.upgrades[SlagEquipmentData.UpgradePath.Power] == 0 || dmgType == DamageType.Unspecified))
                yield return Detonate(data.upgrades[SlagEquipmentData.UpgradePath.Power] == 2 ? 2 : 1);
            yield return base.DestroySequence(dmgType, source, sourceEquip);
       }
    }

    public virtual IEnumerator Detonate(int dmg) {
            explosion.gameObject.SetActive(true);
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
                        if (ge is Unit tu && ge != this) {
// POWER TIER II - Remove friendly fire
                            if (!(ge is PlayerUnit && data.upgrades[SlagEquipmentData.UpgradePath.Power] == 2))
                                affectedCo.Add(tu.StartCoroutine(tu.TakeDamage(dmg, DamageType.Explosion, this, data)));                        
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
            Debug.Log("Explosion Done");
    }

    public override void EnableSelection(bool state) {}
}
