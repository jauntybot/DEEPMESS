using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridContextuals : MonoBehaviour
{

    PlayerManager manager;
    public Grid grid;

    public enum ContextDisplay { None, Linear, Stepped, Parabolic };
    private ContextDisplay currentContext = ContextDisplay.None;

    [HideInInspector] public bool displaying;
    [SerializeField] GameObject contextCursor;
    Animator cursorAnimator;
    [SerializeField] List<Vector2> targetCoords;
    [SerializeField] LineRenderer lr;


    public void Initialize(PlayerManager m) {
        manager = m;
        grid = manager.currentGrid;
        cursorAnimator = contextCursor.GetComponentInChildren<Animator>();
        ToggleValid(false);
    }


    public IEnumerator DisplayGridContextuals(GridElement origin, GameObject refTrans, ContextDisplay context) {
        displaying = true;
        ToggleValid(true);
        UpdateCursorAnim(refTrans.transform);
        UpdateElement(origin.coord);

        lr.SetPosition(0, grid.PosFromCoord(origin.coord));
        while (manager.selectedUnit != null && !manager.unitActing && displaying) {
            
            yield return new WaitForSecondsRealtime(1/Util.fps);
        }
        ToggleValid(false);
        displaying = false;
    }

    public void UpdateCursorAnim(Transform refTrans) {
        cursorAnimator.GetComponent<NestedFadeGroup.NestedFadeGroupSpriteRenderer>().AlphaSelf = 0.5f;
        Animator anim = refTrans.GetComponentInChildren<Animator>();
        cursorAnimator.runtimeAnimatorController = anim.runtimeAnimatorController;
        cursorAnimator.transform.localScale = anim.transform.localScale;
        cursorAnimator.transform.localPosition = anim.transform.localPosition;
    }

    // Update grid position and coordinate
    public void UpdateElement(Vector2 c) {
        if (currentContext != ContextDisplay.None)
            ToggleValid(true);

        Vector3 pos = grid.PosFromCoord(c);
        contextCursor.transform.position = pos;

        UpdateSortOrder(c);

        
        lr.SetPosition(lr.positionCount-1, pos);
    }  

    public virtual void UpdateSortOrder(Vector2 c) {
        int sort = grid.SortOrderFromCoord(c);
        
        cursorAnimator.GetComponent<SpriteRenderer>().sortingOrder = sort;
    }

    public void ToggleValid(bool state = false) {
        contextCursor.SetActive(state);
    }

}
