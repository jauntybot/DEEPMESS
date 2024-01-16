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

    ScenarioManager scenario;
    FloorManager floorManager;
    public FloorPacket tutorialPacket;
    
    public DialogueTooltip tooltip, brTooltip;
    public Animator screenFade;
    [HideInInspector] public string header, body;
    public bool blinking = false;
    bool cont = false;
    List<Unit> playerUnits = new();

    [SerializeField] Color keyColor;

    int descents;

    [Header("GIF Serialization")]
    [SerializeField] RuntimeAnimatorController hittingTheNailAnim;
    [SerializeField] RuntimeAnimatorController hittingEnemiesAnim, shieldAnim, anvilAnim, bigGrabAnim;

    [Header("Button Highlights")]
    [SerializeField] GameObject buttonHighlight;
    [SerializeField] Transform peekButton;
    GameObject destroyHighlight;

    [Header("Gameplay Optional Tooltips")]
    bool enemyBehavior = false;
    public bool hittingEnemies = false, enemySpawnEncountered = false, undoEncountered = false, nailDamageEncountered = false, bloodEncountered = false, 
        objectivesEncountered = false, collisionEncountered = false, slotsEncountered = false;


    public void Initialize(ScenarioManager manager) {
        scenario = manager;
        floorManager = scenario.floorManager;


        floorManager.floorSequence.currentThreshold = FloorPacket.PacketType.Tutorial;    
        floorManager.floorSequence.floorsTutorial = 3;
        floorManager.floorSequence.localPackets.Add(tutorialPacket);
        
        descents = 0;

        floorManager.GenerateFloor(floorManager.floorSequence.GetFloor(), true);
        floorManager.GenerateFloor(floorManager.floorSequence.GetFloor());
    }
    
    public IEnumerator Tutorial() {
        for (int i = 0; i <= scenario.player.units.Count - 1; i++) {
            Unit u = scenario.player.units[i];
            playerUnits.Add(u);
            u.grid = floorManager.currentFloor;
            if (floorManager.currentFloor.gridElements.Contains(u))
                floorManager.currentFloor.RemoveElement(u);
        }
        //scenario.player.units[1].ui.overview.gameObject.SetActive(false); scenario.player.units[2].ui.overview.gameObject.SetActive(false);
        scenario.player.units[0].ui.equipButtons[0].GetComponent<Button>().interactable = false;
        scenario.player.units[0].ui.equipButtons[0].GetComponent<Button>().enabled = false;
        
        scenario.player.units[1].EnableSelection(false);
        scenario.player.units[2].EnableSelection(false);
        scenario.player.units.RemoveAt(1); scenario.player.units.RemoveAt(1);

        floorManager.currentFloor.RemoveElement(scenario.player.units[1]);
        yield return StartCoroutine(floorManager.DescendUnits(new List<GridElement>{ scenario.player.units[1] }));
        //foreach (Unit unit in scenario.player.units) unit.gameObject.SetActive(false);
        yield return new WaitForSecondsRealtime(0.5f);
        scenario.player.nail.ToggleNailState(Nail.NailState.Primed);
        yield return new WaitForSecondsRealtime(1.25f);
        yield return StartCoroutine(SplashMessage());
        
// Drop Flat
        yield return StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Descent, ScenarioManager.Scenario.Combat));
        floorManager.currentFloor.RemoveElement(scenario.player.units[0]);
        scenario.player.units[0].coord = new Vector2(1,2);
        yield return StartCoroutine(floorManager.DescendUnits(new List<GridElement> { scenario.player.units[0] }));
        yield return new WaitForSecondsRealtime(0.15f);

        yield return StartCoroutine(ExplainMovement());
        yield return scenario.StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Player));

        while (true) {
            yield return null;
            if (scenario.player.units[0].coord.x == scenario.player.nail.coord.x || scenario.player.units[0].coord.y == scenario.player.nail.coord.y)
                break;
                else if (scenario.player.units[0].moved && !undoEncountered) {
                    yield return StartCoroutine(Oopsies(1));
                }
        }

        yield return new WaitForSecondsRealtime(0.5f);
        yield return StartCoroutine(HittingTheNail());
        
        

        while (descents < 1) yield return null;
