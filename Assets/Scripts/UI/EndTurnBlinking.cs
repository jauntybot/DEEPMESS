using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndTurnBlinking : MonoBehaviour
{
    ScenarioManager scenario;
    PlayerManager playerManager;
    [SerializeField] GameObject endTurn;

    private bool outOfEnergy;
    private Image endTurnButton;
    private Color startColor = Color.white;
    private Color endColor = new Color(0.27f, 0.49f, 0.76f, 1);
    private float speed = 1;
    private bool blinking = false;

    // Start is called before the first frame update
    void Start()
    {
        scenario = ScenarioManager.instance;
        playerManager = scenario.player;
        endTurnButton = endTurn.GetComponent<Image>();
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

    public void BlinkEndTurn()
    {
        if (!CheckEnergy()){
            endTurnButton.color = Color.white;
        } else {
            if (!blinking) StartCoroutine(BlinkButton());
        }
    }

    public IEnumerator BlinkButton() {
        blinking = true;
        while (scenario.currentTurn == ScenarioManager.Turn.Player) {
            endTurnButton.color = Color.Lerp(startColor, endColor, Mathf.PingPong(Time.time * speed, 1));
            yield return null;
        }
        blinking = false;
    }
}
