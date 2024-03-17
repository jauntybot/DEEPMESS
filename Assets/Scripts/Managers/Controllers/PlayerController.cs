using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// Responsible for translating player input to the player manager

public class PlayerController : MonoBehaviour {


    PlayerManager manager;

    public enum CursorState { Default, Button, Move, Target, };
    public CursorState cursorState;
    [SerializeField] Vector2 pointerAnchor;
    [SerializeField] Texture2D defaultCursor, clickableCursor, moveCursor, nullMoveCursor, targetCursor, nullTargetCursor, buttonHoverCursor;
    int layerMask = 5;

    bool init = false;
    public void Init() {
        layerMask = ~layerMask;

        manager = GetComponent<PlayerManager>();
        UpdateCursor();
        init = true;
    }


    public void UpdateCursor(CursorState state = CursorState.Default) {
        cursorState = state;
        ToggleCursorValid(false);
    }

    public void ToggleCursorValid(bool state) {
        Texture2D targetTexture = defaultCursor;
        switch(cursorState) {
            case CursorState.Default:
                targetTexture = state ? clickableCursor : defaultCursor;
            break;
            case CursorState.Button:
                targetTexture = buttonHoverCursor;
            break;
            case CursorState.Move:
                targetTexture = state ? moveCursor : nullMoveCursor;
            break;
            case CursorState.Target:
                targetTexture = state ? targetCursor : nullTargetCursor;
            break;
        }

        Cursor.SetCursor(targetTexture, pointerAnchor, CursorMode.ForceSoftware);
    }

    public void Update() {
        if (init) {
            if (manager.scenario.currentTurn == ScenarioManager.Turn.Player || manager.scenario.currentTurn == ScenarioManager.Turn.Cascade) {
                if (!FloorManager.instance.descending && !manager.unitActing) {
                    RaycastHit2D hit = ClickInput();
    // On mouseover
                    if (hit != default(RaycastHit2D) && !MouseOverUI()) {
                        if (hit.transform.GetComponent<GridElement>()) {
    // On click
                            manager.GridMouseOver(hit.transform.GetComponent<GridElement>().coord, true);
    // Disable input under these conditions
                            if (!manager.unitActing && !(FloorManager.instance.peeking && manager.scenario.currentTurn != ScenarioManager.Turn.Cascade)) {
                                if (Input.GetMouseButtonDown(0)) {
    // Pass call to contextualize click to manager
                                    manager.GridInput(hit.transform.GetComponent<GridElement>());
                                }
                            }
                        } else {
                            manager.GridMouseOver(new Vector2(-32, -32), false);
                        }
                    } else {
                        manager.GridMouseOver(new Vector2(-32, -32), false);
                    }

                    if (Input.GetMouseButtonDown(1)) {
                        manager.DeselectUnit();
                    }
                } 
                
                OnTurnHotkeyInput();
            }
        }
    }

    public void OnTurnHotkeyInput() {
        if (!manager.unitActing) {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                if (!PersistentMenu.instance.pauseMenu.isActiveAndEnabled)
                    PersistentMenu.instance.pauseMenu.gameObject.SetActive(true);
                else
                    PersistentMenu.instance.pauseMenu.ResumeButton();
                
            }

            if (manager && manager.scenario && !FloorManager.instance.peeking && !PersistentMenu.instance.pauseMenu.isActiveAndEnabled) {
                
                if (Input.GetKeyDown(KeyCode.P) && ScenarioManager.instance.currentTurn == ScenarioManager.Turn.Player && !FloorManager.instance.transitioning && UIManager.instance.peekButton.interactable) {
                    PersistentMenu.instance.TriggerCascade();
                }
                if (Input.GetKeyDown(KeyCode.A)) {
                    manager.SelectUnit(manager.units[0]);
                }
                if (Input.GetKeyDown(KeyCode.S)) {
                    manager.SelectUnit(manager.units[1]);
                }
                if (Input.GetKeyDown(KeyCode.D)) {
                    manager.SelectUnit(manager.units[2]);
                }
                // if (Input.GetKeyDown(KeyCode.E)) {
                //     if (manager.selectedUnit) {
                //         if (!manager.selectedUnit.usedEquip && manager.selectedUnit.energyCurrent > 0) {
                //             manager.selectedUnit.selectedEquipment = manager.selectedUnit.equipment[1];
                //             manager.selectedUnit.UpdateAction(manager.selectedUnit.selectedEquipment);
                //             if (manager.selectedUnit.ui.equipSelectSFX)
                //                 UIManager.instance.PlaySound(manager.selectedUnit.ui.equipSelectSFX.Get());
                //         }
                //     }
                // }
                // if (Input.GetKeyDown(KeyCode.W)) {
                //     if (manager.selectedUnit) {
                //         if (manager.selectedUnit.equipment.Find(e => e is HammerData) && manager.selectedUnit.energyCurrent > 0) {
                //             manager.selectedUnit.selectedEquipment = manager.selectedUnit.equipment.Find(e => e is HammerData);
                //             manager.selectedUnit.UpdateAction(manager.selectedUnit.selectedEquipment);
                //             if (manager.selectedUnit.ui.hammerSelectSFX)
                //                 UIManager.instance.PlaySound(manager.selectedUnit.ui.hammerSelectSFX.Get());
                //         }
                //     }
                // }

                if (Input.GetKeyDown(KeyCode.Z)) 
                    manager.UndoMove();
                
                if (Input.GetKeyDown(KeyCode.E)) 
                    ScenarioManager.instance.EndTurn();
            }
        
            if (Input.GetKeyDown(KeyCode.Space)) 
                FloorManager.instance.previewManager.PreviewButton();
        }
    }

    

//MOBILE CONTROL FLAG
    public RaycastHit2D ClickInput() {
        Vector2 pos = new(Input.mousePosition.x, Input.mousePosition.y);
        RaycastHit2D hitInfo = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(pos), Vector2.zero, Mathf.Infinity);
        return hitInfo;        
    }

    bool MouseOverUI() {
        PointerEventData ped = new(EventSystem.current);
        ped.position = Input.mousePosition;

        List<RaycastResult> rays = new();
        EventSystem.current.RaycastAll(ped, rays);

        for (int i = rays.Count - 1; i >= 0; i--) {
            if (rays[i].gameObject.layer != 5 && rays[i].gameObject.layer != 6)
                rays.Remove(rays[i]);
        }   
        return rays.Count > 0;
    }

}
