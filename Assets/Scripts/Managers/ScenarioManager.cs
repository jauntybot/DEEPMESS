using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    FloorManager floorManager;
    [SerializeField] GameObject enemyPrefab;
    public EnemyManager currentEnemy;
    public PlayerManager player;
    public LevelDefinition lvlDef;
    public Button endTurnButton;

    [SerializeField] MessagePanel messagePanel;

// State machines
    public enum GameState { Null, Setup, PlayerPlace, Battle, End }
    public GameState gameState;
    public enum Turn { Null, Player, Enemy, Environment }
    public Turn currentTurn;

#region Initialization
    public IEnumerator Start() 
    {
        if (FloorManager.instance) 
        {
            floorManager = FloorManager.instance;
            UnitManager enemy = Instantiate(enemyPrefab).GetComponent<EnemyManager>();
            yield return StartCoroutine(floorManager.GenerateFloor(true, enemy)); 
            currentEnemy = (EnemyManager)enemy;
        }
        yield return StartCoroutine(player.Initialize());
        
        yield return new WaitForSeconds(0.75f);
        yield return StartCoroutine(floorManager.TransitionFloors(floorManager.currentFloor.gameObject));

        UnitManager newEnemy = Instantiate(enemyPrefab).GetComponent<EnemyManager>();
        if (floorManager) yield return StartCoroutine(floorManager.GenerateFloor(true, newEnemy));

        yield return new WaitForSeconds(0.75f);
        floorManager.SwitchFloors(true);
        yield return new WaitForSeconds(1);

        StartCoroutine(SwitchTurns(Turn.Enemy));
    }

#endregion

    public void SwitchStates(GameState fromState = GameState.Null) 
    {

    }

// Overload allows you to specify which turn to switch to, otherwise inverts the binary
    public IEnumerator SwitchTurns(Turn fromTurn = Turn.Null) 
    {
        switch(fromTurn == Turn.Null ? currentTurn : fromTurn) 
        {
            case Turn.Player:
                player.StartEndTurn(false);
                if (currentEnemy.units.Count > 0) {
                    yield return StartCoroutine(messagePanel.DisplayMessage("ENEMY TURN"));
                    currentTurn = Turn.Enemy;
                    endTurnButton.enabled = false;
                    StartCoroutine(currentEnemy.TakeTurn());
                } else {
                    yield return StartCoroutine(messagePanel.DisplayMessage("PLAYER WINS"));
                    yield return new WaitForSecondsRealtime(1.5f);
                    SceneManager.LoadScene("Game Scene");
                }
            break;
            case Turn.Enemy:
                if (player.units.Count > 0) {
                    yield return StartCoroutine(messagePanel.DisplayMessage("PLAYER TURN"));
                    currentTurn = Turn.Player;
                    endTurnButton.enabled = true;
                    player.StartEndTurn(true);
                } else {
                    yield return StartCoroutine(messagePanel.DisplayMessage("PLAYER LOSES"));
                    yield return new WaitForSecondsRealtime(1.5f);
                    SceneManager.LoadScene("Game Scene");
                }
            break;
        }
    }

// public function for UI buttons
    public void EndTurn() 
    {
        StartCoroutine(SwitchTurns());
    }

    public void Win() 
    {

    }

    public void Lose() 
    {

    }
}
