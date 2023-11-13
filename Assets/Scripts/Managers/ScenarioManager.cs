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
    [HideInInspector] public UIManager uiManager;
    [HideInInspector] public GameplayOptionalTooltips gpOptional;
    [HideInInspector] public FloorManager floorManager;
    [SerializeField] string resetSceneString;
    public TutorialSequence tutorial;
    public int startCavity;
    public EnemyManager currentEnemy;
    public PlayerManager player;

    [SerializeField] public MessagePanel messagePanel;
    public RunDataTracker runDataTracker;

// State machines
    public enum Scenario { Null, Combat, Boss, Provision, Barrier, EndState };
    public Scenario scenario;
    public enum Turn { Null, Player, Enemy, Descent, Cascade, Loadout, Slots }
    public Turn currentTurn, prevTurn;



#region Initialization
    public IEnumerator Init(int startIndex = -1) {
        if (startIndex != -1) {
            startCavity = startIndex;
            Debug.Log("start cavity override");
        }
        Debug.Log(startCavity);

        if (UIManager.instance)
            uiManager = UIManager.instance;

        if (FloorManager.instance) {
            floorManager = FloorManager.instance;
            yield return StartCoroutine(floorManager.Init(startCavity));
            if (startCavity == 0) {
                tutorial.gameObject.SetActive(true);
                tutorial.Initialize(this);
                floorManager.previewManager.tut = true;
            } else {
                tutorial.gameObject.SetActive(false);
                floorManager.GenerateFloor(null, true); 
                floorManager.GenerateFloor();
            }

            currentEnemy = (EnemyManager)floorManager.currentFloor.enemy;
            player.transform.parent = floorManager.currentFloor.transform;
        }
        resetSceneString = SceneManager.GetActiveScene().name;
        runDataTracker.Init(this);
    
        yield return StartCoroutine(player.Initialize(floorManager.currentFloor));
        if (startCavity >= 2) 
            player.units[0].ui.UpdateLoadout(floorManager.cavityEquip[0]);      
        if (startCavity >= 3) 
            player.units[1].ui.UpdateLoadout(floorManager.cavityEquip[1]);

        if (GameplayOptionalTooltips.instance) {
            gpOptional = GameplayOptionalTooltips.instance;
            gpOptional.Initialize();
        }

        StartCoroutine(FirstTurn());
    }

    public IEnumerator FirstTurn() {
        foreach (Unit u in player.units) {
            u.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = 0;
            u.hitbox.enabled = false;
        }
        if (startCavity == 0) {
                foreach (GridElement ge in player.units)
                    floorManager.currentFloor.RemoveElement(ge);
                    
                StartCoroutine(tutorial.Tutorial());
        } else 
            yield return StartCoroutine(PlayerEnter());
    }

    IEnumerator PlayerEnter() {
        uiManager.LockFloorButtons(true);
        //floorManager.previewManager.InitialPreview();
        
        foreach (GridElement ge in player.units) {
            floorManager.currentFloor.RemoveElement(ge);
        }
        // yield return StartCoroutine(floorManager.ChooseLandingPositions());
        // yield return new WaitForSecondsRealtime(1.25f);
        
        yield return StartCoroutine(SwitchTurns(Turn.Descent, Scenario.Combat));
        yield return StartCoroutine(floorManager.DescendUnits(new List<GridElement>{ player.units[0], player.units[1], player.units[2], player.units[3]} ));
        StartCoroutine(SwitchTurns(Turn.Enemy));
    }

#endregion