// Descent 1
        yield return StartCoroutine(DiggingDown());
        yield return StartCoroutine(scenario.messagePanel.PlayMessage(MessagePanel.Message.Antibody));
        yield return StartCoroutine(ScatterTurn());
        yield return StartCoroutine(EnemyTurn());

        yield return new WaitForSecondsRealtime(1.25f);
        yield return StartCoroutine(NailPriming());
        destroyHighlight = Instantiate(buttonHighlight, peekButton);
        destroyHighlight.transform.SetSiblingIndex(0); destroyHighlight.transform.localPosition = Vector3.zero; destroyHighlight.GetComponent<Animator>().SetBool("Active", true);
        destroyHighlight.GetComponent<Animator>().keepAnimatorStateOnDisable = true;

        Coroutine co = StartCoroutine(AttackingEnemies());

        cont = false;
        while (!cont) yield return null;
// Descent 2

        StopCoroutine(co);
        
        yield return StartCoroutine(scenario.messagePanel.PlayMessage(MessagePanel.Message.Antibody));
        yield return StartCoroutine(EnemyTurn());
        yield return new WaitForSecondsRealtime(0.75f);

        yield return StartCoroutine(Equipment());
        if (!hittingEnemies)
            StartCoroutine(AttackingEnemies());
        
        yield return scenario.StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Player));
    }

    public IEnumerator BlinkTile(Vector2 coord, bool move = true) {
        blinking = true;
        Tile sqr = FloorManager.instance.currentFloor.tiles.Find(sqr => sqr.coord == coord);
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

        header = "";
        body = "Listen up, squish! We're on a big brain mission. Gottat feast on <b>" + ColorToRichText("tasty thoughts", keyColor) + "</b>, yeah? Scavenge, gobble, we'll make it this place our own." + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }

        header = "";
        body = "Gotta dive deeper into the noggin. Down, down, where the <b>" + ColorToRichText("juice gets jucier", keyColor) + "</b>. We're after the prime cuts, not the stale scraps." + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);

        }
        
        header = "";
        body = "We'll toss ya a <b>" + ColorToRichText("Slag unit", keyColor) + "</b>. Control it during our dig.";
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
        }
        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator ExplainMovement() {
        screenFade.gameObject.SetActive(true);

        body = "Unlike us, <b>" + ColorToRichText("Slags", keyColor) + "</b> can move on the floor. Select this one, move it. <b>" + ColorToRichText("Line it up with the Nail", keyColor) + "</b>." + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1 / Util.fps);

        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);

    }

    public IEnumerator HittingTheNail() {
        screenFade.gameObject.SetActive(true);

        header = "HITTING THE NAIL";
        body = "<b>" + ColorToRichText("Hammer's", keyColor) + "</b> our main tool. Grab it from the bottom left of your screen. <b>" + ColorToRichText("Chuck it straight", keyColor) + "</b>, smack a target. Then pick the Slag for it to <b>" + ColorToRichText("bounce back", keyColor) + "</b> to. For now, the <b>" + ColorToRichText("Nail's", keyColor) + "</b> your target. Give it a whirl or stand there looking dumb." + '\n';
        tooltip.SetText(body, header, true, new List<RuntimeAnimatorController>{ hittingTheNailAnim });
        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }
        
        UIManager.instance.canvasAnim.SetTrigger("TutUnit");
        
        screenFade.SetTrigger("FadeOut");
        Vector3 prevPos = tooltip.transform.GetComponent<RectTransform>().anchoredPosition;
        float timer = 0;

        while (timer < 0.25f) {
            tooltip.transform.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(prevPos, prevPos + new Vector3(680, 0), timer/0.25f);
            timer += Time.deltaTime;
            yield return null;
        }
        tooltip.transform.GetComponent<RectTransform>().anchoredPosition = prevPos + new Vector3(680, 0);
              
       
        GameObject highlight = Instantiate(buttonHighlight, scenario.player.units[0].ui.equipButtons[1].button.transform.GetChild(0).transform);
        highlight.transform.localPosition = Vector3.zero; highlight.GetComponent<Animator>().SetBool("Active", true);
        highlight.GetComponent<Animator>().keepAnimatorStateOnDisable = true;
        PlayerUnit pu = (PlayerUnit)scenario.player.units[0];
        while (pu.hammerUses == 0) yield return new WaitForSecondsRealtime(1/Util.fps);
        tooltip.transform.GetComponent<RectTransform>().anchoredPosition = prevPos;
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
        Destroy(highlight);

    }

    public IEnumerator DiggingDown() {
        screenFade.gameObject.SetActive(true);
        
        header = "DIGGING DOWN";
        body = "Hit the <b>" + ColorToRichText("Nail", keyColor) + "</b> with the <b>" + ColorToRichText("Hammer", keyColor) + "</b>—floor crumbles, <b>" + ColorToRichText("all units crash below", keyColor) + "</b>. That's progress, squish." + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
        }
        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
        
    }

    public IEnumerator NailPriming() {
        screenFade.gameObject.SetActive(true);

        header = "NAIL PRIMING";
        body = "We need a breather for the next descent. <b>" + ColorToRichText("Until the Nail is primed, it can't be hit", keyColor) + "</b> by the Hammer or enemies. Smart, right?" + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator AttackingEnemies() {
        hittingEnemies = true;
        bool aligned = false;
        while (true) {
            yield return null;
            if (scenario.currentTurn == ScenarioManager.Turn.Player) {
                Unit unit = scenario.player.units.Find(u => u.equipment.Find(e => e is HammerData) != null);
                foreach (Unit enemy in scenario.currentEnemy.units) {
                    if (unit.coord.x == enemy.coord.x || unit.coord.y == enemy.coord.y)
                        aligned = true;
                }
            }
            if (aligned) break;
        }
        yield return new WaitForSecondsRealtime(0.25f);
        yield return StartCoroutine(HittingAnEnemy());
        
    }


    public IEnumerator HittingAnEnemy() {
        screenFade.gameObject.SetActive(true);

        header = "HITTING ENEMIES";
        body = "Hammer's like a boomerang—chuck it at an enemy, then you can <b>" + ColorToRichText("pick any Slag to bounce it back to", keyColor) + "</b>. Keep the rhythm or get beat, squish." + '\n';
        tooltip.SetText(body, header, true, new List<RuntimeAnimatorController>{ hittingEnemiesAnim });

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
        }

        screenFade.SetTrigger("FadeOut");
        Vector3 prevPos = tooltip.transform.GetComponent<RectTransform>().anchoredPosition;
        float timer = 0;

        while (timer < 0.25f) {
            tooltip.transform.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(prevPos, prevPos + new Vector3(680, 0), timer/0.25f);
            Debug.Log(prevPos + " " + tooltip.transform.GetComponent<RectTransform>().anchoredPosition);
            timer += Time.deltaTime;
            yield return null;
        }

        tooltip.transform.GetComponent<RectTransform>().anchoredPosition = prevPos + new Vector3(680, 0);
        GameObject highlight = Instantiate(buttonHighlight, scenario.player.units[0].ui.equipButtons[1].button.transform.GetChild(0).transform);
        highlight.transform.SetSiblingIndex(0); highlight.transform.localPosition = Vector3.zero; highlight.GetComponent<Animator>().SetBool("Active", true);
        highlight.GetComponent<Animator>().keepAnimatorStateOnDisable = true;

        while (true) {
            yield return null;
            if (scenario.player.units[0].energyCurrent == 0 || scenario.player.units[1].energyCurrent == 0) {
                Debug.Log("AP trigger");
                break;
            }
        }

        Destroy(highlight);
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
        tooltip.transform.GetComponent<RectTransform>().anchoredPosition = prevPos;
        yield return new WaitForSecondsRealtime(1.5f);
        yield return StartCoroutine(OnTurnMoveAndAP());

        tooltip.transform.GetChild(0).gameObject.SetActive(false);

    }
    public IEnumerator ScatterTurn() {
        screenFade.gameObject.SetActive(true);

        header = "ENEMY SCATTER";
        body = "When we land, <b>" + ColorToRichText("enemies scatter but don't attack", keyColor) + "</b>. A little dance before the real brawl." + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }  

    public IEnumerator OnTurnMoveAndAP() {
        screenFade.gameObject.SetActive(true);

        header = "PLAYER TURN";
        body = "Slags hustle, <b>" + ColorToRichText("moving and acting on their turn", keyColor) + "</b>. Trade turns with the enemy, end yours in the bottom right. When you dive down to the next floor, <b>" + ColorToRichText("a fresh start", keyColor) + "</b>. Keep it flowing." + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
        }

        UIManager.instance.canvasAnim.SetTrigger("TutEndTurn");

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }


    public IEnumerator Equipment() {
        screenFade.gameObject.SetActive(true);

        header = "EQUIPMENT";
        body = "<b>" + ColorToRichText("Slags pack gear", keyColor) + "</b>—special stuff. You can have them <b>" + ColorToRichText("use gear or Hammer each turn", keyColor) + "</b>. Eyes up, squish. Plan the play or end up short on actions." + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
        }

        header = "EQUIPMENT";
        body = "Each piece of gear's <b>" + ColorToRichText("unique", keyColor) + "</b>. Check those buttons on the <b>" + ColorToRichText("bottom left", keyColor) + "</b> to get to know your arsenal." + '\n';
        tooltip.SetText(body, header, true, new List<RuntimeAnimatorController>{ shieldAnim,  });

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);

        GameObject highlight = Instantiate(buttonHighlight, scenario.player.units[2].ui.equipButtons[0].button.transform);
        highlight.transform.parent.transform.parent.transform.parent.transform.parent.gameObject.SetActive(true);
        highlight.transform.SetSiblingIndex(0); highlight.transform.localPosition = Vector3.zero; highlight.GetComponent<Animator>().SetBool("Active", true);
        highlight.GetComponent<Animator>().keepAnimatorStateOnDisable = true;
        highlight.transform.parent.transform.parent.transform.parent.transform.parent.gameObject.SetActive(false);
        StartCoroutine(OnShieldUse(highlight));
    }

    public IEnumerator OnShieldUse(GameObject highlight) {
        Debug.Log(floorManager.currentFloor);
        while (scenario.player.units[2].energyCurrent > 0 && floorManager.currentFloor.index == 2) yield return null;
        Destroy(highlight);

    }
    public IEnumerator EnemyBehavior() {
        screenFade.gameObject.SetActive(true);

        header = "ENEMY TURN";
        body = "<b>" + ColorToRichText("Enemy units ain't decoration", keyColor) + "</b>. They move, they strike. These move 2 tiles and then <b>" + ColorToRichText("attack anything", keyColor) + "</b> nearby." + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
        CheckAllDone();
    }

    public IEnumerator TutorialEnd() {
        screenFade.gameObject.SetActive(true);

        header = "";
        body = "Playtime's over, squish. Time for the real deal. <b>" + ColorToRichText("Dig. Make mess", keyColor) + "</b>.";
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1 / Util.fps);
        }
        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }


