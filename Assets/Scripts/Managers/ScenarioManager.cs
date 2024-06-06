using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

// Class that manages the game state during battles

public class ScenarioManager : MonoBehaviour {

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
    [HideInInspector] public FloorManager floorManager;
    [HideInInspector] public UIManager uiManager;
    [HideInInspector] public GameplayOptionalTooltips gpOptional;
    [HideInInspector] public Relics.RelicManager relicManager;
    public PathManager pathManager;
    public ObjectiveManager objectiveManager;
    public int startCavity;
    public EnemyManager currentEnemy;
    public PlayerManager player;

    [SerializeField] public MessagePanel messagePanel;
    public RunDataTracker runDataTracker;

// State machines
    public enum Scenario { Null, Combat, Boss, Provision, Barrier, EndState };
    public Scenario scenario;
    public enum Turn { Null, Player, Enemy, Descent, Cascade, Event, Loadout, }
    public Turn currentTurn, prevTurn;


    [SerializeField] bool queuing;


// RELIC PARAMS - DELETE
    [HideInInspector] public int tackleChance;


#region Initialization
    public IEnumerator Init(int startIndex = -1) {
        if (startIndex != -1) {
            startCavity = startIndex;
        }

        if (UIManager.instance)
            uiManager = UIManager.instance;     
        if (Relics.RelicManager.instance)
            relicManager = Relics.RelicManager.instance;

        if (FloorManager.instance) {
            floorManager = FloorManager.instance;
            yield return StartCoroutine(floorManager.Init(startCavity));
        }

        runDataTracker.Init(this);
    
        yield return StartCoroutine(player.Initialize());

        if (GameplayOptionalTooltips.instance) {
            gpOptional = GameplayOptionalTooltips.instance;
            gpOptional.Initialize();
        }
        
        tackleChance = 0;

        yield return null;

        if (startCavity != 0) {
            floorManager.StartCoroutine(floorManager.TransitionPackets());
            objectiveManager.ClearObjectives();
        }
        else 
            StartCoroutine(FirstTurn());
    }

    public IEnumerator FirstTurn(EnemyManager prevEnemy = null) {
        foreach (Unit u in player.units) {
            u.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = 0;
            //u.hitbox.enabled = false;
        }
        player.ToggleUnitSelectability(true);

        if (floorManager.floorSequence.activePacket.packetType == FloorChunk.PacketType.Tutorial) {
            foreach (GridElement ge in player.units)
                floorManager.currentFloor.RemoveElement(ge);
                
            StartCoroutine(floorManager.tutorial.Tutorial());
        } else {
            if (prevEnemy) {
                List<GridElement> unitsToDrop = new();
                for (int i = prevEnemy.units.Count - 1; i >= 0; i--)
                    unitsToDrop.Add(prevEnemy.units[i]);
                foreach (Unit u in player.units) {
                    if (u is not PlayerUnit && u is not Nail)
                        unitsToDrop.Add(u);
                }
                yield return StartCoroutine(floorManager.DescendUnits(unitsToDrop, prevEnemy, true));
            }
            player.units[3].grid = floorManager.currentFloor;
            yield return StartCoroutine(PlayerEnter());
            List<GridElement> units = new();
            foreach (Unit u in player.units) {
                if (u is PlayerUnit || u is Nail)
                    units.Add(u);
            }
            if (floorManager.floorSequence.activePacket.packetType == FloorChunk.PacketType.BOSS && !floorManager.floorSequence.activePacket.eliteSpawn)
                units.Remove(player.nail);
            
            yield return StartCoroutine(floorManager.DescendUnits(units));
            if (floorManager.floorSequence.activePacket.packetType == FloorChunk.PacketType.BOSS && !floorManager.floorSequence.activePacket.eliteSpawn) {
                yield return new WaitForSecondsRealtime(0.75f);
                yield return StartCoroutine(floorManager.SpawnBoss(floorManager.floorSequence.bossPrefab));
            } 
            StartCoroutine(SwitchTurns(Turn.Enemy));
        }
    }

