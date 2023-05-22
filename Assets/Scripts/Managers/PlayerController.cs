using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// Responsible for translating player input to the player manager

public class PlayerController : MonoBehaviour {

    PlayerManager manager;
    [SerializeField] Texture2D cursorTexture;
    [SerializeField] bool clickable;

    void Start() 
    {
        manager = GetComponent<PlayerManager>();

        //StartCoroutine(HotkeyInput());

        Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.ForceSoftware);
        
    }


// Coroutine that runs while the player is allowed to select elements on the grid
    public IEnumerator GridInput() {
        while (manager.scenario.currentTurn == ScenarioManager.Turn.Player || manager.scenario.currentTurn == ScenarioManager.Turn.Cascade) {
            clickable = true;
            yield return new WaitForSecondsRealtime(1/Util.fps);
            RaycastHit2D hit = ClickInput();
// On mouseover
            if (hit != default(RaycastHit2D)) {
                if (hit.transform.GetComponent<GridElement>()) {
// On click
                    manager.GridMouseOver(hit.transform.GetComponent<GridElement>().coord, true);
// Disable input under these conditions
                    if (!manager.unitActing && !(FloorManager.instance.peeking && manager.scenario.currentTurn != ScenarioManager.Turn.Cascade)) {
                        if (Input.GetMouseButtonDown(0)) {
// Pass call to contextualize click to manager
                            manager.GridInput(hit.transform.GetComponent<GridElement>());       
                            yield return new WaitForSecondsRealtime(1/Util.fps);
                        }
                    }
                }    
                else
                    manager.GridMouseOver(new Vector2(-32, -32), false);
            } else
                manager.GridMouseOver(new Vector2(-32, -32), false);

            if (Input.GetKeyDown(KeyCode.Tab)) {
                manager.DisplayAllHP(true);
            } 
            if (Input.GetKeyUp(KeyCode.Tab)) {
                manager.DisplayAllHP(false);
            }
        }
        clickable = false;
    }

    public IEnumerator HotkeyInput() {
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetKeyDown(KeyCode.Escape)) {
                PersistentMenu.instance.pauseMenu.gameObject.SetActive(true);
            }
            if (manager) {
                if (manager.scenario) {
                    if (manager.scenario.currentTurn == ScenarioManager.Turn.Player || manager.scenario.currentTurn == ScenarioManager.Turn.Cascade) {
                        if (Input.GetKeyDown(KeyCode.A)) {
                            manager.SelectUnit(manager.units[0]);
                        }
                        if (Input.GetKeyDown(KeyCode.S)) {
                            manager.SelectUnit(manager.units[1]);
                        }
                        if (Input.GetKeyDown(KeyCode.D)) {
                            manager.SelectUnit(manager.units[2]);
                        }
                        if (Input.GetKeyDown(KeyCode.E)) {
                            if (manager.selectedUnit) {
                                if (!manager.selectedUnit.usedEquip && manager.selectedUnit.energyCurrent > 0) {
                                    manager.selectedUnit.selectedEquipment = manager.selectedUnit.equipment[1];
                                    manager.selectedUnit.UpdateAction(manager.selectedUnit.selectedEquipment);
                                    if (manager.selectedUnit.ui.equipSelectSFX)
                                        UIManager.instance.PlaySound(manager.selectedUnit.ui.equipSelectSFX.Get());
                                }
                            }
                        }
                        if (Input.GetKeyDown(KeyCode.W)) {
                            if (manager.selectedUnit) {
                                if (manager.selectedUnit.equipment.Find(e => e is HammerData) && manager.selectedUnit.energyCurrent > 0) {
                                    manager.selectedUnit.selectedEquipment = manager.selectedUnit.equipment.Find(e => e is HammerData);
                                    manager.selectedUnit.UpdateAction(manager.selectedUnit.selectedEquipment);
                                    if (manager.selectedUnit.ui.hammerSelectSFX)
                                        UIManager.instance.PlaySound(manager.selectedUnit.ui.hammerSelectSFX.Get());
                                }
                            }
                        }
                        if (Input.GetKeyDown(KeyCode.Q)) {
                            manager.DeselectUnit();
                        }
                        if (Input.GetKeyDown(KeyCode.Z)) {
                            manager.UndoMove();
                        }
                        if (Input.GetKeyDown(KeyCode.Space)) {
                            FloorManager.instance.previewManager.PreviewButton(!FloorManager.instance.peeking);
                        }
                        if (Input.GetKeyDown(KeyCode.T)) {
                            ScenarioManager.instance.EndTurn();
                        }
                    }
                }
            }
        }
    }

    

//MOBILE CONTROL FLAG
    public RaycastHit2D ClickInput() {
        Vector2 pos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        RaycastHit2D hitInfo = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(pos), Vector2.zero);
        return hitInfo;        
    }

}