// Gameplay optional - tutorial specific - tooltips
    public IEnumerator Objectives() {
        screenFade.gameObject.SetActive(true);
        objectivesEncountered = true;

        header = "OBJECTIVES";
        body = "The big slime's got a to-do list. Check 'em off, <b>" + ColorToRichText("score tasty god nuggets", keyColor) + "</b>. Upgrade gear, get stronger. Fail? No biggie, just a hiccup. No whining, squish, just keep grinding." + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }


    public IEnumerator NailDamage() {
        nailDamageEncountered = true;
        while (ScenarioManager.instance.currentTurn != ScenarioManager.Turn.Player) {
            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.25f);
        screenFade.gameObject.SetActive(true);

        header = "NAIL DAMAGE";
        body = "Enemies get a taste of their own medicine when they hit the Nail. Smack the big guy, <b>" + ColorToRichText("get smacked back", keyColor) + "</b>. Make 'em regret laying a finger on our centerpiece." + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
        CheckAllDone();
    }

    public IEnumerator PeekButton() {
        
        Destroy(destroyHighlight);
        header = "PEEK AHEAD";
        body = "<b>" + ColorToRichText("Peek button", keyColor) + "</b>, big ol' eye in the <b>" + ColorToRichText("bottom right", keyColor) + "</b>. Use it. <b>" + ColorToRichText("Preview the next floor", keyColor) + "</b>—enemies, hazards, the works. It helps to know what's comin'." + '\n';
        brTooltip.SetText(body, header, true);

        while (!brTooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }

        brTooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    IEnumerator Oopsies(int trigger) {
        if (trigger == 1) {
            header = "OOPSIES";
            body = "Come on, squish, use your noggin! Move your slag so it lines up with the nail--up, down, left, right. We have a button for oopsies in the bottom right." + '\n';    
        } else if (trigger == 2) {

        }
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
        }
    }

    public IEnumerator UndoTutorial() {
        
        header = "UNDO BUTTON";
        body = "Slags got an <b>" + ColorToRichText("Undo button", keyColor) + "</b>. Move around and reset, but once you take an action, no backtracking. Think ahead, squish." + '\n';
        brTooltip.SetText(body, header, true);

        while (!brTooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }

        undoEncountered = true;
        brTooltip.transform.GetChild(0).gameObject.SetActive(false);
        yield return new WaitForSecondsRealtime(0.15f);
        scenario.player.UndoMove();
        CheckAllDone();
    }

    public IEnumerator EnemySpawn() {
        screenFade.gameObject.SetActive(true);
        enemySpawnEncountered = true;

        header = "ENEMY SPAWNING";
        body = "Enemies won't give up. Clear too many, <b>" + ColorToRichText("more will drop in", keyColor) + "</b>. Better dive deeper when you can to avoid an ambush. Watch your back, squish." + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
        CheckAllDone();
    }
    public IEnumerator BloodTiles() {
        screenFade.gameObject.SetActive(true);
        bloodEncountered = true;

        header = "BLOOD TILES";
        body = "<b>" + ColorToRichText("Blood tiles", keyColor) + "</b>? Slags can't swing Hammer or use gear on those. Check the <b>" + ColorToRichText("top right", keyColor) + "</b> when hovering on a tile. Watch your step, squish." + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
        CheckAllDone();
    }
    

    public IEnumerator DescentDamage() {
        collisionEncountered = true;

        while (ScenarioManager.instance.currentTurn != ScenarioManager.Turn.Player) {
            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.25f);
        screenFade.gameObject.SetActive(true);

        header = "DESCENT DAMAGE";
        body = "<b>" + ColorToRichText("Slags", keyColor) + "</b> and <b>" + ColorToRichText("enemies crush", keyColor) + "</b> anything they land on. No free rides though—units take damage for squashing." + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
        CheckAllDone();
    }

    public IEnumerator TutorialDescend() {
        List<GridElement> toDescend = new();
        switch(descents) {
            case 0:
                floorManager.GenerateFloor(floorManager.floorSequence.GetFloor());
                yield return StartCoroutine(floorManager.TransitionFloors(true, false));
                yield return StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Descent, ScenarioManager.Scenario.Combat));
                scenario.player.nail.ToggleNailState(Nail.NailState.Falling);   
                yield return new WaitForSecondsRealtime(0.25f);
                
                for (int i = scenario.player.units.Count - 1; i >= 0; i--) 
                    toDescend.Add(scenario.player.units[i]);

                floorManager.currentFloor.slagSpawns = new();
                yield return StartCoroutine(floorManager.DescendUnits(toDescend));
