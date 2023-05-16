using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialSequence : MonoBehaviour
{

    #region SINGLETON (and Awake)
    public static TutorialSequence instance;
    private void Awake() {
        if (TutorialSequence.instance) {
            Debug.Log("Warning! More than one instance of TutorialSequence found!");
            return;
        }
        TutorialSequence.instance = this;
    }

    #endregion

    [SerializeField] ScenarioManager scenario;
    [SerializeField] TutorialEnemyManager currentEnemy;
    public List<FloorDefinition> scriptedFloors;
    public GameObject tutorialFloorPrefab, tutorialEnemyPrefab;
    public Tooltip tooltip, wsTooltip;
    public Animator screenFade;
    [HideInInspector] public string header, body;
    public bool blinking = false;
    int coStep = 0;
    public bool skip;


    public void Initialize(ScenarioManager manager) {
        scenario = manager;
        GetComponent<LaterTutorials>().StartListening(scenario.player);
    }

    public IEnumerator Tutorial() {

        yield return new WaitForSecondsRealtime(0.15f);
        yield return StartCoroutine(SplashMessage());
        yield return new WaitForSecondsRealtime(0.15f);
        yield return scenario.StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Player));
        
        UIManager.instance.LockHUDButtons(true);
        scenario.player.units[1].selectable = false;
        scenario.player.units[2].selectable = false;
        scenario.player.units[3].selectable = false;
        scenario.player.units[0].ui.equipment[0].GetComponent<Button>().enabled = false;
        scenario.player.units[0].ui.equipment[1].GetComponent<Button>().enabled = false;
        scenario.player.units[1].ui.equipment[0].GetComponent<Button>().enabled = false;
        scenario.player.units[2].ui.equipment[0].GetComponent<Button>().enabled = false;

// PLAYER FIRST TURN
        yield return StartCoroutine(Select01());
        yield return new WaitForSecondsRealtime(0.15f);
        yield return StartCoroutine(Move01());
        yield return new WaitForSecondsRealtime(0.15f);
        yield return StartCoroutine(Equip01());
        yield return new WaitForSecondsRealtime(0.15f);
        yield return StartCoroutine(Select02());
        yield return new WaitForSecondsRealtime(0.15f);
        yield return StartCoroutine(Message01());
        yield return new WaitForSecondsRealtime(0.15f);
        yield return StartCoroutine(Select03());
        yield return new WaitForSecondsRealtime(0.15f);
        yield return StartCoroutine(Message02());
        yield return new WaitForSecondsRealtime(0.15f);
        yield return StartCoroutine(Select04());
        yield return new WaitForSecondsRealtime(0.15f);
// ENEMY FIRST TURN
        yield return StartCoroutine(Enemy01());
        yield return new WaitForSecondsRealtime(0.15f);
        

// PLAYER SECOND TURN
        yield return StartCoroutine(Descent01());
        yield return new WaitForSecondsRealtime(0.15f);

// ENEMY SECOND TURN
        yield return StartCoroutine(Enemy02());
        yield return new WaitForSecondsRealtime(0.15f);
        
        UIManager.instance.LockHUDButtons(true);
        scenario.player.units[1].selectable = false;
        scenario.player.units[2].selectable = false;
        scenario.player.units[3].selectable = false;

