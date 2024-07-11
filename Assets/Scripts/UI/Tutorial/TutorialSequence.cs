using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialSequence : MonoBehaviour {

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
    public FloorChunk tutorialPacket;
    
    public DialogueTooltip tooltip, brTooltip;
    public Animator screenFade;
    [HideInInspector] public string header, body;
    public bool blinking = false;
    List<Unit> playerUnits = new();

    [SerializeField] Color keyColor;

    int descents;

    [Header("GIF Serialization")]
    [SerializeField] RuntimeAnimatorController hittingTheNailAnim;
    [SerializeField] RuntimeAnimatorController  descentDamage, hittingEnemiesAnim, shieldAnim, anvilAnim, bigGrabAnim;

    [Header("Button Highlights")]
    [SerializeField] GameObject buttonHighlight;
    [SerializeField] Transform peekButton, undoButton;
    bool peeked = false;
    GameObject peekHighlight, undoHighlight;

    [Header("Gameplay Optional Tooltips")]
    bool enemyBehavior = false;
    public bool hittingEnemies = false, enemySpawnEncountered = false, undoEncountered = false, nailDamageEncountered = false, bloodEncountered = false, 
        collisionEncountered = false, slotsEncountered = false, sequenceEnd = false;


    public void Initialize(ScenarioManager manager) {
        scenario = manager;
        floorManager = scenario.floorManager;


        floorManager.floorSequence.currentThreshold = FloorChunk.PacketType.Tutorial;    
        floorManager.floorSequence.floorsTutorial = 3;
        floorManager.floorSequence.AddPacket(tutorialPacket);
        
        descents = 0;

        floorManager.floorSequence.StartPacket(tutorialPacket);
        floorManager.GenerateFloor(floorManager.floorSequence.GetFloor(true), true);
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
        scenario.player.units[0].ui.equipButtons[0].GetComponent<Animator>().SetBool("Tut", true);
        scenario.player.units[0].ui.equipButtons[0].GetComponent<GearTooltipTrigger>().enabled = false;
        
        scenario.player.units[1].EnableSelection(false);
        scenario.player.units[1].ui.equipButtons[0].GetComponent<Button>().interactable = false;
        scenario.player.units[1].ui.equipButtons[0].GetComponent<Button>().enabled = false;
        scenario.player.units[1].ui.equipButtons[0].GetComponent<Animator>().SetBool("Tut", true);
        scenario.player.units[1].ui.equipButtons[0].GetComponent<GearTooltipTrigger>().enabled = false;
        
        scenario.player.units[2].EnableSelection(false);
        scenario.player.units.RemoveAt(1); scenario.player.units.RemoveAt(1);

        UIManager.instance.peekButton.enabled = false;
        UIManager.instance.peekButton.animator.GetComponent<Animator>().SetBool("Tut", true);
        UIManager.instance.endTurnButton.enabled = false;
        UIManager.instance.endTurnButton.GetComponent<Animator>().SetBool("Tut", true);

        peeked = false;

        floorManager.currentFloor.RemoveElement(scenario.player.units[1]);
        yield return StartCoroutine(floorManager.DescendUnits(new List<GridElement>{ scenario.player.units[1] }));

        scenario.player.units[0].ui.equipButtons[0].GetComponent<Animator>().SetTrigger("Disabled");
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
            else if (scenario.player.units[0].moved && !oopsies) {
                yield return StartCoroutine(Oopsies(1));
            }
        }

        yield return new WaitForSecondsRealtime(0.5f);
        yield return StartCoroutine(SelectingTheHammer());
        yield return new WaitForSecondsRealtime(0.5f);
        yield return StartCoroutine(HittingTheNail());
        
        while (descents < 1) yield return null;
// yield Descent 1
        yield return StartCoroutine(DiggingDown());

// yield first enemy turn
        yield return StartCoroutine(EnemyTurn());

        yield return new WaitForSecondsRealtime(1.25f);
        yield return StartCoroutine(NailPriming());

        StartCoroutine(HittingAnEnemy());
        while (scenario.currentTurn != ScenarioManager.Turn.Enemy) yield return null;
        while (scenario.currentTurn != ScenarioManager.Turn.Player) yield return null;
        yield return new WaitForSecondsRealtime(0.5f);
        yield return StartCoroutine(PeekHeadsUp());

        while (descents < 2) yield return null;
// Descent 2
        
        yield return new WaitForSecondsRealtime(1.25f);

        yield return StartCoroutine(Gear());
        yield return scenario.StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Enemy));
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
        body = "Listen up, squish! We're on a big brain mission. Gotta feast on <b>" + ColorToRichText("tasty thoughts", keyColor) + "</b>, yeah?" + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }

        header = "";
        body = "Gotta dive deeper into the noggin. Down, down, where the <b>" + ColorToRichText("juice gets juicier", keyColor) + "</b>." + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);

        }
        
        header = "";
        body = "Big Slime upstairs will toss ya a <b>" + ColorToRichText("Slag unit", keyColor) + "</b>. Control it during our dig.";
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
        }
        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator ExplainMovement() {
        screenFade.gameObject.SetActive(true);

        body = "<b>" + ColorToRichText("Slags", keyColor) + "</b> can move on the floor. Select this one, move it. <b>" + ColorToRichText("Line it up with the Nail", keyColor) + "</b>." + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1 / Util.fps);

        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);

    }

    public IEnumerator SelectingTheHammer() {
        Vector3 prevPos = tooltip.transform.GetComponent<RectTransform>().anchoredPosition;
        tooltip.transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(-500, prevPos.y);
        header = "ARMING THE HAMMER";
        body = "<b>" + ColorToRichText("Hammer's our main tool", keyColor) + "</b>—use it to hit anything and everything. Arm it with the <b>" + ColorToRichText("button", keyColor) + "</b> in the bottom left.";
        tooltip.SetText(body, header, true);

        while (tooltip.GetComponentInChildren<DialogueTypewriter>().writing) yield return null;
        tooltip.skip = true;
        
        UIManager.instance.canvasAnim.SetTrigger("TutUnit");
        GameObject highlight = Instantiate(buttonHighlight, scenario.player.units[0].ui.equipButtons[1].button.transform.GetChild(0).transform);
        highlight.transform.localPosition = Vector3.zero; highlight.GetComponent<Animator>().SetBool("Active", true);
        highlight.GetComponent<Animator>().keepAnimatorStateOnDisable = true;

        while (scenario.player.units[0].selectedEquipment == null) yield return null;
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
        
        tooltip.transform.GetComponent<RectTransform>().anchoredPosition = prevPos;
        Destroy(highlight);
    }

    public IEnumerator HittingTheNail() {
        screenFade.gameObject.SetActive(true);

        header = "STRIKING THE NAIL";
        body = "<b>" + ColorToRichText("Chuck the Hammer straight", keyColor) + "</b> to smack the Nail, then pick the Slag for it to <b>" + ColorToRichText("bounce back", keyColor) + "</b> to." + '\n';
        tooltip.SetText(body, header, true, false, new List<RuntimeAnimatorController>{ hittingTheNailAnim });
        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }

        
        screenFade.SetTrigger("FadeOut");
        Vector3 prevPos = tooltip.transform.GetComponent<RectTransform>().anchoredPosition;
        float timer = 0;

        while (timer < 0.25f) {
            tooltip.transform.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(prevPos, prevPos + new Vector3(680, 0), timer/0.25f);
            timer += Time.deltaTime;
            yield return null;
        }
        tooltip.transform.GetComponent<RectTransform>().anchoredPosition = prevPos + new Vector3(680, 0);
              
       
        PlayerUnit pu = (PlayerUnit)scenario.player.units[0];
        while (pu.energyCurrent == 1) yield return new WaitForSecondsRealtime(1/Util.fps);
        tooltip.transform.GetComponent<RectTransform>().anchoredPosition = prevPos;
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator DiggingDown() {
        screenFade.gameObject.SetActive(true);
        
        header = "DIGGING DOWN";
        body = "Smash the <b>" + ColorToRichText("Nail", keyColor) + "</b> with the <b>" + ColorToRichText("Hammer", keyColor) + "</b>—the floor crumbles and all units <b>" + ColorToRichText("crash below", keyColor) + "</b>. That's how we go deeper, squish." + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
        }
        header = "DIGGING DOWN";
        body = "...and deal <b>" + ColorToRichText("a lot of damage", keyColor) + "</b>. <b>" + ColorToRichText("Crush", keyColor) + "</b> enemies below and <b>" + ColorToRichText("drop em' on hazards", keyColor) + "</b>. Units that land on something take damage too, so be careful with your Slags." + '\n';
        tooltip.SetText(body, header, true, false, new List<RuntimeAnimatorController> { descentDamage } );
        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator NailPriming() {
        screenFade.gameObject.SetActive(true);

        header = "NAIL PRIMING";
        body = "The Nail lands hard, takes a turn to prime. <b>" + ColorToRichText("Until the Nail is primed, it can't be hit", keyColor) + "</b> by the Hammer." + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator HittingAnEnemy() {
        screenFade.gameObject.SetActive(true);

        header = "HITTING ENEMIES";
        body = "Now you got two Slags, <b>" + ColorToRichText("share the Hammer", keyColor) + "</b>. Hit that enemy and <b>" + ColorToRichText("bounce the Hammer to the other Slag", keyColor) + "</b>." + '\n';
        tooltip.SetText(body, header, true, false, new List<RuntimeAnimatorController>{ hittingEnemiesAnim });

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
            if (scenario.player.units[0].energyCurrent == 0) {
                break;
            } else if (scenario.player.units[0].moved && scenario.player.units[0].coord.x != scenario.currentEnemy.units[0].coord.x && scenario.player.units[0].coord.y != scenario.currentEnemy.units[0].coord.y && !oopsies)
                yield return StartCoroutine(Oopsies(2));

        }

        Destroy(highlight);
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
        tooltip.transform.GetComponent<RectTransform>().anchoredPosition = prevPos;
        
        while (true) {
            if (scenario.player.units[1].energyCurrent == 0) break;
            if (scenario.player.units[0].energyCurrent == 0 && scenario.player.units[0].equipment.Find(e => e is HammerData) != null) break;
            yield return null;
        }
        yield return new WaitForSecondsRealtime(1f);
        yield return StartCoroutine(OnTurnMoveAndAP());

        tooltip.transform.GetChild(0).gameObject.SetActive(false);

    }

    public IEnumerator OnTurnMoveAndAP() {
        screenFade.gameObject.SetActive(true);

        header = "PLAYER TURN";
        body = "Each Slag can <b>" + ColorToRichText("move and act on their turn", keyColor) + "</b>. When you're all set, <b>" + ColorToRichText("end your turn", keyColor) + "</b> in the bottom right." + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
        }

        UIManager.instance.endTurnButton.GetComponent<Animator>().SetBool("Tut", false);
        UIManager.instance.endTurnButton.enabled = true;
        UIManager.instance.canvasAnim.SetTrigger("TutEndTurn");
        UIManager.instance.canvasAnim.SetBool("Active", true);

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }


    public IEnumerator Gear() {
        screenFade.gameObject.SetActive(true);

        header = "GEAR";
        body = "<b>" + ColorToRichText("Slags pack Gear", keyColor) + "</b>—special stuff. You can have them <b>" + ColorToRichText("use Gear or Hammer each turn", keyColor) + "</b>. Don't end up short on actions, squish." + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
        }
        float x = tooltip.contentField.GetComponent<RectTransform>().sizeDelta.x;
        tooltip.contentField.GetComponent<RectTransform>().sizeDelta = new Vector2(900, tooltip.contentField.GetComponent<RectTransform>().sizeDelta.y);
        header = "GEAR";
        body = "Each piece of Gear's useful for combat or to deal damage on descent. Check those <b>" + ColorToRichText("buttons", keyColor) + "</b> in the bottom left to get to know your arsenal." + '\n';
        tooltip.SetText(body, header, true, false, new List<RuntimeAnimatorController>{ shieldAnim, anvilAnim, bigGrabAnim });

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
        }
        tooltip.contentField.GetComponent<RectTransform>().sizeDelta = new Vector2(x, tooltip.contentField.GetComponent<RectTransform>().sizeDelta.y);
        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);

        scenario.player.units[0].ui.equipButtons[0].GetComponent<Button>().enabled = true;
        scenario.player.units[0].ui.equipButtons[0].GetComponent<Animator>().SetBool("Tut", false);
        scenario.player.units[0].ui.equipButtons[0].GetComponent<GearTooltipTrigger>().enabled = true;
        scenario.player.units[1].ui.equipButtons[0].GetComponent<Button>().enabled = true;
        scenario.player.units[1].ui.equipButtons[0].GetComponent<Animator>().SetBool("Tut", false);
        scenario.player.units[1].ui.equipButtons[0].GetComponent<GearTooltipTrigger>().enabled = true;
    }

    public IEnumerator EnemyBehavior() {
        screenFade.gameObject.SetActive(true);

        header = "ENEMY TURN";
        body = "<b>" + ColorToRichText("Enemy units ain't decoration", keyColor) + "</b>. Select enemies on your turn to learn about them in the bottom left." + '\n';
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
        body = "Playtime's over, squish. Time for the real deal. <b>" + ColorToRichText("Dig deep. Make mess", keyColor) + "</b>.";
        tooltip.SetText(body, header, true);
        sequenceEnd = true;

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1 / Util.fps);
        }
        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);

        if (!undoEncountered) StartCoroutine(ForcedOopsies());
    }

