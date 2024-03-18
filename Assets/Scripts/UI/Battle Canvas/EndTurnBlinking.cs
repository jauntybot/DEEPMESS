using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndTurnBlinking : MonoBehaviour
{
    ScenarioManager scenario;
    PlayerManager playerManager;
    [SerializeField] Animator highlightAnim;

    private bool outOfEnergy;

    private bool blinking = false;

    // Start is called before the first frame update
    void Start()
    {
        scenario = ScenarioManager.instance;
        playerManager = scenario.player;
    }



    bool CheckEnergy() {
        outOfEnergy = true;
        foreach (Unit unit in playerManager.units) {
            if (unit is PlayerUnit pUnit) {
                if (pUnit.energyCurrent > 0) outOfEnergy = false;
            }
        }
        return outOfEnergy;
    }

    public void BlinkEndTurn() {
        if (CheckEnergy()){
            if (!blinking) StartCoroutine(BlinkButton());
        } else if (blinking) {
            blinking = !blinking;
        }
    }

    public IEnumerator BlinkButton() {
        blinking = true;
        highlightAnim.SetBool("Active", true);
        while (blinking && scenario.currentTurn == ScenarioManager.Turn.Player) {
            yield return null;
        }
        highlightAnim.SetBool("Active", false);
        blinking = false;
    }
}
