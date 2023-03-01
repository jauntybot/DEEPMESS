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
    [SerializeField] string resetSceneString;
    public EnemyManager currentEnemy;
    public PlayerManager player;
    public Button endTurnButton;

    [SerializeField] MessagePanel messagePanel;

// State machines
    public enum GameState { Null, Setup, PlayerPlace, Battle, End }
    public GameState gameState;
    public enum Turn { Null, Player, Enemy, Descent }
    public Turn currentTurn;
    public int turnCount, turnsToDescend;

#region Initialization
    public IEnumerator Start() 
    {
        if (FloorManager.instance) 
        {
            floorManager = FloorManager.instance;
            
            yield return StartCoroutine(floorManager.GenerateFloor(true)); 
            currentEnemy = (EnemyManager)floorManager.currentFloor.enemy;
            player.transform.parent = floorManager.currentFloor.transform;
        }
        yield return StartCoroutine(player.Initialize());
        yield return StartCoroutine(player.DropNail());

        yield return new WaitForSeconds(.75f);
        yield return StartCoroutine(floorManager.TransitionFloors(floorManager.currentFloor.gameObject));

        if (floorManager) yield return StartCoroutine(floorManager.GenerateFloor(true));

        yield return new WaitForSeconds(0.75f);
        StartCoroutine(floorManager.PreviewFloor(false, false));
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
                if (turnCount >= turnsToDescend) {
                    floorManager.Descend();
                } else {
                    yield return StartCoroutine(messagePanel.DisplayMessage("ENEMY TURN"));
                    currentTurn = Turn.Enemy;
                    foreach(Unit u in currentEnemy.units) {
                        u.energyCurrent = u.energyMax;
                        u.elementCanvas.UpdateStatsDisplay();
                    }
                    endTurnButton.enabled = false;
                    StartCoroutine(currentEnemy.TakeTurn());
                }
            break;
            case Turn.Enemy:
                if (player.units.Count >= 2) {
                    turnCount++;
                    yield return StartCoroutine(messagePanel.DisplayMessage("PLAYER TURN"));
                    if (turnsToDescend - turnCount > 0)
                        yield return StartCoroutine(messagePanel.DisplayMessage(turnsToDescend - turnCount + 1 + " TURNS UNTIL DESCENT"));
                    else
                        yield return StartCoroutine(messagePanel.DisplayMessage("FINAL TURN UNTIL DESCENT"));
                    currentTurn = Turn.Player;
                    endTurnButton.enabled = true;
                    player.StartEndTurn(true);
                } else {
                    yield return StartCoroutine(Lose());
                }
            break;
            case Turn.Descent:
                turnCount = 0;
                player.StartEndTurn(false);
                currentTurn = Turn.Descent;
                endTurnButton.enabled = false;
                yield return StartCoroutine(messagePanel.DisplayMessage("DESCENDING"));
            break;
        }
    }

// public function for UI buttons
    public void EndTurn() 
    {
        StartCoroutine(SwitchTurns());
    }

    public IEnumerator Win() 
    {
        yield return StartCoroutine(messagePanel.DisplayMessage("PLAYER WINS"));
        yield return new WaitForSecondsRealtime(1.5f);
        SceneManager.LoadScene(resetSceneString);
    }

    public IEnumerator Lose() 
    {
        if (currentTurn == Turn.Enemy)
            currentEnemy.EndTurnEarly();
        yield return StartCoroutine(messagePanel.DisplayMessage("PLAYER LOSES"));
        yield return new WaitForSecondsRealtime(1.5f);
        SceneManager.LoadScene(resetSceneString);
    }

    public void UpdateUnitUI(Unit u) {
        
    }
}
