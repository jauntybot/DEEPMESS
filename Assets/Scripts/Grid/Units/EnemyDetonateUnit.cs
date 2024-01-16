using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetonateUnit : EnemyUnit {

    public bool primed;
    [SerializeField] GameObject explosionVFX;

    public void PrimeSelf() {
        primed = true;
        gfxAnim.SetBool("Primed", true);
    }

    public IEnumerator Explode() {
        gfxAnim.SetTrigger("Explode");
        Grid grid = FloorManager.instance.currentFloor;
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

    public override IEnumerator ScatterTurn() {
        if (!conditions.Contains(Status.Stunned)) {
            if (!primed)
                yield return base.ScatterTurn();
            else
                yield return StartCoroutine(ExplodeCo());
        } else {
            moved = true;
            energyCurrent = 0;
            yield return new WaitForSecondsRealtime(0.125f);
            RemoveCondition(Status.Stunned);
        }
    
    }

    public override IEnumerator CalculateAction() {
        if (!conditions.Contains(Status.Stunned)) {
            if (!primed)
                yield return base.CalculateAction();
            else 
                yield return StartCoroutine(ExplodeCo());
        }
        else {
            moved = true;
            energyCurrent = 0;
            yield return new WaitForSecondsRealtime(0.125f);
        }

        grid.DisableGridHighlight();
        manager.unitActing = false;
    }

    IEnumerator ExplodeCo() {

        manager.SelectUnit(this);
        UpdateAction(equipment[1]);
        grid.DisplayValidCoords(validActionCoords, selectedEquipment.gridColor);
        yield return new WaitForSecondsRealtime(0.5f);
        StartCoroutine(selectedEquipment.UseEquipment(this, null));
        grid.UpdateSelectedCursor(false, Vector2.one * -32);
        grid.DisableGridHighlight();
        Coroutine co = StartCoroutine(TakeDamage(hpCurrent));
        manager.DeselectUnit();
        yield return co;
    }

    public override IEnumerator DestroySequence(DamageType dmgType = DamageType.Unspecified, GridElement source = null, EquipmentData sourceEquip = null) {
        if (!destroyed) {
            if (primed) {
                ObjectiveEventManager.Broadcast(GenerateDestroyEvent(dmgType, source, sourceEquip));        
                yield return StartCoroutine(ExplodeCo());
            } else
                yield return base.DestroySequence(dmgType, source, sourceEquip);
        }
    }

}
