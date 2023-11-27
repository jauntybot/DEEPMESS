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
        if (data.upgrades[SlagEquipmentData.UpgradePath.Special] == 1) hpMax = 3;
        if (data.upgrades[SlagEquipmentData.UpgradePath.Special] >= 2) hpMax = 4;
        hpCurrent = hpMax;
        if (elementCanvas == null)  {
            elementCanvas = GetComponentInChildren<ElementCanvas>();
            elementCanvas.Initialize(this);
        } else
            elementCanvas.InstantiateMaxPips();

    }

    public override IEnumerator CollideFromAbove(GridElement subGE) {

        yield return StartCoroutine(TakeDamage(hpCurrent));

    }

    public override IEnumerator DestroySequence(DamageType dmgType = DamageType.Unspecified) {
        if (data.upgrades[SlagEquipmentData.UpgradePath.Special] < 3 || dmgType == DamageType.Unspecified)
            yield return base.DestroySequence(dmgType);
        else {
            yield return Detonate();
            yield return base.DestroySequence(dmgType);
        }
    }

    public virtual IEnumerator Detonate() {
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
                            affectedCo.Add(tu.StartCoroutine(tu.TakeDamage(2)));
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

    public virtual IEnumerator PushOnLanding() {
        explosion.gameObject.SetActive(true);
        List<Vector2> aoe = EquipmentAdjacency.OrthagonalAdjacency(coord, 1, targetTypes);
        grid.DisplayValidCoords(aoe, 2);
        List<Coroutine> affectedCo = new();
        foreach (Vector2 _coord in aoe) {
            if (grid.CoordContents(_coord).Count > 0) {
                foreach (GridElement ge in grid.CoordContents(_coord)) {
                    if (ge is Unit tu && ge != this) {
                        float xDelta = _coord.x - coord.x;
                        float yDelta = _coord.y - coord.y;
                        Vector2 targetCoord = new(_coord.x + xDelta, _coord.y + yDelta);
                        
                        if (grid.CoordContents(targetCoord).Count == 0 && targetCoord.x >= 0 && targetCoord.x <= 7 && targetCoord.y >= 0 && targetCoord.y <= 0) 
                            affectedCo.Add(StartCoroutine(PushToCoord(tu, targetCoord)));
                        

                        if (data.upgrades[SlagEquipmentData.UpgradePath.Power] == 3) StartCoroutine(tu.TakeDamage(1, DamageType.Melee));
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
        grid.DisableGridHighlight();
        yield return new WaitForSecondsRealtime(0.15f);
        Debug.Log("Push Done");
    }

    public virtual IEnumerator PushToCoord(Unit unit, Vector2 moveTo) {       
// Build frontier dictionary for stepped lerp
        Dictionary<Vector2, Vector2> fromTo = new() { { unit.coord, moveTo } };

        Vector2 current = unit.coord;
        unit.coord = moveTo;
        
// Lerp units position to target
        while (!Vector2.Equals(current, moveTo)) {
// exposed UpdateElement() functionality to selectively update sort order
            if (unit.grid.SortOrderFromCoord(fromTo[current]) > unit.grid.SortOrderFromCoord(current))
                unit.UpdateSortOrder(fromTo[current]);
            Vector3 toPos = FloorManager.instance.currentFloor.PosFromCoord(fromTo[current]);
            float timer = 0;
            while (timer < .2f) {
                yield return null;
                unit.transform.position = Vector3.Lerp(unit.transform.position, toPos, timer/.2f);
                timer += Time.deltaTime;
            }
            current = fromTo[current];
            yield return null;
        }        
        unit.UpdateElement(moveTo);
    }

    public override void EnableSelection(bool state) {}
}
