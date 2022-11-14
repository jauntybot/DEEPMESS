using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BlueManager))]
public class PlayerController : MonoBehaviour {


    BlueManager manager;


    void Start() {

        manager = GetComponent<BlueManager>();
    }


    public IEnumerator HandInput() {
        while (manager.scenario.currentTurn == ScenarioManager.Turn.Player) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) {
                RaycastHit2D hit = ClickInput();
                if (hit != default(RaycastHit2D) && 
                    hit.transform.GetComponent<Card>()) {
                        if (hit.transform.GetComponent<Card>() == manager.selectedCard) 
                            manager.DeselectCard();                       
                        else manager.SelectCard(hit.transform.GetComponent<Card>());                     
                }
            }
        }
    }

    public IEnumerator GridInput() {
        while (manager.scenario.currentTurn == ScenarioManager.Turn.Player) {
            yield return new WaitForSecondsRealtime(1/Util.fps);

            if (Input.GetMouseButtonDown(0)) {
                RaycastHit2D hit = ClickInput();
                if (hit != default(RaycastHit2D)) {                           
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
// Collision detected
        return hitInfo;        
    }

}
