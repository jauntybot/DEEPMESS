using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BlueManager))]
public class PlayerController : MonoBehaviour {

    BlueManager manager;

    void Start() {
        manager = GetComponent<BlueManager>();
    }

    public void ChangeStates(BlueManager.State targetState) {
        switch(targetState) {
            case BlueManager.State.Hand:
                StartCoroutine(HandInput());
            break;
            case BlueManager.State.Grid:
                StartCoroutine(GridInput());
            break;
            case BlueManager.State.Idle:

            break;
        }
    }

    IEnumerator HandInput() {
        while (manager.state == BlueManager.State.Hand) {
            yield return new WaitForSecondsRealtime(1/Util.fps);

            if (Input.GetMouseButtonDown(0)) {
                RaycastHit2D hit = ClickInput();
                if (hit != default(RaycastHit2D) &&
                    hit.transform.GetComponent<Card>()) {
                        if (manager.selectedCard == null) {
                            manager.SelectCard(hit.transform.GetComponent<Card>());
                        }
                }
            }
        }
    }

    IEnumerator GridInput() {
        while (manager.state == BlueManager.State.Grid) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) {
                RaycastHit2D hit = ClickInput();
                if (hit != default(RaycastHit2D) &&
                    hit.transform.GetComponent<Card>()) {

                    
                }
            }
        }
    }

//MOBILE CONTROL FLAG
    public RaycastHit2D ClickInput() {
        Vector2 pos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        RaycastHit2D hitInfo = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(pos), Vector2.zero);
// Collision detected
        if (hitInfo)  return hitInfo;        
        return default(RaycastHit2D);
    }

}
