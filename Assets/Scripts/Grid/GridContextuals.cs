using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridContextuals : MonoBehaviour
{

    PlayerManager manager;
    [HideInInspector] public bool toggled = true;
    public Grid grid;

    public enum ContextDisplay { None, IconOnly, Linear, Stepped, Parabolic };
    [SerializeField] private ContextDisplay currentContext = ContextDisplay.None;

    public bool displaying;
    [SerializeField] GameObject contextCursor;
    Animator cursorAnimator;

    [SerializeField] Color playerColor, enemyColor, equipColor;
    
    [SerializeField] LineRenderer lr;
    int lrI = 0;
    GridElement fromOverride = null;

    public void Initialize(PlayerManager m) {
        manager = m;
        grid = manager.currentGrid;
        cursorAnimator = contextCursor.GetComponentInChildren<Animator>();
        
        ToggleValid(false);
    }

    public void DisplayGridContextuals(GridElement origin, GameObject refTrans, ContextDisplay context, int gridColor) {
        if (toggled) {
            ToggleValid(context != ContextDisplay.None);

            if (refTrans)
                UpdateCursorAnim(refTrans.transform);
            UpdateContext(context, gridColor);        
            
            if (origin)
                UpdateCursor((Unit)origin, origin.coord);
        }
    }

    public void StartUpdateCoroutine() {
        StopAllCoroutines();
        ResetLR();
        StartCoroutine(UpdateCoroutine());
    }

    public IEnumerator UpdateCoroutine() {
        displaying = true;
        while (manager.selectedUnit != null && !manager.unitActing && displaying) {
            
            yield return null;
        }
        ResetLR();
    }

    void ResetLR() {
        lrI = 0;
        lr.positionCount = lrI;
        fromOverride = null;
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
    public void UpdateCursor(Unit from, Vector2 to) {
        if (toggled) {
            ToggleValid(currentContext != ContextDisplay.None);

            Vector2 fromCoord = from.coord;
            if (fromOverride != null)
                fromCoord = fromOverride.coord;

            contextCursor.transform.position = grid.PosFromCoord(to);
            UpdateSortOrder(to);

            lr.positionCount = lrI + 3;
            lr.SetPosition(lrI, grid.PosFromCoord(fromCoord));
            lr.SetPosition(lrI + 1, grid.PosFromCoord(fromCoord));
            lr.SetPosition(lrI + 2, grid.PosFromCoord(fromCoord));
            switch(currentContext) {
                default:
                case ContextDisplay.None:
                case ContextDisplay.IconOnly:
                    lr.positionCount = 0;
                break;
                case ContextDisplay.Linear:
                    lr.positionCount = lrI + 6;
                    lr.SetPosition(lrI + 3, grid.PosFromCoord(to));
                    lr.SetPosition( lrI + 4, grid.PosFromCoord(to));
                    lr.SetPosition(lrI + 5, grid.PosFromCoord(to));
                break;
                case ContextDisplay.Stepped:
                    Dictionary<Vector2, Vector2> fromTo = EquipmentAdjacency.SteppedCoordAdjacency(fromCoord, to, from.selectedEquipment);
                    Vector2 prev = fromCoord;
                    lr.positionCount = lrI + (fromTo.Count + 1) * 3;
                    for (int i = 1; i <= fromTo.Count; i++) {
                        Vector3 linePos = grid.PosFromCoord(fromTo[prev]);
                        lr.SetPosition(lrI + 3*i, linePos); lr.SetPosition(lrI + 3*i + 1, linePos); lr.SetPosition(lrI + 3*i + 2, linePos);
                        prev = fromTo[prev];
                    }
                
                break;
                case ContextDisplay.Parabolic:
                    float h = 0.25f + Vector2.Distance(fromCoord, to) / 2;
                    List<Vector3> points = Util.SampledParabola(grid.PosFromCoord(fromCoord), grid.PosFromCoord(to), h, 24);
                    lr.positionCount = lrI + points.Count * 3;
                    for (int i = 1; i < points.Count; i++) {
                        lr.SetPosition(lrI + 3*i, points[i]); lr.SetPosition(lrI + 3*i + 1, points[i]); lr.SetPosition(lrI + 3*i + 2, points[i]);
                    }
                break;
            }        
        }
    }

    public void UpdateContext(ContextDisplay context, int highlightIndex, GridElement newAnim = null, GridElement newFrom = null) {
        currentContext = context;
        lrI = lr.positionCount;
        Color gridColor = FloorManager.instance.GetFloorColor(highlightIndex);
        switch (highlightIndex) {
            default:
            case 0: gridColor = playerColor; break;
            case 1: gridColor = enemyColor; break;
            case 2: gridColor = equipColor; break;
        }
        lr.startColor = gridColor; lr.endColor = gridColor;

        if (newAnim != null) {
            UpdateCursorAnim(newAnim.transform);
        }
        if (newFrom != null) {
            fromOverride = newFrom;  
        }
    }  

    public virtual void UpdateSortOrder(Vector2 c) {
        int sort = grid.SortOrderFromCoord(c);
        
        cursorAnimator.GetComponent<SpriteRenderer>().sortingOrder = sort;
    }

    public void ToggleValid(bool state = false) {
        contextCursor.SetActive(state);
    }

}
