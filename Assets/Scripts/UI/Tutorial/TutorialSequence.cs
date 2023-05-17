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

        header = "WELCOME to DEEPMESS";
        body = "It's time to dig deep and make a mess. I'm the NAIL and I'll give you a few tips on how to descend down through this head." + '\n';
        tooltip.SetText(new Vector3(100,0,0), body, header);

        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        
        header = "";
        body = "You’re in control of three SLAGS: Flat, Pony, and Spike." + '\n' + '\n' + "Together, we need to navigate this limited space and hostile ANTIBODIES in order to descend.";
        tooltip.SetText(new Vector3(100,0,0), body, header);

        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator Select01() {
        header = "";
        body = "Select Flat to move it in line with this ANTIBODY.";
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
        body = "Move Flat here by selecting the highlighted tile.";
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
        body = "This is the equipment panel. Equip the HAMMER by selecting its button.";
        tooltip.SetText(new Vector2(-300, -125), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (scenario.player.selectedUnit) {
                if (scenario.player.selectedUnit.selectedEquipment) break;
            }
        }
        body = "Select the ANTIBODY to target it.";
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
        body = "Now select a SLAG for the HAMMER to bounce to." + '\n' + '\n' + "Let's send it to Pony.";
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

        body = "Great. That did some damage, but we'll need to attack it again to finish it off.";
        tooltip.SetText(new Vector2(680, -100), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        body = "The HAMMER can be bounced between SLAGS or it can bounce back to the SLAG that threw it.";
        tooltip.SetText(new Vector2(440, 110), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator Select03() {
        scenario.player.units[2].selectable = true;

        body = "MOVE Pony in range of the ANTIBODY and attack it again with the HAMMER." + '\n' + '\n' + "Select Pony again for the HAMMER to bounce back to it.";
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

        body = "That one is taken care of. There's still an ANTIBODY on the floor, but the HAMMER can't be thrown again because Pony is out of actions.";
        tooltip.SetText(new Vector2(680, -100), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        body = "Spike can still act, let's see what it can do after we take a look below.";
        tooltip.SetText(new Vector2(210, 225), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        UIManager.instance.LockFloorButtons(false);
        body = "DEEPMESS is all about getting an advantage on the ANTIBODIES below." + '\n' + "Select the PEEK button to get a look at the floor below.";
        tooltip.SetText(new Vector2(650, -200), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (FloorManager.instance.transitioning) break;
        }
        while (FloorManager.instance.transitioning)
            yield return new WaitForSecondsRealtime(1/Util.fps);
        UIManager.instance.LockFloorButtons(true);
        body = "This is a preview of the next floor. You can see where ANTIBODIES and other hazards are located.";
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
        body = "Let's use Spike's equipment to crush one of these ANTIBODIES." + '\n' + '\n' + "Select the PEEK button again to return to the top floor.";
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
        body = "Move Spike into position over the ANTIBODY. Use the PEEK button to line them up.";
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

        body = "This equipment allows Spike to MOVE again, leaving an ANVIL on the previous tile. Equipment can only be used once per floor, so use it wisely!";
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
        body = "Now that we set this trap, we're out of actions. Press END TURN and ANTIBODIES will respond.";
        tooltip.SetText(new Vector2(650, -225), body);
        while (scenario.currentTurn == ScenarioManager.Turn.Player)
            yield return new WaitForSecondsRealtime(1/Util.fps);       
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator Enemy01() {
        while (scenario.currentTurn != ScenarioManager.Turn.Enemy) yield return null;
        yield return new WaitForSecondsRealtime(0.5f);

        body = "ANTIBODIES can move 2 tiles and then attack anything next to them.";
        tooltip.SetText(new Vector2(480, -90), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
        yield return StartCoroutine(currentEnemy.MoveInOrder(new List<Vector2>{ new Vector2(3,6) }, new List<Vector2>{ new Vector2(2,6) }));
        body = "Ouch! This ANTIBODY attacked me! But I'm not completely helpless." + '\n' + '\n' + "I deal damage to anything that attacks me.";
        tooltip.SetText(new Vector2(400, 90), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        body = "Be careful, though. If I or all three of your SLAGS are destroyed, we'll lose the run.";
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

        body = "It's our turn again. Let's descend to take advantage of our ANVIL trap.";
        tooltip.SetText(new Vector2(20, 120), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        
        body = "Descend to the next floor by striking me with the HAMMER, or by eliminating all ANTIBODIES on the current floor.";
        tooltip.SetText(new Vector2(265, 0), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
         
        UIManager.instance.LockFloorButtons(false);
        body = "Before descending, let's make sure your SLAGS are in a safe position." + '\n' + '\n' + "PEEK down to the next floor";
        tooltip.SetText(new Vector2(650, -200), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (FloorManager.instance.transitioning) break;
        }
        while (FloorManager.instance.transitioning) yield return null;
        yield return new WaitForSecondsRealtime(1/Util.fps);
        body = "Flat would land on a wall if you descend now. SLAGS and ANTIBODIES crush anything they land on, but take damage as a result.";
        tooltip.SetText(new Vector2(335, -220), body);
        StartCoroutine(BlinkTile(new Vector2(6,1)));
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        blinking = false;
        yield return null;
        body = "Return to the current floor to move Flat out of the way.";
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
        body = "It's time to strike me, the NAIL. Move Pony into range and select the HAMMER." + '\n' + "Target the NAIL and bounce the HAMMER to Spike.";
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
        body = "Solid landing! I land after everything else, crushing anything I land on." + '\n' + '\n' + "Don't worry, I won't land on any of your SLAGS.";
        tooltip.SetText(new Vector2(400, 180), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
        scenario.currentTurn = ScenarioManager.Turn.Descent;
        yield return scenario.StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Enemy));
        body = "When you land on a floor, ANTIBODIES scatter but can't attack yet. Let's see what we're dealing with.";
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
        body = "Move Flat next to this ANTIBODY and use its equipment, BIG GRAB.";
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

        body = "Pony landed in BLOOD. BLOOD doesn't do damage, but it prevents using the HAMMER or equipment.";
        tooltip.SetText(new Vector2(375, 180), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        body = "MOVE Pony out of there.";
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
        body = "Spike has the HAMMER, but I can't be struck to descend until next turn. Line Spike up to attack the ANTIBODY instead.";
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
        body = "Oops. Spike can't hit that ANTIBODY with Pony in the way.";
        tooltip.SetText(new Vector2(130, 180), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        body = "You can UNDO your SLAGS' MOVEMENT. Once a SLAG performs an action, however, you can't UNDO any previous MOVE. Plan accordingly!";
        tooltip.SetText(new Vector2(650, -200), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        UIManager.instance.LockHUDButtons(false);
        UIManager.instance.endTurnButton.enabled = false;
        UIManager.instance.LockFloorButtons(true);
        body = "UNDO Spike and Pony's MOVE so you can attack with Spike before Pony MOVES.";
        tooltip.SetText(body);
        while (scenario.player.undoableMoves.Count > 0) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }
        scenario.player.units[1].selectable = true;
        body = "Now, attack that last ANTIBODY and bounce the HAMMER to Pony.";
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
        body = "Remember, you descend after striking the NAIL or as soon as the last ANTIBODY is defeated on a floor." + '\n' + "Make sure you're prepared!";
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

        body = "Good job going down. Remember, our goal is to descend as far as possible, and if I or all three SLAGS are destroyed, the run is over!";
        tooltip.SetText(new Vector2(90, -60), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }

        body = "We landed on an empty floor. When this happens we use our momentum to trigger a CASCADE. SLAGS won't be able to act on this floor, and we DESCEND again immediately.";
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
