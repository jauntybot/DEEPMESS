using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetonateUnit : EnemyUnit
{


    public bool primed;
    [SerializeField] Animator explosion;

    public void PrimeSelf() {
        primed = true;
        Debug.Log("Primed");
        gfxAnim.SetBool("Primed", true);
    }

    public void Explode() {
        gfxAnim.SetTrigger("Explode");
        explosion.gameObject.SetActive(true);
    }

    public override IEnumerator CalculateAction()
    {
        if (!primed) yield return base.CalculateAction();
        else {
            manager.SelectUnit(this);
            UpdateAction(equipment[1]);
            grid.DisplayValidCoords(validActionCoords, selectedEquipment.gridColor);
            yield return new WaitForSecondsRealtime(0.5f);
            Coroutine co = StartCoroutine(selectedEquipment.UseEquipment(this, null));
            grid.UpdateSelectedCursor(false, Vector2.one * -32);
            grid.DisableGridHighlight();
            yield return co;
            yield return new WaitForSecondsRealtime(0.125f);
            manager.DeselectUnit();
            yield break;

        }
    }

}
