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
    public enum Turn { Null, Player, Enemy, Descent, Cascade, Loadout, Slots }
    public Turn currentTurn, prevTurn;



#region Initialization
    public IEnumerator Start() 
    {
        if (FloorManager.instance) 
        {
            floorManager = FloorManager.instance;
            
            yield return StartCoroutine(floorManager.GenerateFloor()); 
            currentEnemy = (EnemyManager)floorManager.currentFloor.enemy;
            player.transform.parent = floorManager.currentFloor.transform;
        }
        resetSceneString = SceneManager.GetActiveScene().name;
        
        yield return StartCoroutine(player.Initialize());
    }

    public void InitialDescent() {
        StartCoroutine(FirstTurn());
    }

    public IEnumerator FirstTurn() {
        yield return StartCoroutine(SwitchTurns(Turn.Descent));

        yield return new WaitForSeconds(.75f);
        yield return StartCoroutine(floorManager.TransitionFloors(floorManager.currentFloor.gameObject, false));

        if (floorManager) yield return StartCoroutine(floorManager.GenerateFloor());

        yield return new WaitForSeconds(0.75f);
        StartCoroutine(floorManager.PreviewFloor(false, false));
        yield return new WaitForSeconds(1);

        currentTurn = Turn.Enemy;
        StartCoroutine(SwitchTurns(Turn.Player));
        floorManager.currentFloor.LockGrid(false);
    }

#endregion

// Overload allows you to specify which turn to switch to, otherwise inverts the binary
    public IEnumerator SwitchTurns(Turn toTurn = default) 
    {
        if (toTurn == default) {
            switch (currentTurn) {
                default: toTurn = Turn.Player; break;
                case Turn.Player: toTurn = Turn.Enemy; break;
                case Turn.Enemy: toTurn = Turn.Player; break;
                case Turn.Descent: toTurn = prevTurn; break;
                case Turn.Cascade: toTurn = Turn.Descent; break;
            }
        }
        floorManager.upButton.GetComponent<Button>().enabled = false; floorManager.downButton.GetComponent<Button>().enabled = false;
        prevTurn = currentTurn;
        switch(toTurn) 
        {
            case Turn.Enemy:
                if (currentEnemy.units.Count > 0) {
                    currentTurn = Turn.Enemy;
                    player.StartEndTurn(false);
                    yield return StartCoroutine(messagePanel.DisplayMessage("ANTIBODY RESPONSE", 2));

                    endTurnButton.enabled = false;
                    
                    if (prevTurn == Turn.Descent)
                        StartCoroutine(currentEnemy.TakeTurn(true));
                    else
                        StartCoroutine(currentEnemy.TakeTurn(false));
                }
                else if (currentEnemy.units.Count <= 0) {
                   floorManager.Descend(prevTurn == Turn.Descent);
                   Debug.Log("Empty floor descent");
                }
            break;
            case Turn.Player:
                //currentEnemy.ResolveConditions();
                bool lose = true;
                foreach (Unit u in player.units) {
                    if (u is not Nail && !u.conditions.Contains(Unit.Status.Disabled)) {
                        lose = false;
                        break;
                    }
                }
                if (!lose && player.units.Find(u => u is Nail) != null) {
                    floorManager.upButton.GetComponent<Button>().enabled = true; floorManager.downButton.GetComponent<Button>().enabled = true;
                    yield return StartCoroutine(messagePanel.DisplayMessage("PLAYER TURN", 1));

                    currentTurn = Turn.Player;
                    endTurnButton.enabled = true;

                    player.StartEndTurn(true);
                } else {
                    yield return StartCoroutine(Lose());
                }
            break;
            case Turn.Descent:
                yield return StartCoroutine(floorManager.CancelPreview());
                if (prevTurn == Turn.Cascade) {
                    player.currentGrid = floorManager.floors[player.currentGrid.index-1];
                    for (int i = player.units.Count - 1; i >= 0; i--) {
                        if (player.units[i] is not Nail) {
                            floorManager.currentFloor.RemoveElement(player.units[i]);
                        }
                    }
                }
                currentTurn = Turn.Descent;
                player.StartEndTurn(false);
                foreach(Unit u in player.units)
                    u.usedEquip = false;
                endTurnButton.enabled = false;
                yield return StartCoroutine(messagePanel.DisplayMessage("DESCENDING", 0));
            break;
            case Turn.Cascade:
                currentTurn = Turn.Cascade;
                player.currentGrid = floorManager.currentFloor;
                player.StartEndTurn(true, true);
                foreach (Unit u in player.units) {
                    if (u is not Nail) {
                        u.energyCurrent = 0;
                        u.usedEquip = true;
                        u.elementCanvas.UpdateStatsDisplay();
                        u.ui.UpdateEquipmentButtons();
                        u.StoreInGrid(player.currentGrid);
                    }
                }
                endTurnButton.enabled = true;
                yield return StartCoroutine(messagePanel.DisplayMessage("REPOSITION UNITS", 1));
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
        yield return StartCoroutine(messagePanel.DisplayMessage("PLAYER WINS", 1));
        yield return new WaitForSecondsRealtime(1.5f);
        SceneManager.LoadScene(resetSceneString);
    }

    public IEnumerator Lose() 
    {
        if (currentTurn == Turn.Enemy)
            currentEnemy.EndTurnEarly();
        yield return StartCoroutine(messagePanel.DisplayMessage("PLAYER LOSES", 2));
        yield return new WaitForSecondsRealtime(1.5f);
        SceneManager.LoadScene(resetSceneString);
    }
}