// Add pony to the drop list
                scenario.player.units.Insert(1, playerUnits[1]);
                floorManager.currentFloor.RemoveElement(scenario.player.units[1]);
                scenario.player.units[1].coord = new Vector2(6,1);
                yield return StartCoroutine(floorManager.DescendUnits( new List<GridElement> { scenario.player.units[1] }));
                
            break;
            case 1:
                EnemyManager prevEnemy = scenario.currentEnemy;
            
                yield return StartCoroutine(floorManager.TransitionFloors(true, false));
                yield return StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Descent, ScenarioManager.Scenario.Combat));
                scenario.player.nail.ToggleNailState(Nail.NailState.Falling);   
                yield return new WaitForSecondsRealtime(0.25f);
                
                scenario.player.units[2].coord = new Vector2(2,4);
                for (int i = scenario.player.units.Count - 1; i >= 0; i--) {
                    toDescend.Add(scenario.player.units[i]);
                }
                if (prevEnemy.units.Count > 0) toDescend.Add(prevEnemy.units[0]);
            
                yield return StartCoroutine(floorManager.DescendUnits(toDescend, prevEnemy));
                
// Find a valid coord that a player unit is not in
                bool validCoord = false;
                Vector2 spawn = Vector2.zero;

                while (!validCoord) {
                    validCoord = true;
                    
                    spawn = new Vector2(Random.Range(0,7), Random.Range(0,7));
                        
                    foreach(GridElement ge in floorManager.currentFloor.gridElements) 
                        if (ge.coord == spawn) validCoord = false;                    

                    if (floorManager.currentFloor.tiles.Find(sqr => sqr.coord == spawn).tileType == Tile.TileType.Blood) validCoord = false;
                }
