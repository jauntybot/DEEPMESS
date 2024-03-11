using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class GridContextuals : MonoBehaviour {

    PlayerManager manager;
    FloorManager floorManager;
    [HideInInspector] public bool toggled = true;

    public enum ContextDisplay { None, IconOnly, Linear, Stepped, Parabolic };
    [SerializeField] private ContextDisplay currentContext = ContextDisplay.None;

    public bool displaying;

    [SerializeField] GameObject gridCursorPrefab;
    [SerializeField] GameObject gridCursor, contextCursor;
    List<GameObject> aoeCursors = new();
    Animator cursorAnimator;
    [SerializeField] NeutralElementTooltip neutralTooltip;

    [SerializeField] public Color playerColor, enemyColor, equipColor, validColor, invalidColor;
    
    [SerializeField] LineRenderer lr;
    int lrI = 0;
    GridElement fromOverride = null;

    public void Initialize(PlayerManager m) {
        manager = m;
        floorManager = FloorManager.instance;
        cursorAnimator = contextCursor.GetComponentInChildren<Animator>();
    
        ToggleValid(false);
    }

    public void ToggleValid(bool state = false) {
        contextCursor.SetActive(state);
        if (state == false && !displaying) {
            for (int i = aoeCursors.Count - 1; i >= 0; i--) {
                GameObject del = aoeCursors[i];
                aoeCursors.Remove(del);
                Destroy(del);
            }
        }
    }

    public void UpdateGridCursor(bool state, Vector2 coord, bool fill = false, bool _valid = true) {
        gridCursor.SetActive(state);
        gridCursor.transform.position = floorManager.currentFloor.PosFromCoord(coord);
        gridCursor.transform.parent = floorManager.currentFloor.transform;
        gridCursor.transform.localScale = new Vector3(FloorManager.sqrSize, FloorManager.sqrSize, FloorManager.sqrSize);

        if (state) {
            bool occupied = false;
            foreach (GridElement ge in floorManager.currentFloor.CoordContents(coord)) {
                if (ge is not Unit) {
                    occupied = true;
                    neutralTooltip.HoverOver(ge);
                }
            }
            if (!occupied) {
                Tile tile = floorManager.currentFloor.tiles.Find(t => t.coord == coord);
                if (tile != null)
                    neutralTooltip.HoverOver(tile);
            } 


            Color c = _valid ? validColor : invalidColor;
            List<GameObject> cursors = new();
            foreach (GameObject curs in aoeCursors) cursors.Add(curs);
            cursors.Add(gridCursor);
            foreach (GameObject cursor in cursors) {
                SpriteShapeRenderer ssr = cursor.GetComponentInChildren<SpriteShapeRenderer>();
                if (ssr) {
                    ssr.color = new Color(c.r, c.g, c.b, fill ? 0.25f : 0);
                    ssr.sortingOrder = floorManager.currentFloor.SortOrderFromCoord(coord);
                }
                LineRenderer lr = cursor.GetComponentInChildren<LineRenderer>();
                if (lr) {
                    lr.startColor = new Color(c.r, c.g, c.b, 0.75f); lr.endColor = new Color(c.r, c.g, c.b, 0.75f);
                    lr.sortingOrder = floorManager.currentFloor.SortOrderFromCoord(coord);
                }
            }
        } else 
            neutralTooltip.HoverOver();
    }

// Update call w/o Vector2 reference
    public void UpdateGridCursor(bool state, bool fill = false, bool _valid = true) {
        gridCursor.SetActive(state);
        
        if (state) {
            Color c = _valid ? validColor : invalidColor;

            SpriteShapeRenderer ssr = gridCursor.GetComponentInChildren<SpriteShapeRenderer>();
            if (ssr) {
                ssr.color = new Color(c.r, c.g, c.b, fill ? 0.25f : 0);
            }
            LineRenderer lr = gridCursor.GetComponentInChildren<LineRenderer>();
            if (lr) {
                lr.startColor = new Color(c.r, c.g, c.b, 0.75f); lr.endColor = new Color(c.r, c.g, c.b, 0.75f);
            }
        } else
        neutralTooltip.HoverOver();
    }

    public void DisplayGridContextuals(GridElement origin, EquipmentData data, int gridColor, GameObject refTrans = null) {
        if (toggled) {
            ToggleValid(data.contextDisplay != ContextDisplay.None);
            if (data.contextualAnimGO != null)
                UpdateCursorAnim(data.contextualAnimGO.transform);
            else if (refTrans)
                UpdateCursorAnim(refTrans.transform);
            UpdateContext(data, gridColor);        
            
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

    // Update grid position and coordinate
    public void UpdateCursor(Unit from, Vector2 to) {
        if (toggled) {
            ToggleValid(currentContext != ContextDisplay.None);

            Vector2 fromCoord = from.coord;
            if (fromOverride != null)
                fromCoord = fromOverride.coord;

            contextCursor.transform.position = floorManager.currentFloor.PosFromCoord(to);
            UpdateSortOrder(to);

            lr.positionCount = lrI + 3;
            lr.SetPosition(lrI, floorManager.currentFloor.PosFromCoord(fromCoord));
            lr.SetPosition(lrI + 1, floorManager.currentFloor.PosFromCoord(fromCoord));
            lr.SetPosition(lrI + 2, floorManager.currentFloor.PosFromCoord(fromCoord));
            switch(currentContext) {
                default:
                case ContextDisplay.None:
                case ContextDisplay.IconOnly:
                    lr.positionCount = 0;
                break;
                case ContextDisplay.Linear:
                    lr.positionCount = lrI + 6;
                    lr.SetPosition(lrI + 3, floorManager.currentFloor.PosFromCoord(to));
                    lr.SetPosition( lrI + 4, floorManager.currentFloor.PosFromCoord(to));
                    lr.SetPosition(lrI + 5, floorManager.currentFloor.PosFromCoord(to));
                break;
                case ContextDisplay.Stepped:
                    Dictionary<Vector2, Vector2> fromTo = EquipmentAdjacency.SteppedCoordAdjacency(fromCoord, to, from.selectedEquipment);
                    Vector2 prev = fromCoord;
                    lr.positionCount = lrI + (fromTo.Count + 1) * 3;
                    for (int i = 1; i <= fromTo.Count; i++) {
                        Vector3 linePos = floorManager.currentFloor.PosFromCoord(fromTo[prev]);
                        lr.SetPosition(lrI + 3*i, linePos); lr.SetPosition(lrI + 3*i + 1, linePos); lr.SetPosition(lrI + 3*i + 2, linePos);
                        prev = fromTo[prev];
                    }
                
                break;
                case ContextDisplay.Parabolic:
                    float h = 0.25f + Vector2.Distance(fromCoord, to) / 2;
                    List<Vector3> points = Util.SampledParabola(floorManager.currentFloor.PosFromCoord(fromCoord), floorManager.currentFloor.PosFromCoord(to), h, 24);
                    lr.positionCount = lrI + points.Count * 3;
                    for (int i = 1; i < points.Count; i++) {
                        lr.SetPosition(lrI + 3*i, points[i]); lr.SetPosition(lrI + 3*i + 1, points[i]); lr.SetPosition(lrI + 3*i + 2, points[i]);
                    }
                break;
            }        
        }
    }

    public void UpdateContext(EquipmentData data, int highlightIndex, ContextDisplay newContext = ContextDisplay.None, GridElement newAnim = null, GridElement newFrom = null) {
        currentContext = data.contextDisplay;
        if (newContext != ContextDisplay.None) currentContext = newContext;

        lrI = lr.positionCount;
        ChangeLineColor(highlightIndex);

        if (newAnim != null) {
            UpdateCursorAnim(newAnim.transform);
        }
        if (newFrom != null) {
            fromOverride = newFrom;  
        }

        for (int i = aoeCursors.Count - 1; i >= 0; i--) {
            GameObject del = aoeCursors[i];
            aoeCursors.Remove(del);
            Destroy(del);
        }
        if (data.aoeRange > 0) {
            List<Vector2> aoeCoords = EquipmentAdjacency.GetAdjacent(new Vector2(3,3), data.aoeRange, data, null, floorManager.currentFloor, true);
            for (int i = aoeCoords.Count - 1; i >= 0; i--) {
                GameObject cursor = Instantiate(gridCursorPrefab);
                gridCursor.transform.position = floorManager.currentFloor.PosFromCoord(new Vector2(3,3));
                cursor.transform.parent = gridCursor.transform;
                cursor.transform.position = floorManager.currentFloor.PosFromCoord(aoeCoords[i]);
                cursor.transform.localScale = Vector3.one;
                cursor.SetActive(true);
                aoeCursors.Add(cursor);
            }
        }

    }  

    public void UpdateCursorAnim(Transform refTrans) {
        cursorAnimator.GetComponent<NestedFadeGroup.NestedFadeGroupSpriteRenderer>().AlphaSelf = 0.5f;
        Animator anim = null;
        if (refTrans.GetComponentInChildren<Animator>())
            anim = refTrans.GetComponentInChildren<Animator>();
        if (anim) {
            cursorAnimator.enabled = true;
            cursorAnimator.runtimeAnimatorController = anim.runtimeAnimatorController;
            cursorAnimator.transform.localScale = anim.transform.localScale;
            cursorAnimator.transform.localPosition = anim.transform.localPosition;
        } else {
            cursorAnimator.enabled = false;
            cursorAnimator.gameObject.GetComponent<SpriteRenderer>().sprite = refTrans.GetComponentInChildren<SpriteRenderer>().sprite;
        }
    }

    public void ChangeLineColor(int highlightIndex) {
        Color gridColor = FloorManager.instance.GetFloorColor(highlightIndex);
        switch (highlightIndex) {
            case 0: gridColor = playerColor; break;
            case 1: gridColor = enemyColor; break;
            case 2: gridColor = equipColor; break;
            default:
            case 3: gridColor = invalidColor; break;
        }
        lr.startColor = gridColor; lr.endColor = gridColor;
    }

    public virtual void UpdateSortOrder(Vector2 c) {
        int sort = floorManager.currentFloor.SortOrderFromCoord(c);
        
        cursorAnimator.GetComponent<SpriteRenderer>().sortingOrder = sort;
    }


}