// Overload allows you to specify which turn to switch to, otherwise inverts the binary
    public IEnumerator SwitchTurns(Turn toTurn = default, Scenario toScenario = default) {

        foreach(GridElement ge in floorManager.currentFloor.gridElements) 
                ge.TargetElement(false);
        if (toTurn == default) {
            switch (currentTurn) {
                default: toTurn = Turn.Player; break;
                case Turn.Player: toTurn = Turn.Enemy; break;
                case Turn.Enemy: toTurn = Turn.Player; break;
                case Turn.Descent: toTurn = prevTurn; break;
                case Turn.Cascade: toTurn = Turn.Descent; break;
            }
        }
        prevTurn = currentTurn;
        uiManager.LockHUDButtons(true);
// Scenario state machine (more optional)        
        switch(toScenario) {
            default:
            case Scenario.Null: break;
            case Scenario.Combat:
                scenario = toScenario;
            break;
            case Scenario.Boss:
                scenario = toScenario;
            break;
            case Scenario.Provision:
            
            break;
            case Scenario.Barrier:
            
            break;
            case Scenario.EndState:
                scenario = toScenario;
            break;
        }
// Turn state machine
        switch(toTurn) 
        {
            default:
            case Turn.Null: break;
            case Turn.Enemy:
                currentTurn = Turn.Enemy;
                player.StartEndTurn(false);
                yield return new WaitForSecondsRealtime(0.625f);

                if (prevTurn == Turn.Descent)
                    StartCoroutine(currentEnemy.TakeTurn(true));
                else {
                    if (uiManager.gameObject.activeSelf)
                        yield return StartCoroutine(messagePanel.PlayMessage(MessagePanel.Message.Antibody));
                    StartCoroutine(currentEnemy.TakeTurn(false));
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
                if (!lose && !player.nail.conditions.Contains(Unit.Status.Disabled)) {
                    uiManager.LockFloorButtons(false);
                    if (uiManager.gameObject.activeSelf)
                        yield return StartCoroutine(messagePanel.PlayMessage(MessagePanel.Message.Slag));

                    currentTurn = Turn.Player;
                    uiManager.LockHUDButtons(false);

                    player.StartEndTurn(true);
                } else {
                    yield return StartCoroutine(Lose());
                }
            break;
// Is not in control, this is called only to display message, coroutine continued in FloorManager
            case Turn.Descent:
                floorManager.previewManager.TogglePreivews(false);
                
                currentEnemy.InterruptReinforcements();
                if (prevTurn == Turn.Cascade) {
                    //player.currentGrid = floorManager.floors[player.currentGrid.index-1];
                    for (int i = player.units.Count - 1; i >= 0; i--) {
                        if (player.units[i] is not Nail) {
                            floorManager.currentFloor.RemoveElement(player.units[i]);
                        }
                    }
                }
                currentTurn = Turn.Descent;
                player.StartEndTurn(false);
// Reset per floor equipment on PlayerUnits
                foreach(Unit u in player.units)
                    u.usedEquip = false;
                if (uiManager.gameObject.activeSelf)
                    yield return StartCoroutine(messagePanel.PlayMessage(MessagePanel.Message.Descent));
            break;
            case Turn.Cascade:
                currentTurn = Turn.Cascade;
                player.currentGrid = floorManager.currentFloor;
                floorManager.previewManager.alignmentFloor = floorManager.currentFloor;
                floorManager.currentFloor.LockGrid(false);
                player.StartEndTurn(true, true);
                foreach(Unit u in player.units)
                    u.hitbox.enabled = false;
                foreach (Unit u in player.units) {
                    if (u is not Nail) {
                        u.energyCurrent = 0;
                        u.usedEquip = true;
                        u.elementCanvas.UpdateStatsDisplay();
                        u.ui.UpdateEquipmentButtons();
                        u.StoreInGrid(player.currentGrid);
                    }
                }
                uiManager.LockHUDButtons(false);
                uiManager.LockFloorButtons(true);
                yield return new WaitForSecondsRealtime(0.2f);
                if (uiManager.gameObject.activeSelf)
                    yield return StartCoroutine(messagePanel.PlayMessage(MessagePanel.Message.Position));
            break;
        }
    }

// public function for UI buttons
    public void EndTurn() 
    {
        if (floorManager.floorSequence.currentThreshold == FloorPacket.PacketType.Tutorial)
            tutorial.SwitchTurns();
        else
            StartCoroutine(SwitchTurns());
    }

    public IEnumerator Win() 
    {
        if (uiManager.gameObject.activeSelf) {
            yield return StartCoroutine(messagePanel.PlayMessage(MessagePanel.Message.Win));
            uiManager.ToggleBattleCanvas(false);
        }
        yield return new WaitForSecondsRealtime(1.5f);
        runDataTracker.UpdateAndDisplay(true, floorManager.currentFloor.index + 1, player.defeatedEnemies);
    }

    public IEnumerator Lose() 
    {
        scenario = Scenario.EndState;
        if (currentTurn == Turn.Enemy)
            currentEnemy.EndTurnEarly();
        else if (currentTurn == Turn.Player)
            player.StartEndTurn(false);
        if (uiManager.gameObject.activeSelf) {
            yield return StartCoroutine(messagePanel.PlayMessage(MessagePanel.Message.Lose));
            uiManager.ToggleBattleCanvas(false);
        }
        yield return StartCoroutine(player.RetrieveNailAnimation());

        runDataTracker.UpdateAndDisplay(false, floorManager.currentFloor.index + 1, player.defeatedEnemies);
    }
}