// Add spike to the drop list
                scenario.player.units.Insert(2, playerUnits[2]);
                floorManager.currentFloor.RemoveElement(scenario.player.units[2]);
                scenario.player.units[2].coord = spawn;
                yield return StartCoroutine(floorManager.DescendUnits( new List<GridElement> { scenario.player.units[2] }));
            break;
            default:
                EnemyManager enemy = (EnemyManager)floorManager.currentFloor.enemy;
                if (floorManager.floors.Count - 1 > floorManager.currentFloor.index) {
                    if (!floorManager.floorSequence.ThresholdCheck()) {
                        floorManager.GenerateFloor();       
                    }

                    scenario.player.nail.ToggleNailState(Nail.NailState.Falling);   
                    yield return StartCoroutine(floorManager.TransitionFloors(true, false));
                    
                    yield return StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Descent, ScenarioManager.Scenario.Null));
                    yield return new WaitForSecondsRealtime(0.25f);
                    
    // Descend units from previous floor
                    yield return StartCoroutine(floorManager.DescendUnits(floorManager.floors[floorManager.currentFloor.index -1].gridElements, enemy));
                    
                    StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Enemy));
                } else 
                    yield return StartCoroutine(floorManager.TransitionPackets(enemy));
            break;
        }
        descents++;
        cont = true;
    }

    public IEnumerator EnemyTurn() {
        scenario.prevTurn = scenario.currentTurn;
        scenario.currentTurn = ScenarioManager.Turn.Enemy;
        scenario.player.StartEndTurn(false);

        
        if (scenario.prevTurn == ScenarioManager.Turn.Descent)
            yield return StartCoroutine(scenario.currentEnemy.TakeTurn(true));
        else {
            if (scenario.uiManager.gameObject.activeSelf)
                yield return StartCoroutine(scenario.messagePanel.PlayMessage(MessagePanel.Message.Antibody));
            if (!enemyBehavior && scenario.currentEnemy.units.Count > 0) {
                yield return StartCoroutine(EnemyBehavior());
                enemyBehavior = true;
            }
            yield return StartCoroutine(scenario.currentEnemy.TakeTurn(false));
        }
    }

    public void SwitchTurns() {
        StartCoroutine(EnemyTurn());
    }

    public void CheckAllDone() {
        if (enemyBehavior && 
            nailDamageEncountered &&
            undoEncountered && 
            enemySpawnEncountered &&
            objectivesEncountered &&
            bloodEncountered &&
            slotsEncountered)
            Debug.Log("Tutorial finished");
    }

    public static string ColorToRichText(string str, Color color, string font = "") {
        return "<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">" + str + "</color>";
    }

}