    IEnumerator PlayerEnter() {
        uiManager.LockPeekButton(true);
        uiManager.ToggleEndTurnText(false);
        currentEnemy = (EnemyManager)floorManager.currentFloor.enemy;

        yield return StartCoroutine(SwitchTurns(Turn.Cascade));
        player.units[0].UpdateElement(new Vector2(3,3));
        player.units[1].UpdateElement(new Vector2(3,4));
        player.units[2].UpdateElement(new Vector2(3,5));
        player.nail.selectable = false;
        player.nail.grid.RemoveElement(player.nail);
        floorManager.previewManager.InitialPreview();
        
        foreach (GridElement ge in player.units) {
            if (floorManager.currentFloor.gridElements.Contains(ge))
                floorManager.currentFloor.RemoveElement(ge);
            
        }
        yield return StartCoroutine(floorManager.ChooseLandingPositions());
        yield return new WaitForSecondsRealtime(.5f);
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
        if (toTurn != Turn.Cascade)
            uiManager.ToggleEndTurnText(true);
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
        switch(toTurn) {
            default:
            case Turn.Null: break;
            case Turn.Enemy:
                currentTurn = Turn.Enemy;
                player.StartEndTurn(false);
                GridElement beacon = floorManager.currentFloor.gridElements.Find(ge => ge is Beacon);
                if (beacon != null) {
                    beacon.GetComponent<Beacon>().EnableSelection(false);
                }
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
                    currentTurn = Turn.Player;
                    uiManager.LockPeekButton(false);
                    uiManager.LockHUDButtons(false);
                    if (uiManager.gameObject.activeSelf)
                        yield return StartCoroutine(messagePanel.PlayMessage(MessagePanel.Message.Slag));


                    player.StartEndTurn(true);
                    GridElement be = floorManager.currentFloor.gridElements.Find(ge => ge is Beacon);
                    if (be != null) {
                        be.GetComponent<Beacon>().EnableSelection(true);
                    }
                } else {
                    yield return StartCoroutine(Lose());
                }
            break;
// Is not in control, this is called only to display message, coroutine continued in FloorManager
            case Turn.Descent:
                floorManager.previewManager.TogglePreivews(false);
                uiManager.LockPeekButton(true);
                
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
                // if (uiManager.gameObject.activeSelf) {
                //     if (floorManager.floorCount == floorManager.floorSequence.activePacket.packetLength)
                //         yield return StartCoroutine(messagePanel.PlayMessage(MessagePanel.Message.FinalDescent));
                //     else
                //         yield return StartCoroutine(messagePanel.PlayMessage(MessagePanel.Message.Descent));
                // }
            break;
            case Turn.Cascade:
                currentTurn = Turn.Cascade;
                player.currentGrid = floorManager.currentFloor;
                floorManager.previewManager.alignmentFloor = floorManager.currentFloor;
                player.StartEndTurn(true, true);
                foreach (Unit u in player.units) {
                    if (u is PlayerUnit) {
                        u.energyCurrent = 0;
                        u.elementCanvas.UpdateStatsDisplay();
                        u.ui.UpdateEquipmentButtons();
                        u.ui.overview.UpdateOverview();
                        u.StoreInGrid(player.currentGrid);
                    }
                }
                uiManager.LockHUDButtons(false);
                uiManager.LockPeekButton(true);
                yield return new WaitForSecondsRealtime(0.2f);
                if (uiManager.gameObject.activeSelf)
                    yield return StartCoroutine(messagePanel.PlayMessage(MessagePanel.Message.Position));
                floorManager.currentFloor.LockGrid(false);
            break;
        }
    }

// public function for UI buttons
    public void EndTurn() {
        EndTurnEvent evt = ObjectiveEvents.EndTurnEvent;
        evt.toTurn = Turn.Enemy;
        ObjectiveEventManager.Broadcast(evt);

        player.overrideEquipment = null;
        if (floorManager.floorSequence.currentThreshold == FloorChunk.PacketType.Tutorial)
            floorManager.tutorial.SwitchTurns();
        else
            StartCoroutine(SwitchTurns());
    }

    public void RewardOnKill(GridElement ge) {
        relicManager.QueueRelicReward(ge);
    }

    public IEnumerator FinalDrop() {
        yield return new WaitForSecondsRealtime(1f);
        yield return StartCoroutine(floorManager.FinalDescent());
    }

    public IEnumerator Win() {
        scenario = Scenario.EndState;
        yield return StartCoroutine(gpOptional.WhatsAhead());
        if (uiManager.gameObject.activeSelf) {
            uiManager.ToggleBattleCanvas(false);
            yield return StartCoroutine(messagePanel.PlayMessage(MessagePanel.Message.Win));
        }
        objectiveManager.ClearObjectives();
        relicManager.ClearRelics();
        yield return new WaitForSecondsRealtime(1.25f);
        StartCoroutine(runDataTracker.UpdateAndDisplay(true, floorManager.floors.Count - 1, player.defeatedEnemies, relicManager.scrapValue));
    }

    public IEnumerator Lose() {
        scenario = Scenario.EndState;
        if (currentTurn == Turn.Enemy)
            currentEnemy.StopActingUnit();
        else if (currentTurn == Turn.Player)
            player.StartEndTurn(false);
        if (uiManager.gameObject.activeSelf) {
            uiManager.ToggleBattleCanvas(false);
            pathManager.gameObject.SetActive(false);
            player.upgradeManager.gameObject.SetActive(false);
            gpOptional.gameObject.SetActive(false);
            if (floorManager.tutorial)
                floorManager.tutorial.gameObject.SetActive(false);

            yield return StartCoroutine(messagePanel.PlayMessage(MessagePanel.Message.Lose));
        }
        yield return StartCoroutine(player.RetrieveNailAnimation());
        objectiveManager.ClearObjectives();
        relicManager.ClearRelics();
        StartCoroutine(runDataTracker.UpdateAndDisplay(false, floorManager.floors.Count - 2 >= 0 ? floorManager.floors.Count - 2 : 0, player.defeatedEnemies,  relicManager.scrapValue));
    }

}
