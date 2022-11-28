using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Responsible for translating player input to the player manager

public class PlayerController : MonoBehaviour {

    BlueManager manager;

    void Start() 
    {
        manager = GetComponent<BlueManager>();
    }


// Coroutine that runs while the player is allowed to select cards from their hand
    public IEnumerator HandInput() {
        while (manager.scenario.currentTurn == ScenarioManager.Turn.Player) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
// On click
            if (Input.GetMouseButtonDown(0)) {
                RaycastHit2D hit = ClickInput();
// If clicked a card
                if (hit != default(RaycastHit2D) && 
                    hit.transform.GetComponent<Card>()) {
// Select card
                        if (hit.transform.GetComponent<Card>() == manager.selectedCard) 
                            manager.DeselectCard();                       
                        else manager.SelectCard(hit.transform.GetComponent<Card>());                     
                }
            }
        }
    }

// Coroutine that runs while the player is allowed to select elements on the grid
    public IEnumerator GridInput() {
        while (manager.scenario.currentTurn == ScenarioManager.Turn.Player) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
// On click
            if (Input.GetMouseButtonDown(0)) {
                RaycastHit2D hit = ClickInput();
                if (hit != default(RaycastHit2D)) {
// If clicked a grid element                           
                    if (hit.transform.GetComponent<Token>()) {
                        if (hit.transform.GetComponent<Token>() == manager.selectedToken)
                            manager.DeselectToken();
                        else manager.SelectToken(hit.transform.GetComponent<Token>());
                    }
                    if (hit.transform.GetComponent<GridSquare>()) {
                        if (manager.selectedToken)
                            manager.MoveToken(hit.transform.GetComponent<GridSquare>().coord);
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
