using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioManager : MonoBehaviour
{

#region SINGLETON (and Awake)
    public static ScenarioManager instance;
    private void Awake() {
        if (ScenarioManager.instance) {
            Debug.Log("Warning! More than one instance of ScenarioManager found!");
            return;
        }
        ScenarioManager.instance = this;
    }
#endregion

    Grid grid;
    [SerializeField] TokenManager[] tknMngrs;
    [SerializeField] BlueManager player;

    public enum Turn { Null, Player, Opponent, Environment }
    public Turn currentTurn;
    int turnActions;
    public IEnumerator Start() {
        if (Grid.instance) {
            grid=Grid.instance;
            yield return StartCoroutine(grid.GenerateGrid());
        }
        foreach(TokenManager manager in tknMngrs) {
            yield return StartCoroutine(manager.Initialize());
        }
        yield return new WaitForSeconds(1/Util.fps);
        SwitchTurns(Turn.Opponent);
    }

// Overload allows you to specify which turn to switch to, otherwise inverts the binary
    public void SwitchTurns(Turn fromTurn = Turn.Null) {
        turnActions = 0;
        switch(fromTurn == Turn.Null ? currentTurn : fromTurn) {
            case Turn.Player:
                player.StartEndTurn(false);
                currentTurn = Turn.Opponent;
            break;
            case Turn.Opponent:
                currentTurn = Turn.Player;
                player.StartEndTurn(true);
            break;
        }
    }

    public void TurnAction() {
        turnActions++;
        if (turnActions >= 2) {
            SwitchTurns();
        }
    }

    public void EndTurn() {
        SwitchTurns();
    }
}