// Gameplay optional - tutorial specific - tooltips

    IEnumerator ForcedOopsies() {
        while (scenario.player.undoableMoves.Count <= 0) yield return null;
        StartCoroutine(Oopsies(3));
    }

    public IEnumerator NailDamage() {
        nailDamageEncountered = true;
        while (ScenarioManager.instance.currentTurn != ScenarioManager.Turn.Player) {
            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.25f);
        screenFade.gameObject.SetActive(true);

        header = "NAIL DAMAGE";
        body = "Watch out, squish! <b>" + ColorToRichText("If the Nail dies, we're done", keyColor) + "</b>. The Nail strikes back when attacked, but don't count on it!" + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
        CheckAllDone();
    }

    IEnumerator PeekHeadsUp() {
        UIManager.instance.peekButton.enabled = true;
        UIManager.instance.peekButton.animator.GetComponent<Animator>().SetBool("Tut", false);

        screenFade.gameObject.SetActive(true);
        header = "PEEK AHEAD";
        body = "Listen up, squish, this is <b>" + ColorToRichText("important", keyColor) + "</b>. It's time to use the <b>" + ColorToRichText("peek button", keyColor) + "</b>. Big ol' eye <b>" + ColorToRichText("button", keyColor) + "</b> in the bottom right.";
        brTooltip.SetText(body, header, true);

        while (!brTooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
        }

        peekHighlight = Instantiate(buttonHighlight, peekButton);
        peekHighlight.transform.SetSiblingIndex(0); peekHighlight.transform.localPosition = Vector3.zero; peekHighlight.GetComponent<Animator>().SetBool("Active", true);
        peekHighlight.GetComponent<Animator>().keepAnimatorStateOnDisable = true;

        screenFade.SetTrigger("FadeOut");
        brTooltip.transform.GetChild(0).gameObject.SetActive(false);
    
    }

    public IEnumerator PeekButton() {
        if (peekHighlight)
            Destroy(peekHighlight);
        header = "PEEK BUTTON";
        body = "This let's you <b>" + ColorToRichText("preview the next floor", keyColor) + "</b> and <b>" + ColorToRichText("where units will land", keyColor) + "</b>. Look before you leap, squish." + '\n';
        brTooltip.SetText(body, header, true);
        peeked = true;

        while (floorManager.transitioning) yield return null;
        yield return new WaitForSecondsRealtime(0.5f);
        while (!brTooltip.skip && floorManager.peeking) {
            yield return new WaitForSecondsRealtime(1/Util.fps);   
        }
        

        brTooltip.skip = true;
        brTooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    bool oopsies;
    IEnumerator Oopsies(int trigger) {
        oopsies = true;
        screenFade.gameObject.SetActive(true);
        if (trigger == 1) {
            header = "OOPSIES";
            body = "Use your noggin, squish! <b>" + ColorToRichText("Slide your Slag in line with the nail", keyColor) + "</b> Got a problem? <b>" + ColorToRichText("Fix it in the bottom right", keyColor) + "</b>." + '\n';    
        } else if (trigger == 2) {
            header = "OOPSIES";
            body = "Use your noggin, squish! <b>" + ColorToRichText("Slide your Slag in line with the enemy", keyColor) + "</b> Got a problem? <b>" + ColorToRichText("Fix it in the bottom right", keyColor) + "</b>." + '\n';    
        } else if (trigger == 3) {
            header = "OOPSIES";
            body = "Make an oopsie? You can <b>" + ColorToRichText("undo", keyColor) + "</b> your <b>" + ColorToRichText("Slag's movement", keyColor) + "</b> with the button in the bottom right." + '\n';    
        }
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);

        if (trigger == 1)
            UIManager.instance.canvasAnim.SetTrigger("TutUndoFirst");
        if (trigger == 2) {
            UIManager.instance.canvasAnim.SetTrigger("TutEndTurn");
        }

        undoHighlight = Instantiate(buttonHighlight, undoButton);
        undoHighlight.transform.SetSiblingIndex(0); undoHighlight.transform.localPosition = Vector3.zero; undoHighlight.GetComponent<Animator>().SetBool("Active", true);
        undoHighlight.GetComponent<Animator>().keepAnimatorStateOnDisable = true;

        yield return null;

        UIManager.instance.canvasAnim.SetBool("Active", true);
    }

    public IEnumerator UndoTutorial() {
        if (header != "UNDO BUTTON") {
            if (undoHighlight)
                Destroy(undoHighlight);
            header = "UNDO BUTTON";
            body = "Slags got an <b>" + ColorToRichText("Undo button", keyColor) + "</b>. Move around and reset, but once you take an action, no backtracking. Think ahead, squish." + '\n';
            brTooltip.SetText(body, header, true);

            while (!brTooltip.skip) {
                yield return new WaitForSecondsRealtime(1/Util.fps);
            }
        }

        brTooltip.skip = true;
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
        body = "Enemies won't give up. Wait too long on a floor, <b>" + ColorToRichText("more will drop in", keyColor) + "</b>. Better dive deeper to avoid an ambush." + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
        CheckAllDone();
    }
    public IEnumerator BloodTiles() {
        while (ScenarioManager.instance.currentTurn != ScenarioManager.Turn.Player) {
            yield return null;
        }
        screenFade.gameObject.SetActive(true);
        bloodEncountered = true;

        header = "BLOOD TILES";
        body = "<b>" + ColorToRichText("Blood tiles", keyColor) + "</b>? Check the <b>" + ColorToRichText("top right", keyColor) + "</b> when hovering on a tile for more info." + '\n';
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
                scenario.player.nail.ToggleNailState(Nail.NailState.Falling);   
                yield return StartCoroutine(floorManager.TransitionFloors(true, false));
                yield return StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Descent, ScenarioManager.Scenario.Combat));
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

                scenario.player.nail.ToggleNailState(Nail.NailState.Falling);   
                yield return StartCoroutine(floorManager.TransitionFloors(true, false));
                yield return StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Descent, ScenarioManager.Scenario.Combat));
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
                if (!peeked) peekHighlight.SetActive(false);

                yield return StartCoroutine(floorManager.DescendUnits( new List<GridElement> { scenario.player.units[2] }));
            break;
            default:
                EnemyManager enemy = (EnemyManager)floorManager.currentFloor.enemy;
                foreach (Unit u in playerUnits) {
                    u.StartCoroutine(u.TakeDamage(u.hpCurrent - u.hpMax, GridElement.DamageType.Heal));
                }
                if (!peeked) peekHighlight.SetActive(true);
                floorManager.floorSequence.ThresholdCheck();
                yield return StartCoroutine(floorManager.TransitionPackets(enemy));
            break;
        }
        descents++;
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
            bloodEncountered &&
            slotsEncountered)
            Debug.Log("Tutorial finished");
    }

    public static string ColorToRichText(string str, Color color, string font = "") {
        return "<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">" + str + "</color>";
    }

}
