using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndTurnBlinking : MonoBehaviour
{
    [SerializeField] PlayerManager playerManager;
    [SerializeField] GameObject endTurn;

    private bool outOfEnergy;
    private Image endTurnButton;
    private Color startColor = Color.white;
    private Color endColor = new Color(0.27f, 0.49f, 0.76f, 1);
    private float speed = 1;

    // Start is called before the first frame update
    void Start()
    {
        endTurnButton = endTurn.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        BlinkEndTurn(CheckEnergy());
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

    void BlinkEndTurn(bool energy)
    {
        if (!energy){
            endTurnButton.color = Color.white;
        } else {
            endTurnButton.color = Color.Lerp(startColor, endColor, Mathf.PingPong(Time.time * speed, 1));
        }
    }
}