// PLAYER THIRD TURN
        yield return StartCoroutine(Equip02());
        yield return new WaitForSecondsRealtime(0.15f);
        yield return StartCoroutine(Descent02());
        yield return new WaitForSecondsRealtime(0.15f);

        yield return StartCoroutine(Message03());
        yield return new WaitForSecondsRealtime(0.15f);
    }

    public IEnumerator BlinkTile(Vector2 coord, bool move = true) {
        blinking = true;
        GridSquare sqr = FloorManager.instance.currentFloor.sqrs.Find(sqr => sqr.coord == coord);
        float timer = 0;
        int i = 0;
        sqr.ToggleValidCoord(true, move ? FloorManager.instance.equipmentColor : FloorManager.instance.playerColor);
        while (blinking) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            timer += Time.deltaTime;
            if (timer > 0.5f) {
                sqr.ToggleValidCoord(true, i%2==0 ? FloorManager.instance.playerColor : FloorManager.instance.equipmentColor);
                i++; timer = 0;
            }
        }
        if (scenario.player.selectedUnit) {
            if (scenario.player.selectedUnit.validActionCoords.Count > 0)
                scenario.player.currentGrid.DisplayValidCoords(scenario.player.selectedUnit.validActionCoords);
        }
        sqr.ToggleValidCoord(false);
    }

    public IEnumerator SplashMessage() {
        screenFade.gameObject.SetActive(true);
        currentEnemy = (TutorialEnemyManager)scenario.player.currentGrid.enemy;

        header = "WELCOME";
        body = "to DEEPMESS. Here's a brief summary of what you'll be doing in the game." + '\n' + '\n' + "SUMMARY";
        tooltip.SetText(new Vector3(100,0,0), body, header);
        tooltip.transform.GetChild(0).gameObject.SetActive(true);

        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator Select01() {
        header = "";
        body = "Select FLAT to move it in line with an ENEMY.";
        tooltip.SetText(new Vector2(280, -50), body);
        StartCoroutine(BlinkTile(new Vector2(4,1)));

        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (scenario.player.selectedUnit) break;
        }
        blinking = false;
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator Move01() {
        scenario.player.currentGrid.LockGrid(false);
        body = "Move FLAT here by selecting the highlighted tile.";
        tooltip.SetText(new Vector2(370,-200), body);
        StartCoroutine(BlinkTile(new Vector2(6,1)));

        while (true) {
            if (scenario.player.selectedUnit)
                scenario.player.selectedUnit.validActionCoords = new List<Vector2> {new Vector2(6,1)};
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (scenario.player.unitActing) break;
        }
        blinking = false;
        while (scenario.player.unitActing)
            yield return new WaitForSecondsRealtime(1/Util.fps);
        UIManager.instance.LockHUDButtons(true);
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator Equip01() {
        scenario.player.units[0].ui.equipment[1].GetComponent<Button>().enabled = true;
        body = "These are FLAT's equipment. Equip the HAMMER by clicking its button.";
        tooltip.SetText(new Vector2(-300, -125), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (scenario.player.selectedUnit) {
                if (scenario.player.selectedUnit.selectedEquipment) break;
            }
        }
        body = "Now, target the ENEMY in range of FLAT's selected equipment.";
        tooltip.SetText(new Vector2(680, -100), body);

        StartCoroutine(BlinkTile(new Vector2(6,4)));
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (scenario.player.selectedUnit) {
                if (scenario.player.selectedUnit.selectedEquipment) {
                    if (scenario.player.selectedUnit.selectedEquipment.firstTarget) break;
                }
            } else {
                // PLAYER DESELECTED UNIT
            }
        }
        blinking = false;
        yield return new WaitForSecondsRealtime(1/Util.fps);
        UIManager.instance.LockHUDButtons(true);
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }


    public IEnumerator Select02() {
        body = "Now select a slag for the HAMMER to bounce to." + '\n' + "Let's send it to PONY.";
        tooltip.SetText(new Vector2(-90, -50), body);
        StartCoroutine(BlinkTile(new Vector2(3,3)));

        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (scenario.player.selectedUnit) {
                if (scenario.player.selectedUnit.selectedEquipment) {
                    if (scenario.player.selectedUnit.selectedEquipment.firstTarget)
                        scenario.player.selectedUnit.validActionCoords = new List<Vector2>{ new Vector2(3,3) };
                }
            }
            if (scenario.player.unitActing) break;
        }
        blinking = false;
        while (scenario.player.unitActing)
            yield return new WaitForSecondsRealtime(1/Util.fps);
        UIManager.instance.LockHUDButtons(true);
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator Message01() {
        while (scenario.player.unitActing)
            yield return new WaitForSecondsRealtime(1/Util.fps);
        scenario.player.units[0].selectable = false;
        yield return new WaitForSecondsRealtime(0.25f);
        UIManager.instance.LockHUDButtons(true);

        body = "Great. That knocked off the enemy's shield. We'll need to attack it again to kill it for good.";
        tooltip.SetText(new Vector2(680, -100), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        body = "The HAMMER can be passed between slag units or it can bounce back to the unit that threw it.";
        tooltip.SetText(new Vector2(440, 110), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator Select03() {
        scenario.player.units[2].selectable = true;

        body = "MOVE PONY in range of the ENEMY, attack it again with the HAMMER, then bounce it back to PONY.";
        tooltip.SetText(new Vector2(140, 320), body);
        StartCoroutine(BlinkTile(new Vector2(3,3)));
        while (!scenario.player.selectedUnit) yield return null;
        blinking = false;
        yield return new WaitForSecondsRealtime(1/Util.fps);
        StartCoroutine(BlinkTile(new Vector2(3,4)));
        while (!scenario.player.units[2].moved) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
            if (scenario.player.selectedUnit) 
                scenario.player.selectedUnit.validActionCoords = new List<Vector2>{new Vector2(3,4)};
        }
        blinking = false;
        while (scenario.player.unitActing)
            yield return new WaitForSecondsRealtime(1/Util.fps);
        UIManager.instance.LockHUDButtons(true);
        StartCoroutine(BlinkTile(new Vector2(6,4)));
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (scenario.player.units[2].energyCurrent > 0) {
                if (scenario.player.units[2].selectedEquipment)  break;
            }
        }
        while (scenario.player.units[2].energyCurrent > 0) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (scenario.player.units[2].selectedEquipment) {
                if (scenario.player.units[2].selectedEquipment.firstTarget) break;
            }
            
        }
        blinking = false;
        yield return new WaitForSecondsRealtime(1/Util.fps);
        StartCoroutine(BlinkTile(new Vector2(3,4)));
        while (scenario.player.units[2].energyCurrent > 0) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
                if (scenario.player.units[2].selectedEquipment) {
                    if (scenario.player.units[2].selectedEquipment.firstTarget)
                        scenario.player.units[2].validActionCoords = new List<Vector2>{ new Vector2(3,4) };
                }
        }
        blinking = false;
        while (scenario.player.unitActing)
            yield return new WaitForSecondsRealtime(1/Util.fps);
        UIManager.instance.LockHUDButtons(true);
        
        scenario.player.units[2].selectable = false;
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator Message02() {
        while (scenario.player.unitActing)
            yield return new WaitForSecondsRealtime(1/Util.fps);

        body = "That ENEMY is taken care of. There's still an ENEMY on the floor, but the HAMMER can't be thrown again because PONY already acted.";
        tooltip.SetText(new Vector2(680, -100), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        body = "SPIKE can still act, let's see what it can do.";
        tooltip.SetText(new Vector2(210, 225), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        UIManager.instance.LockFloorButtons(false);
        body = "Units will descend to the next floor crushing anything below." + '\n' + "Select PEEK to look at the next floor.";
        tooltip.SetText(new Vector2(650, -200), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (FloorManager.instance.transitioning) break;
        }
        while (FloorManager.instance.transitioning)
            yield return new WaitForSecondsRealtime(1/Util.fps);
        UIManager.instance.LockFloorButtons(true);
        body = "This is a preview of the next floor. You can see where enemies and hazards are located.";
        tooltip.SetText(new Vector2(360,-160), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        body = "You also see where the units above will land after a descent.";
        tooltip.SetText(new Vector2(-70, 0), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        UIManager.instance.LockFloorButtons(false);
        body = "Let's use SPIKE's equipment to crush an enemy below." + '\n' + "Use the PEEK button again to return to the top floor.";
        tooltip.SetText(new Vector2(650, -200), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (FloorManager.instance.transitioning) break;
        }
        while (FloorManager.instance.transitioning) yield return null;
        yield return new WaitForSecondsRealtime(1/Util.fps);

        scenario.player.units[0].selectable = false;
        scenario.player.units[1].selectable = false;
        scenario.player.units[2].selectable = false;
        scenario.player.units[2].ui.equipment[1].enabled = false;
        scenario.player.units[3].selectable = false;

        UIManager.instance.LockHUDButtons(true);
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator Select04() {
        scenario.player.units[1].selectable = true;
        scenario.player.units[1].ui.equipment[0].GetComponent<Button>().enabled = true;
        UIManager.instance.LockFloorButtons(false);
        body = "Move Spike into position over an ENEMY. Use the PEEK button to line them up.";
        StartCoroutine(BlinkTile(new Vector2(1,2)));
        tooltip.SetText(new Vector2(80, 90), body);
        while (!scenario.player.selectedUnit)
            yield return new WaitForSecondsRealtime(1/Util.fps);
        blinking = false;
        yield return new WaitForSecondsRealtime(1/Util.fps);
        StartCoroutine(BlinkTile(new Vector2(1,4)));
        while (!scenario.player.units[1].moved) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (scenario.player.selectedUnit)
                scenario.player.selectedUnit.validActionCoords = new List<Vector2>{ new Vector2(1, 4) };
        }
        blinking = false;
        while (scenario.player.unitActing)
            yield return new WaitForSecondsRealtime(1/Util.fps);
        UIManager.instance.LockHUDButtons(true);
        body = "Select the ANVIL.";
        tooltip.SetText(new Vector2(-440, -250), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (scenario.player.selectedUnit) {
                if (scenario.player.selectedUnit.selectedEquipment) break;
            }
        }

        body = "This equipment allows SPIKE to MOVE again, leaving an ANVIL on the previous tile.";
        tooltip.SetText(new Vector2(50, 450), body);
        StartCoroutine(BlinkTile(new Vector2(1,5)));
        while (scenario.player.units[1].energyCurrent > 0) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (scenario.player.selectedUnit) {
                if (scenario.player.selectedUnit.selectedEquipment) {
                    scenario.player.units[1].validActionCoords = new List<Vector2> { new Vector2(1, 5) };
                    
                }
            }
        }
        blinking = false;
        while (scenario.player.unitActing)
            yield return new WaitForSecondsRealtime(1/Util.fps);
            scenario.player.units[1].selectable = false;
        scenario.player.units[1].ui.equipment[0].GetComponent<Button>().enabled = false;
        UIManager.instance.LockFloorButtons(true);
        body = "Now that we set this trap we're out of actions. Press END TURN and ENEMIES will respond.";
        tooltip.SetText(new Vector2(650, -225), body);
        while (scenario.currentTurn == ScenarioManager.Turn.Player)
            yield return new WaitForSecondsRealtime(1/Util.fps);       
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator Enemy01() {
        while (scenario.currentTurn != ScenarioManager.Turn.Enemy) yield return null;
        yield return new WaitForSecondsRealtime(0.5f);

        body = "ENEMIES can move 2 tiles and then attack an adjacent tile for 1 damage.";
        tooltip.SetText(new Vector2(480, -90), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
        yield return StartCoroutine(currentEnemy.MoveInOrder(new List<Vector2>{ new Vector2(3,6) }, new List<Vector2>{ new Vector2(2,6) }));
        body = "Ouch! This ENEMY attakcked me! But I'm not helpless, I deal 1 damage back to attackers.";
        tooltip.SetText(new Vector2(400, 90), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        body = "Be careful, though. If I die, or all three slag units, the run is failed.";
        tooltip.SetText(new Vector2(400, 90), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        currentEnemy.EndTurn();
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator Descent01() {
        while (scenario.currentTurn != ScenarioManager.Turn.Player)
            yield return new WaitForSecondsRealtime(1/Util.fps);
        yield return new WaitForSecondsRealtime(0.125f);

        scenario.player.units[0].selectable = false;
        scenario.player.units[1].selectable = false;
        scenario.player.units[2].selectable = false;
        scenario.player.units[3].selectable = false;
        UIManager.instance.LockFloorButtons(true);

        body = "Our turn again. Let's descend to take advantage of our ANVIL trap.";
        tooltip.SetText(new Vector2(20, 120), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        
        body = "Descend to the next floor by striking me with the HAMMER, or by eliminating all ENEMIES on the current floor.";
        tooltip.SetText(new Vector2(265, 0), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
         
        UIManager.instance.LockFloorButtons(false);
        body = "Before descending, check to make sure your slag are in safe positions." + '\n' + "PEEK down to the next floor";
        tooltip.SetText(new Vector2(650, -200), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (FloorManager.instance.transitioning) break;
        }
        while (FloorManager.instance.transitioning) yield return null;
        yield return new WaitForSecondsRealtime(1/Util.fps);
        body = "FLAT would land on a wall if you descend now. Any unit will crush anything they land on, but take 1 damage as a result.";
        tooltip.SetText(new Vector2(335, -220), body);
        StartCoroutine(BlinkTile(new Vector2(6,1)));
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        blinking = false;
        yield return null;
        body = "Return to the current floor to move FLAT out of the way.";
        tooltip.SetText(new Vector2(650, -230), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (FloorManager.instance.transitioning) break;
        }
        while (FloorManager.instance.transitioning) yield return null;
        scenario.player.units[1].selectable = false;
        scenario.player.units[2].selectable = false;
        scenario.player.units[3].selectable = false;
        UIManager.instance.LockHUDButtons(true);
        yield return new WaitForSecondsRealtime(1/Util.fps);
        StartCoroutine(BlinkTile(new Vector2(6,1)));
        scenario.player.units[0].selectable = true;
        while (!scenario.player.selectedUnit) yield return new WaitForSecondsRealtime(1/Util.fps);
        blinking = false;
        yield return new WaitForSecondsRealtime(1/Util.fps);
        StartCoroutine(BlinkTile(new Vector2(6,3)));
        while (!scenario.player.units[0].moved) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (scenario.player.selectedUnit)
                scenario.player.selectedUnit.validActionCoords = new List<Vector2>{ new Vector2(6, 3) };
        }
        scenario.player.DeselectUnit();
        blinking = false;
        while (scenario.player.unitActing) yield return null;
        scenario.player.units[0].selectable = true;
        scenario.player.units[2].selectable = true;
        scenario.player.units[2].ui.equipment[0].GetComponent<Button>().enabled = false;
        scenario.player.units[2].ui.equipment[1].GetComponent<Button>().enabled = false;
        body = "It's time to strike the NAIL. Move PONY into range, target me with the HAMMER, and pass to SPIKE.";
        tooltip.SetText(new Vector2(140, -10), body);
        StartCoroutine(BlinkTile(new Vector2(3,4)));
        while (!scenario.player.selectedUnit) yield return null;
        blinking = false;
        yield return new WaitForSecondsRealtime(1/Util.fps);
        StartCoroutine(BlinkTile(new Vector2(2,3)));
        while (!scenario.player.units[2].moved) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (scenario.player.selectedUnit) {
                if (scenario.player.selectedUnit)
                    scenario.player.selectedUnit.validActionCoords = new List<Vector2> { new Vector2(2,3) };
            }
        }
        blinking = false;
        scenario.player.units[2].ui.equipment[1].GetComponent<Button>().enabled = true;
        while (scenario.player.unitActing) yield return null;
        StartCoroutine(BlinkTile(new Vector2(2,6)));
        scenario.player.units[2].ui.equipment[1].enabled = true;
        while (true) {
            yield return null;
            if (scenario.player.units[2].selectedEquipment) {
                if (scenario.player.units[2].selectedEquipment.firstTarget) break;
            }
        }
        blinking = false; yield return null;
        StartCoroutine(BlinkTile(new Vector2(1,5)));
        while (scenario.currentTurn == ScenarioManager.Turn.Player) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (scenario.player.units[2].selectedEquipment) {
                if (scenario.player.units[2].selectedEquipment.firstTarget)
                    scenario.player.units[2].validActionCoords = new List<Vector2> { new Vector2(1, 5 ) };
            }
        }               
        blinking = false;
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator Enemy02() {
        while (scenario.currentTurn == ScenarioManager.Turn.Descent)
            yield return new WaitForSecondsRealtime(1/Util.fps);

        currentEnemy = (TutorialEnemyManager)scenario.currentEnemy;
        body = "Solid landing. I land after all other units, but never on slag.";
        tooltip.SetText(new Vector2(400, 180), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
        scenario.currentTurn = ScenarioManager.Turn.Descent;
        yield return scenario.StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Enemy));
        body = "ENEMIES act first after a descent, but they scatter in fear and don't attack.";
        tooltip.SetText(body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
        yield return currentEnemy.StartCoroutine(currentEnemy.MoveInOrder(new List<Vector2>{ new Vector2(4, 6), new Vector2(5, 1) }));
        currentEnemy.EndTurn();
    }
    
    public IEnumerator Equip02() {
        while (scenario.currentTurn != ScenarioManager.Turn.Player) yield return null;

        scenario.player.units[0].selectable = false;
        scenario.player.units[1].selectable = false;
        scenario.player.units[2].selectable = false;
        scenario.player.units[3].selectable = false;
        UIManager.instance.LockHUDButtons(true);

        body = "These green tiles are BILE, a deadly substance for anything that falls in it. We can use this to our advantage.";
        tooltip.SetText(new Vector2(520, -20), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        
        scenario.player.units[0].selectable = true;
        body = "Move FLAT next to an enemy to use its equipment, BIG GRAB.";
        tooltip.SetText(new Vector2(625, -125), body);
        StartCoroutine(BlinkTile(new Vector2(6,3)));
        while (!scenario.player.selectedUnit) yield return null;
        blinking = false; yield return null;
        StartCoroutine(BlinkTile(new Vector2(4,5)));
        while (!scenario.player.units[0].moved) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (scenario.player.selectedUnit) 
                scenario.player.selectedUnit.validActionCoords = new List<Vector2> { new Vector2(4, 5) };
        }
        blinking = false;
        while (scenario.player.unitActing) yield return null;
        scenario.player.units[0].ui.equipment[0].GetComponent<Button>().enabled = true;
        UIManager.instance.LockHUDButtons(true);
        body = "Equip BIG GRAB, select the enemy in range, then select a BILE tile to toss it into.";
        tooltip.SetText(new Vector2(375, 250), body);
        while (!scenario.player.units[0].selectedEquipment) yield return null;
        StartCoroutine(BlinkTile(new Vector2(4,6)));
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (scenario.player.units[0].selectedEquipment) {
                if (scenario.player.units[0].selectedEquipment.firstTarget) break;
            }
        }
        blinking = false; yield return null;
        StartCoroutine(BlinkTile(new Vector2(5, 6))); StartCoroutine(BlinkTile(new Vector2(6, 6))); StartCoroutine(BlinkTile(new Vector2(6, 5))); StartCoroutine(BlinkTile(new Vector2(7, 5)));
        while (scenario.player.units[0].energyCurrent > 0) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (scenario.player.units[0].selectedEquipment) {
                if (scenario.player.units[0].selectedEquipment.firstTarget) {
                    scenario.player.units[0].validActionCoords = new List<Vector2> { new Vector2(5, 6), new Vector2(6, 6), new Vector2(6, 5), new Vector2(7,5) };
                }
            }
        }
        blinking = false;
        while (scenario.player.unitActing) yield return null;
        scenario.player.units[1].ui.equipment[0].GetComponent<Button>().enabled = false;
        yield return new WaitForSecondsRealtime(0.125f);
        scenario.player.DeselectUnit();
        scenario.player.units[0].selectable = false;
        UIManager.instance.LockHUDButtons(true);
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator Descent02() {
        while (scenario.player.unitActing) yield return null;

        scenario.player.units[2].selectable = true;
        scenario.player.units[2].ui.equipment[0].GetComponent<Button>().enabled = false;

        body = "PONY landed in BLOOD. BLOOD won't damage units, but it restricts you from using the HAMMER or equipment.";
        tooltip.SetText(new Vector2(375, 180), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        body = "MOVE PONY out of there.";
        tooltip.SetText(new Vector2(-150, 280), body);
        StartCoroutine(BlinkTile(new Vector2(2,3)));
        while (!scenario.player.selectedUnit) yield return null;
        blinking = false; yield return null;
        StartCoroutine(BlinkTile(new Vector2(2,1)));
        while (!scenario.player.units[2].moved) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (scenario.player.selectedUnit) 
                scenario.player.selectedUnit.validActionCoords = new List<Vector2> { new Vector2( 2, 1 ) };
        }
        blinking = false;
        while (scenario.player.unitActing) yield return null;
        scenario.player.DeselectUnit();
        yield return new WaitForSecondsRealtime(0.125f);
        scenario.player.units[2].selectable = false;
        scenario.player.units[1].selectable = true;
        scenario.player.units[1].ui.equipment[0].GetComponent<Button>().enabled = false;
        scenario.player.units[1].ui.equipment[1].GetComponent<Button>().enabled = false;
        UIManager.instance.LockHUDButtons(true);
        body = "SPIKE has the HAMMER, but I can't be hit to descend until next turn. Line SPIKE up to attack the enemy instead.";
        tooltip.SetText(new Vector2(200, 165), body);
        StartCoroutine(BlinkTile(new Vector2(1, 5)));
        while (!scenario.player.selectedUnit) yield return null;
        blinking = false; yield return null;
        StartCoroutine(BlinkTile(new Vector2(1, 1)));
        while (!scenario.player.units[1].moved) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (scenario.player.selectedUnit) 
                scenario.player.selectedUnit.validActionCoords = new List<Vector2> { new Vector2( 1, 1 ) };
        }
        blinking = false;
        scenario.player.units[2].selectable = false;
        scenario.player.units[1].selectable = false;
        while (scenario.player.unitActing)
            yield return new WaitForSecondsRealtime(1/Util.fps);
        UIManager.instance.LockHUDButtons(true);
        body = "Oops. SPIKE can't hit that enemy with PONY in the way.";
        tooltip.SetText(new Vector2(130, 180), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        body = "You can UNDO your slags' MOVEMENT only. Once a slag performs an action, you can't UNDO any slags' previous MOVE. Plan accordingly!";
        tooltip.SetText(new Vector2(650, -200), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        UIManager.instance.LockHUDButtons(false);
        UIManager.instance.endTurnButton.enabled = false;
        UIManager.instance.LockFloorButtons(true);
        body = "UNDO SPIKE and PONY's MOVE so you can attack with SPIKE before PONY MOVES.";
        tooltip.SetText(body);
        while (scenario.player.undoableMoves.Count > 0) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }
        scenario.player.units[1].selectable = true;
        body = "Now, attack that last enemy and pass the HAMMER to PONY.";
        tooltip.SetText(new Vector2(-220, 420), body);
        StartCoroutine(BlinkTile(new Vector2(1,5)));
        while (!scenario.player.selectedUnit) yield return null;
        blinking = false; yield return null;
        StartCoroutine(BlinkTile(new Vector2(1, 1)));
        while (!scenario.player.units[1].moved) {
            yield return null;
            scenario.player.selectedUnit.validActionCoords = new List<Vector2> { new Vector2( 1, 1 ) };
        }
        blinking = false; yield return null;
        StartCoroutine(BlinkTile(new Vector2(5, 1)));
        scenario.player.units[1].ui.equipment[1].GetComponent<Button>().enabled = true;
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (scenario.player.units[1].selectedEquipment) {
                if (scenario.player.units[1].selectedEquipment.firstTarget) break;
            }
        }
        while (scenario.player.unitActing) yield return null;
        yield return new WaitForSecondsRealtime(1/Util.fps);
        UIManager.instance.LockHUDButtons(true);
        blinking = false; yield return null;
        StartCoroutine(BlinkTile(new Vector2(2,3)));
        while (scenario.player.units[1].energyCurrent > 0) {
            yield return new WaitForSecondsRealtime(1/Util.fps);   
                if (scenario.player.units[1].selectedEquipment) {
                    if (scenario.player.units[1].selectedEquipment.firstTarget)
                        scenario.player.units[1].validActionCoords = new List<Vector2> { new Vector2( 2, 3 ) };
            }
        }
        blinking = false;
        scenario.player.units[1].selectable = false;
        while (scenario.player.unitActing)
            yield return new WaitForSecondsRealtime(1/Util.fps);
        yield return new WaitForSecondsRealtime(1/Util.fps);
        UIManager.instance.LockHUDButtons(true);
        scenario.player.units[2].selectable = true;
        body = "Don't forget, eliminating the last enemy will also trigger a DESCENT.";
        tooltip.SetText(new Vector2(-90, 360), body);
        StartCoroutine(BlinkTile(new Vector2(2,3)));
        while (!scenario.player.selectedUnit) yield return null;
        blinking = false; yield return null;
        StartCoroutine(BlinkTile(new Vector2(2, 1)));
        while (!scenario.player.units[2].moved) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (scenario.player.selectedUnit) {
                scenario.player.selectedUnit.validActionCoords = new List<Vector2> { new Vector2( 2, 1 ) };
            }
        }
        
        blinking = false; 
        while (scenario.player.unitActing) yield return null;
        yield return new WaitForSecondsRealtime(1/Util.fps);
        UIManager.instance.LockHUDButtons(true);
        StartCoroutine(BlinkTile(new Vector2(5, 1)));
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (scenario.player.units[2].selectedEquipment) {
                if (scenario.player.units[2].selectedEquipment.firstTarget) break;
            }
        }
        blinking = false; yield return null;
        StartCoroutine(BlinkTile(new Vector2(4,5)));
        while (scenario.currentTurn == ScenarioManager.Turn.Player) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (scenario.player.selectedUnit) {
                if (!scenario.player.units[2].moved) {
                } else if (scenario.player.selectedUnit.selectedEquipment) {
                    if (scenario.player.selectedUnit.selectedEquipment.firstTarget)
                        scenario.player.selectedUnit.validActionCoords = new List<Vector2> { new Vector2( 4, 5 ) };
                }
            }
        }
        yield return new WaitForSecondsRealtime(1/Util.fps);
        UIManager.instance.LockHUDButtons(true);
        blinking = false;
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator Message03() {
        while (scenario.currentTurn == ScenarioManager.Turn.Descent)
            yield return new WaitForSecondsRealtime(1/Util.fps);

        body = "Good job going down. Remember, our primary goal is to descend as far as possible.";
        tooltip.SetText(new Vector2(90, -60), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }

        body = "We landed on an empty floor. When this happens we use our momentum to trigger a CASCADE. Slag won't be able to act on this floor, and we DESCEND again immediately.";
        tooltip.SetText(body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        
        body = "Don't worry though. Before they fall, you can reposition them anywhere on the next floor. This happens at the very beginning of a run too.";
        tooltip.SetText(body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        body = "That's enough to get you started. There is more for you to discover on your own. Good luck in the DEEPMESS.";
        tooltip.SetText(body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
        
        scenario.player.units[0].ui.equipment[0].GetComponent<Button>().enabled = true;
        scenario.player.units[0].ui.equipment[1].GetComponent<Button>().enabled = true;
        scenario.player.units[1].ui.equipment[0].GetComponent<Button>().enabled = true;
        scenario.player.units[2].ui.equipment[0].GetComponent<Button>().enabled = true;
        currentEnemy = (TutorialEnemyManager)scenario.currentEnemy;
        scenario.currentTurn = ScenarioManager.Turn.Descent;
        FloorManager.instance.Descend(true, false);
    }

}
