using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DescentPreview : MonoBehaviour {
    public Unit unit;
    [SerializeField] SpriteRenderer portraitSR, bgSR;
    Sprite portrait;
    public Animator anim;

    DescentPreviewManager mgmt;

    public void Initialize(Unit u, DescentPreviewManager manager) {
        unit = u;
        u.ElementUpdated += UpdatePreview;
        u.ElementDestroyed += DestroySelf;
        anim.gameObject.SetActive(true);
        gameObject.name = u.name + " Descent Preview";
        portrait = u.portrait;
        portraitSR.sprite = portrait;
        if (u is EnemyUnit) {
            portraitSR.transform.localPosition = new Vector3(-0.031f, -1,0);
            anim.SetInteger("Element", 1);
        } else if (u is PlayerUnit) {
            portraitSR.transform.localPosition = new Vector3(0.028f,-0.25f,0);
            anim.SetInteger("Element", 0);
        } else if (u is Anvil) {
            portraitSR.transform.localPosition = new Vector3(0, -0.25f, 0);
            portraitSR.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            anim.SetInteger("Element", 2);
        }
        anim.keepAnimatorStateOnDisable = true;
        anim.gameObject.SetActive(false);
        mgmt = manager;
        mgmt.descentPreviews.Add(this);
    }

    void DestroySelf(GridElement blank) {
        Debug.Log("Remove Preview");
        mgmt.descentPreviews.Remove(this);
        Destroy(gameObject);
    }

    public void UpdatePreview(GridElement ge) {
        //anim.gameObject.SetActive(true);
        if (mgmt.alignmentFloor) {
            transform.localPosition = mgmt.alignmentFloor.PosFromCoord(ge.coord);
            anim.gameObject.GetComponent<SpriteRenderer>().sortingOrder = mgmt.alignmentFloor.SortOrderFromCoord(ge.coord) - 1;
            portraitSR.sortingOrder = mgmt.alignmentFloor.SortOrderFromCoord(ge.coord);
            bgSR.sortingOrder = mgmt.alignmentFloor.SortOrderFromCoord(ge.coord) - 1;

        }
    }

    public void HighlightTile(bool state) {
        if (state) {
            int index = 0;
            if (unit is PlayerUnit) index = 0;
            else if (unit is EnemyUnit) index = 1;
            else index = 2;
            Color c = FloorManager.instance.GetFloorColor(index);
            mgmt.alignmentFloor.tiles.Find(t => t.coord == unit.coord).ToggleValidCoord(true, c, true);
        } else 
            FloorManager.instance.currentFloor.DisableGridHighlight();       
    }
}
