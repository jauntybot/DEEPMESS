using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class that manages the game state during battles

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

// Instanced refs
    Grid grid;
    [SerializeField] RedManager enemy;
    [SerializeField] BlueManager player;

// State machines
    public enum GameState { Setup, PlayerPlace, Battle, End }
    public GameState gameState;
    public enum Turn { Null, Player, Opponent, Environment }
    public Turn currentTurn;

    public IEnumerator Start() {
        if (Grid.instance) {
            grid=Grid.instance;
            yield return StartCoroutine(grid.GenerateGrid());
        }

        yield return StartCoroutine(enemy.Initialize());
        yield return StartCoroutine(player.Initialize());
        
        yield return new WaitForSeconds(1/Util.fps);
        SwitchTurns(Turn.Opponent);
    }

// Overload allows you to specify which turn to switch to, otherwise inverts the binary
    public void SwitchTurns(Turn fromTurn = Turn.Null) {
        switch(fromTurn == Turn.Null ? currentTurn : fromTurn) {
            case Turn.Player:
                player.StartEndTurn(false);
                currentTurn = Turn.Opponent;
                StartCoroutine(enemy.TakeTurn());
            break;
            case Turn.Opponent:
                currentTurn = Turn.Player;
                player.StartEndTurn(true);
            break;
        }
    }

// public function for UI buttons
    public void EndTurn() 
    {
        SwitchTurns();
    }
}
