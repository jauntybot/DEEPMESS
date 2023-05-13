using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    ScenarioManager scenario;
    [SerializeField] TutorialEnemyManager currentEnemy;
    public List<FloorDefinition> scriptedFloors;
    public GameObject tutorialFloorPrefab, tutorialEnemyPrefab;
    public Tooltip tooltip;
    public Animator screenFade;
    [HideInInspector] public string header, body;
    bool blinking = false;


    public void Initialize(ScenarioManager manager) {
        scenario = manager;
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

        Destroy(gameObject);
    }

    IEnumerator BlinkTile(Vector2 coord) {
        blinking = true;
        GridSquare sqr = scenario.player.currentGrid.sqrs.Find(sqr => sqr.coord == coord);
        float timer = 0;
        int i = 0;
        while (blinking) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            timer += Time.deltaTime;
            if (timer > 0.5f) {
                sqr.ToggleValidCoord(true, i%2==0 ? FloorManager.instance.playerColor : FloorManager.instance.equipmentColor);
                i++; timer = 0;
            }
        }
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
        body = "Select this unit to move it in line with an ENEMY.";
        tooltip.SetText(new Vector2(-200, -190), body);
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
        body = "Move your unit to this space by selecting a highlighted square.";
        tooltip.SetText(new Vector2(-85,-360), body);
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
        body = "Select the HAMMER and select the ENEMY in range.";
        tooltip.SetText(new Vector2(-360, -200), body);
        StartCoroutine(BlinkTile(new Vector2(4,1)));
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
        while (scenario.player.unitActing)
            yield return new WaitForSecondsRealtime(1/Util.fps);
        UIManager.instance.LockHUDButtons(true);
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }


    public IEnumerator Select02() {
        body = "After selecting its first target, select a friendly unit for the HAMMER to bounce to.";
        tooltip.SetText(new Vector2(-130, -130), body);

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

        body = "Great. That knocked off the enemy's shield. We'll need to attack it again to kill it for good.";
        tooltip.SetText(new Vector2(270, -280), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        body = "The HAMMER can be passed between your units or it can bounce back to the unit that threw it.";
        tooltip.SetText(body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator Select03() {
        scenario.player.units[2].selectable = true;

        body = "MOVE Pony in range of the ENEMY and attack it again with the HAMMER." +'\n' + '\n' + "Then select Pony again to bounce it back to them.";
        tooltip.SetText(new Vector2(60, 240), body);
        while (!scenario.player.units[2].moved) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
            if (scenario.player.selectedUnit) 
                scenario.player.selectedUnit.validActionCoords = new List<Vector2>{new Vector2(3,4)};
        }
        while (scenario.player.unitActing)
            yield return new WaitForSecondsRealtime(1/Util.fps);
        UIManager.instance.LockHUDButtons(true);
        while (scenario.player.units[2].energyCurrent > 0) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (scenario.player.selectedUnit) {
                if (scenario.player.selectedUnit.selectedEquipment) {
                    if (scenario.player.selectedUnit.selectedEquipment.firstTarget)
                        scenario.player.selectedUnit.validActionCoords = new List<Vector2>{ new Vector2(3,4) };
                }
            }
        }
        while (scenario.player.unitActing)
            yield return new WaitForSecondsRealtime(1/Util.fps);
        UIManager.instance.LockHUDButtons(true);
        
        scenario.player.units[2].selectable = false;
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator Message02() {
        while (scenario.player.unitActing)
            yield return new WaitForSecondsRealtime(1/Util.fps);

        body = "That ENEMY is taken care of. There's still an ENEMY on the floor, but the HAMMER can't be thrown again because Pony already acted.";
        tooltip.SetText(new Vector2(270, -280), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        body = "You still have one unit able to act before you end your turn, let's see what they can do soon.";
        tooltip.SetText(new Vector2(-230, 50), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        UIManager.instance.LockFloorButtons(false);
        body = "DEEPMESS is about descending, and crushing enemies below. PEEK down at the floor below here.";
        tooltip.SetText(new Vector2(650, -280), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (FloorManager.instance.transitioning) break;
        }
        while (FloorManager.instance.transitioning)
            yield return new WaitForSecondsRealtime(1/Util.fps);
        UIManager.instance.LockFloorButtons(true);
        body = "This is a preview of the next floor. You can see where future enemies and hazards are.";
        tooltip.SetText(new Vector2(0,0), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        body = "You also see where the units above will land after a descent. Let's use your last unit's equipment to crush an enemy below.";
        tooltip.SetText(new Vector2(-225, 0), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        UIManager.instance.LockFloorButtons(false);
        body = "Use the PEEK button again to return to the top floor.";
        tooltip.SetText(new Vector2(650, -270), body);
        while (FloorManager.instance.transitioning) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
        }
        scenario.player.units[0].selectable = false;
        scenario.player.units[1].selectable = false;
        scenario.player.units[2].selectable = false;
        scenario.player.units[3].selectable = false;

        UIManager.instance.LockHUDButtons(true);
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator Select04() {
        scenario.player.units[1].selectable = true;
        UIManager.instance.LockFloorButtons(false);
        body = "Move Spike into position over an ENEMY. Use the PEEK button to line them up.";
        tooltip.SetText(new Vector2(-230, 50), body);
        while (!scenario.player.units[1].moved) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (scenario.player.selectedUnit)
                scenario.player.selectedUnit.validActionCoords = new List<Vector2>{ new Vector2(1, 4) };
        }
        while (scenario.player.unitActing)
            yield return new WaitForSecondsRealtime(1/Util.fps);
        UIManager.instance.LockHUDButtons(true);
        body = "Select the ANVIL and move again, leaving an ANVIL where Spike was.";
        tooltip.SetText(new Vector2(-130, 50), body);
        while (scenario.player.units[1].energyCurrent > 0) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (scenario.player.selectedUnit) {
                if (scenario.player.selectedUnit.selectedEquipment) {
                    scenario.player.units[1].validActionCoords = new List<Vector2> { new Vector2(1, 3) };
                    
                }
            }
        }
        while (scenario.player.unitActing)
            yield return new WaitForSecondsRealtime(1/Util.fps);
        UIManager.instance.LockFloorButtons(true);
        body = "Now your units are are out of actions. Press END TURN and ENEMIES will respond.";
        tooltip.SetText(new Vector2(650, -270), body);
        while (scenario.currentTurn == ScenarioManager.Turn.Player)
            yield return new WaitForSecondsRealtime(1/Util.fps);       
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator Enemy01() {

        body = "ENEMIES can move 2 spaces and then attack an adjacent space for 1 damage.";
        tooltip.SetText(new Vector2(430, -20), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
        yield return StartCoroutine(currentEnemy.CalculateAction((EnemyUnit)currentEnemy.units[0]));
        body = "This enemy attacked the NAIL, but the NAIL deals 1 damage back to attackers.";
        tooltip.SetText(new Vector2(430, -20), body);
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

        scenario.player.units[0].selectable = false;
        scenario.player.units[1].selectable = false;
        scenario.player.units[2].selectable = false;
        scenario.player.units[3].selectable = false;

        body = "With an ANVIL trap laid, it's time to descend. Trigger a descent by striking the NAIL with the HAMMER, or by eliminating all ENEMIES on the current floor.";
        tooltip.SetText(new Vector2(430, -20), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        body = "Before descending, check to make sure your units aren't in a compromising position.";
        tooltip.SetText(new Vector2(650, -280), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (FloorManager.instance.transitioning) break;
        }
        body = "Flat here would land on a wall if you descend now. Any unit that lands on something takes 1 damage.";
        tooltip.SetText(body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        body = "Return to the current floor to move Flat out of the way.";
        tooltip.SetText(new Vector2(650, -280), body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (FloorManager.instance.transitioning) break;
        }
        scenario.player.units[0].selectable = true;
        while (!scenario.player.units[0].moved) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (scenario.player.selectedUnit)
                scenario.player.selectedUnit.validActionCoords = new List<Vector2>{ new Vector2(6, 4) };
        }
        body = "Now you can trigger a descent. Strike me with the HAMMER for this one.";
        tooltip.SetText(body);
        while (scenario.currentTurn == ScenarioManager.Turn.Player) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (!scenario.player.units[2].moved) {
                if (scenario.player.selectedUnit)
                    scenario.player.selectedUnit.validActionCoords = new List<Vector2> { new Vector2(2,3) };
            } else if (scenario.player.units[2].selectedEquipment) {
                if (scenario.player.units[2].selectedEquipment.firstTarget)
                    scenario.player.units[2].validActionCoords = new List<Vector2> { new Vector2(1, 3 ) };
            }             
        }
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator Enemy02() {
        while (scenario.currentTurn == ScenarioManager.Turn.Descent)
            yield return new WaitForSecondsRealtime(1/Util.fps);

        currentEnemy = (TutorialEnemyManager)scenario.currentEnemy;
        body = "Solid landing. I land after all other units on a random space, excluding spaces with friendly units.";
        tooltip.SetText(body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        yield return scenario.StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Enemy));
        body = "ENEMIES act first after a descent, but they scatter in fear and don't attack.";
        tooltip.SetText(body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        currentEnemy.StartCoroutine(currentEnemy.MoveInOrder(new List<Vector2>{ new Vector2(4, 6), new Vector2(5, 1) }));
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
        while (scenario.currentTurn == ScenarioManager.Turn.Enemy) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
        }
    }
    
    public IEnumerator Equip02() {

        body = "These green tiles are BILE, a deadly substance for anything that enters it.";
        tooltip.SetText(body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        
        scenario.player.units[0].selectable = true;
        body = "Move Flat next to an enemy to use its equipment.";
        tooltip.SetText(body);
        while (!scenario.player.units[0].moved) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (scenario.player.selectedUnit) 
                scenario.player.selectedUnit.validActionCoords = new List<Vector2> { new Vector2(5, 6) };
        }
        body = "Select BIG GRAB, then select the enemy, then select a BILE tile to toss it into.";
        tooltip.SetText(body);
        while (scenario.player.units[0].energyCurrent > 0) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (scenario.player.selectedUnit) {
                if (scenario.player.selectedUnit.selectedEquipment) {
                    if (scenario.player.selectedUnit.selectedEquipment.firstTarget) {
                        //scenario.player.selectedUnit.validActionCoords = new List<Vector2> { new Vector2(5, 3) };
                    }
                }
            }
        }
        scenario.player.units[0].selectable = false;
        UIManager.instance.LockHUDButtons(true);
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator Descent02() {

        scenario.player.units[2].selectable = true;
        body = "Pony landed in BLOOD. BLOOD won't damage units, but it restricts you from using the HAMMER or equipment.";
        tooltip.SetText(body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        body = "Move Pony out of there.";
        tooltip.SetText(body);
        while (!scenario.player.units[2].moved) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (scenario.player.selectedUnit) 
                scenario.player.selectedUnit.validActionCoords = new List<Vector2> { new Vector2( 2, 1 ) };
        }
        scenario.player.units[2].selectable = false;
        scenario.player.units[1].selectable = true;
        while (scenario.player.unitActing)
            yield return new WaitForSecondsRealtime(1/Util.fps);
        body = "Spike has the hammer, but it can't strike me until your next turn. Line it up to attack the enemy.";
        tooltip.SetText(body);
        while (!scenario.player.units[1].moved) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (scenario.player.selectedUnit) 
                scenario.player.selectedUnit.validActionCoords = new List<Vector2> { new Vector2( 1, 1 ) };
        }
        scenario.player.units[2].selectable = false;
        scenario.player.units[1].selectable = false;
        while (scenario.player.unitActing)
            yield return new WaitForSecondsRealtime(1/Util.fps);
        body = "Oops. Spike can't hit that enemy with Pony in the way.";
        tooltip.SetText(body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        body = "You can undo your units' MOVEMENT only. If any unit performs an action it clears your undo history.";
        tooltip.SetText(body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        UIManager.instance.LockHUDButtons(false);
        UIManager.instance.endTurnButton.enabled = false;
        UIManager.instance.LockFloorButtons(true);
        body = "Undo Spike's and Pony's movement so you can attack with Spike before moving Pony.";
        tooltip.SetText(body);
        while (scenario.player.undoableMoves.Count > 0) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }
        scenario.player.units[1].selectable = true;
        body = "Now, move Spike and attack that last enemy. Pass the hammer to Pony.";
        tooltip.SetText(body);
        while (scenario.player.units[1].energyCurrent > 0) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
            if (scenario.player.selectedUnit) {
                if (!scenario.player.units[1].moved)
                    scenario.player.selectedUnit.validActionCoords = new List<Vector2> { new Vector2( 1, 1 ) };
                else if (scenario.player.selectedUnit.selectedEquipment) {
                    if (scenario.player.selectedUnit.selectedEquipment.firstTarget)
                        scenario.player.selectedUnit.validActionCoords = new List<Vector2> { new Vector2( 2, 3 ) };
                }
            }
        }
        scenario.player.units[1].selectable = false;
        while (scenario.player.unitActing)
            yield return new WaitForSecondsRealtime(1/Util.fps);
        scenario.player.units[2].selectable = true;
        body = "Use Pony to finish the enemy. Don't forget, eliminating the last enemy will also trigger a DESCENT.";
        tooltip.SetText(body);
        while (scenario.currentTurn == ScenarioManager.Turn.Player) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (scenario.player.selectedUnit) {
                if (!scenario.player.units[2].moved) {
                    scenario.player.selectedUnit.validActionCoords = new List<Vector2> { new Vector2( 2, 1 ) };
                } else if (scenario.player.selectedUnit.selectedEquipment) {
                    if (scenario.player.selectedUnit.selectedEquipment.firstTarget)
                        scenario.player.selectedUnit.validActionCoords = new List<Vector2> { new Vector2( 5, 6 ) };
                }
            }
        }
    }

    public IEnumerator Message03() {
        while (scenario.currentTurn == ScenarioManager.Turn.Descent)
            yield return new WaitForSecondsRealtime(1/Util.fps);

        body = "Good job going down. Remember, our primary goal is to descend as far as possible.";
        tooltip.SetText(body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }

        body = "We landed on an empty floor. When this happens we use our momentum to trigger a CASCADE. Your units won't be able to act on this floor, and we DESCEND again immediately.";
        tooltip.SetText(body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        
        body = "Don't worry though. Before they fall, you can move them wherever on the next floor. This happens at the very beginning of a run too.";
        tooltip.SetText(body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        body = "That's all I have to teach you. There is more for you to discover on your own. Good luck.";
        tooltip.SetText(body);
        while (true) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            if (Input.GetMouseButtonDown(0)) break;
        }
        currentEnemy = (TutorialEnemyManager)scenario.currentEnemy;
        scenario.currentTurn = ScenarioManager.Turn.Descent;
        FloorManager.instance.Descend(true, false);
    }

}
