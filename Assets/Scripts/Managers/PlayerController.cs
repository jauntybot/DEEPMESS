using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// Responsible for translating player input to the player manager

public class PlayerController : MonoBehaviour {

    PlayerManager manager;

    void Start() 
    {
        manager = GetComponent<PlayerManager>();
    }


// Coroutine that runs while the player is allowed to select elements on the grid
    public IEnumerator GridInput() {
        while (manager.scenario.currentTurn == ScenarioManager.Turn.Player) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            RaycastHit2D hit = ClickInput();
// On mouseover
            if (hit != default(RaycastHit2D)) {
                if (hit.transform.GetComponent<GridElement>()) {
// On click
                    manager.GridMouseOver(hit.transform.GetComponent<GridElement>().coord);
                    if (Input.GetMouseButtonDown(0)) {
// Pass call to contextualize click to manager
                        manager.GridInput(hit.transform.GetComponent<GridElement>());       
                    }
                }    
            }

            if (Input.GetKeyDown(KeyCode.Tab)) {
                manager.DisplayAllHP(true);
            } 
            if (Input.GetKeyUp(KeyCode.Tab)) {
                manager.DisplayAllHP(false);
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
