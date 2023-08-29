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
    List<Unit> playerUnits = new List<Unit>();

    [SerializeField] Color keyColor;

    int descents;

    [Header("GIF Serialization")]
    [SerializeField] RuntimeAnimatorController hittingTheNailAnim;
    [SerializeField] RuntimeAnimatorController hittingEnemiesAnim, shieldAnim;

    [Header("Button Highlights")]
    [SerializeField] GameObject buttonHighlight;
    [SerializeField] Transform peekButton;
    GameObject destroyHighlight;

    [Header("Gameplay Optional Tooltips")]
    bool enemyBehavior = false;
    public bool hittingEnemies = false, undoEncountered = false, nailDamageEncountered = false, bloodEncountered = false, collisionEncountered = false, slotsEncountered = false;


    public void Initialize(ScenarioManager manager) {
        scenario = manager;
        floorManager = scenario.floorManager;


        floorManager.floorSequence.currentThreshold = FloorPacket.PacketType.Tutorial;    
        floorManager.floorSequence.floorsTutorial = 3;
        floorManager.floorSequence.localPackets.Add(tutorialPacket);
        
        descents = 0;

        floorManager.GenerateFloor(floorManager.floorSequence.GetFloor(true), true);
        floorManager.GenerateFloor(floorManager.floorSequence.GetFloor(true));
    }
    
    public IEnumerator Tutorial() {
        for (int i = 0; i <= scenario.player.units.Count - 1; i++)
            playerUnits.Add(scenario.player.units[i]);
        //scenario.player.units[1].ui.overview.gameObject.SetActive(false); scenario.player.units[2].ui.overview.gameObject.SetActive(false);
        scenario.player.units.RemoveAt(1); scenario.player.units.RemoveAt(1);


        yield return StartCoroutine(floorManager.DescendUnits(new List<GridElement>{ scenario.player.units[1] }));
        //foreach (Unit unit in scenario.player.units) unit.gameObject.SetActive(false);
        yield return new WaitForSecondsRealtime(0.5f);
        scenario.player.nail.ToggleNailState(Nail.NailState.Primed);
        yield return new WaitForSecondsRealtime(1.25f);
        yield return StartCoroutine(SplashMessage());
        
        yield return StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Descent, ScenarioManager.Scenario.Combat));
        floorManager.currentFloor.RemoveElement(scenario.player.units[0]);
        yield return StartCoroutine(floorManager.DescendUnits(new List<GridElement> { scenario.player.units[0] }));
        yield return new WaitForSecondsRealtime(0.15f);

        yield return StartCoroutine(ExplainMovement());
        yield return scenario.StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Player));

        while (true) {
            yield return null;
            if (scenario.player.units[0].coord.x == scenario.player.nail.coord.x || scenario.player.units[0].coord.y == scenario.player.nail.coord.y)
                break;
        }

        yield return new WaitForSecondsRealtime(0.5f);
        yield return StartCoroutine(HittingTheNail());
        cont = false;

        while (!cont) yield return null;
// Descent 1
        yield return StartCoroutine(DiggingDown());
        yield return StartCoroutine(EnemyTurn());

        yield return new WaitForSecondsRealtime(1.25f);
        yield return StartCoroutine(NailPriming());

        Coroutine co = StartCoroutine(AttackingEnemies());

        cont = false;
        while (!cont) yield return null;
// Descent 3

        yield return StartCoroutine(EnemyTurn());
        StopCoroutine(co);
        yield return new WaitForSecondsRealtime(0.25f);
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
        body = "We are inside the top layers of <b>" + ColorToRichText("GOD'S", keyColor) + "</b> head. Our <b>" + ColorToRichText("PURPOSE", keyColor) + "</b> is buried deep here. We need you to <b>" + ColorToRichText("DIG", keyColor) + "</b>." + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }
        
        header = "";
        body = "We will send you a <b>" + ColorToRichText("SLAG", keyColor) + "</b> unit to control during our excavation.";
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
        }
        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator ExplainMovement() {
        screenFade.gameObject.SetActive(true);

        body = "<b>" + ColorToRichText("SLAGS", keyColor) + "</b> can move around the floor. Select the Slag and move it. <b>" + ColorToRichText("LINE IT UP WITH THE NAIL.", keyColor) + "</b>" + '\n';
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
        body = "The <b>" + ColorToRichText("HAMMER", keyColor) + "</b> is our main tool. Select it in the bottom left corner of your screen and throw it in a <b>" + ColorToRichText("STRAIGHT LINE", keyColor) + "</b> to strike a target. Then select a <b>" + ColorToRichText("SLAG", keyColor) + "</b> for the Hammer to bounce back to. <b>" + ColorToRichText("STRIKE THE NAIL", keyColor) + "</b>" + '\n';
        tooltip.SetText(body, header, true, new List<RuntimeAnimatorController>{ hittingTheNailAnim });
        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }

        
        screenFade.SetTrigger("FadeOut");
        Vector3 prevPos = tooltip.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition;
        float timer = 0;

        while (timer < 0.25f) {
            tooltip.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(prevPos, prevPos + new Vector3(680, 0), timer/0.25f);
            timer += Time.deltaTime;
            yield return null;
        }
        tooltip.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition = prevPos + new Vector3(680, 0);
              
       
        GameObject highlight = Instantiate(buttonHighlight, scenario.player.units[0].ui.equipButtons[0].gameObject.transform);
        highlight.transform.SetSiblingIndex(0); highlight.transform.localPosition = Vector3.zero; highlight.GetComponent<Animator>().SetBool("Active", true);
        highlight.GetComponent<Animator>().keepAnimatorStateOnDisable = true;
        PlayerUnit pu = (PlayerUnit)scenario.player.units[0];
        while (pu.hammerUses == 0) yield return new WaitForSecondsRealtime(1/Util.fps);
        tooltip.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition = prevPos;
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
        Destroy(highlight);

    }

    public IEnumerator DiggingDown() {
        screenFade.gameObject.SetActive(true);
        
        header = "DIGGING DOWN";
        body = "Striking the <b>" + ColorToRichText("NAIL", keyColor) + "</b> with the <b>" + ColorToRichText("HAMMER", keyColor) + "</b> destroys the floor. <b>" + ColorToRichText("ALL UNITS CRASH BELOW", keyColor) + "</b>. We progress." + '\n';
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
        body = "We need time to get the <b>" + ColorToRichText("NAIL", keyColor) + "</b> ready for another descent. When the Nail is not primed, <b>" + ColorToRichText("IT CANNOT BE HIT BY THE HAMMER", keyColor) + "</b>" + '\n';
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
        body = "The <b>" + ColorToRichText("HAMMER", keyColor) + "</b> can be <b>" + ColorToRichText("BOUNCED BETWEEN SLAGS", keyColor) + "</b>. Strike the enemy and select the <b>" + ColorToRichText("OTHER SLAG", keyColor) + "</b> to bounce it to. If no enemies remain on the floor, a descent is forced." + '\n';
        tooltip.SetText(body, header, true, new List<RuntimeAnimatorController>{ hittingEnemiesAnim });

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
        }
        screenFade.SetTrigger("FadeOut");
        Vector3 prevPos = tooltip.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition;
        float timer = 0;

        while (timer < 0.25f) {
            tooltip.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(prevPos, prevPos + new Vector3(680, 0), timer/0.25f);
            Debug.Log(prevPos + " " + tooltip.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition);
            timer += Time.deltaTime;
            yield return null;
        }

        tooltip.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition = prevPos + new Vector3(680, 0);
        GameObject highlight = Instantiate(buttonHighlight, scenario.player.units[0].ui.equipButtons[0].gameObject.transform);
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
        tooltip.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition = prevPos;
        yield return new WaitForSecondsRealtime(1.5f);
        yield return StartCoroutine(OnTurnMoveAndAP());

        tooltip.transform.GetChild(0).gameObject.SetActive(false);

    }

    public IEnumerator OnTurnMoveAndAP() {
        screenFade.gameObject.SetActive(true);

        header = "PLAYER TURN";
        body = "<b>" + ColorToRichText("SLAGS", keyColor) + "</b> can move and take an action on each turn. <b>" + ColorToRichText("DESCENDING", keyColor) + "</b> to the next floor <b>" + ColorToRichText("STARTS A NEW TURN", keyColor) + "</b>." + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }


    public IEnumerator Equipment() {
        screenFade.gameObject.SetActive(true);

        header = "EQUIPMENT";
        body = "Your first equipment is the <b>" + ColorToRichText("SHIELD", keyColor) + "</b>. Erect a Shield around any <b>" + ColorToRichText("SLAG", keyColor) + "</b> or the <b>" + ColorToRichText("NAIL", keyColor) + "</b>, protecting it from damage. Equipment can be used <b>" + ColorToRichText("ONCE PER FLOOR", keyColor) + "</b>." + '\n';
        tooltip.SetText(body, header, true, new List<RuntimeAnimatorController>{ shieldAnim });

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);

        GameObject highlight = Instantiate(buttonHighlight, scenario.player.units[2].ui.equipButtons[0].gameObject.transform);
        highlight.transform.parent.transform.parent.transform.parent.transform.parent.gameObject.SetActive(true);
        highlight.transform.SetSiblingIndex(0); highlight.transform.localPosition = Vector3.zero; highlight.GetComponent<Animator>().SetBool("Active", true);
        highlight.GetComponent<Animator>().keepAnimatorStateOnDisable = true;
        highlight.transform.parent.transform.parent.transform.parent.transform.parent.gameObject.SetActive(false);
        StartCoroutine(OnShieldUse(highlight));
    }

    public IEnumerator OnShieldUse(GameObject highlight) {
        Debug.Log(floorManager.currentFloor);
        while (!scenario.player.units[2].usedEquip && floorManager.currentFloor.index == 2) yield return null;
        Destroy(highlight);

    }
    public IEnumerator EnemyBehavior() {
        screenFade.gameObject.SetActive(true);

        header = "ENEMY TURN";
        body = "<b>" + ColorToRichText("ENEMY UNITS", keyColor) + "</b> can move and attack on their turn. These enemies can <b>" + ColorToRichText("MOVE 2 TILES", keyColor) + "</b> and <b>" + ColorToRichText("STRIKE ANYTHING ADJACENT", keyColor) + "</b>." + '\n';
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
        body = "You will unlock more equipment as you breach cavities on our descent. We need to get through to the <b>" + ColorToRichText("15th FLOOR", keyColor) + "</b>. It is time. <b>" + ColorToRichText("DIG DEEP. MAKE MESS.", keyColor) + "</b>";
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1 / Util.fps);
        }
        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }


// Gameplay optional - tutorial specific - tooltips
    public IEnumerator ScatterTurn() {
        while (ScenarioManager.instance.currentTurn != ScenarioManager.Turn.Player) {
            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.25f);
        screenFade.gameObject.SetActive(true);

        header = "ENEMY SCATTER";
        body = "When we land on a floor, <b>" + ColorToRichText("ENEMIES SCATTER", keyColor) + "</b> but do not attack." + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
        destroyHighlight = Instantiate(buttonHighlight, peekButton);
        destroyHighlight.transform.SetSiblingIndex(0); destroyHighlight.transform.localPosition = Vector3.zero; destroyHighlight.GetComponent<Animator>().SetBool("Active", true);
        destroyHighlight.GetComponent<Animator>().keepAnimatorStateOnDisable = true;
    }  

    public IEnumerator NailDamage() {
        nailDamageEncountered = true;
        while (ScenarioManager.instance.currentTurn != ScenarioManager.Turn.Player) {
            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.25f);
        screenFade.gameObject.SetActive(true);

        header = "NAIL DAMAGE";
        body = "We deal damage back to enemies that <b>" + ColorToRichText("ATTACK THE NAIL", keyColor) + "</b>." + '\n';
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
        body = "The <b>" + ColorToRichText("PEEK BUTTON", keyColor) + "</b> [space] lets you preview the next floor. You can see where enemies and other hazards are located." + '\n';
        brTooltip.SetText(body, header, true);

        while (!brTooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }

        brTooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator BloodTiles() {
        screenFade.gameObject.SetActive(true);
        bloodEncountered = true;

        header = "BLOOD TILES";
        body = "Blood tiles <b>" + ColorToRichText("PREVENT SLAGS", keyColor) + "</b> from using the <b>" + ColorToRichText("HAMMER", keyColor) + "</b> or <b>" + ColorToRichText("EQUIPMENT", keyColor) + "</b> while standing in it. View tile information in the top right when hovering over a tile" + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
        CheckAllDone();
    }
    
    public IEnumerator UndoTutorial() {
        
        header = "UNDO BUTTON";
        body = "You can <b>" + ColorToRichText("UNDO", keyColor) + "</b> [z] any <b>" + ColorToRichText("SLAG'S MOVEMENT", keyColor) + "</b>. Once any Slag performs an action, you cannot undo any <b>" + ColorToRichText("PREVIOUS MOVES", keyColor) + "</b>." + '\n';
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

    public IEnumerator DescentDamage() {
        collisionEncountered = true;

        while (ScenarioManager.instance.currentTurn != ScenarioManager.Turn.Player) {
            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.25f);
        screenFade.gameObject.SetActive(true);

        header = "DESCENT DAMAGE";
        body = "<b>" + ColorToRichText("SLAGS", keyColor) + "</b> and <b>" + ColorToRichText("ENEMIES CRUSH", keyColor) + "</b> anything they land on. They take damage as a result." + '\n';
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
                yield return StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Descent, ScenarioManager.Scenario.Combat));
                floorManager.GenerateFloor(floorManager.floorSequence.GetFloor(true));
                yield return StartCoroutine(floorManager.TransitionFloors(true, false));
                scenario.player.nail.ToggleNailState(Nail.NailState.Falling);   
                yield return new WaitForSecondsRealtime(0.25f);
                
                for (int i = scenario.player.units.Count - 1; i >= 0; i--) 
                    toDescend.Add(scenario.player.units[i]);

                floorManager.currentFloor.slagSpawns = new();
                yield return StartCoroutine(floorManager.DescendUnits(toDescend));
                scenario.player.units.Insert(1, playerUnits[1]);
                //scenario.player.units[1].ui.overview.gameObject.SetActive(true);
                floorManager.currentFloor.RemoveElement(scenario.player.units[1]);
                floorManager.currentFloor.slagSpawns.Add(floorManager.currentFloor.lvlDef.initSpawns.Find(s => s.asset.prefab.GetComponent<PlayerUnit>()).coord);
                yield return StartCoroutine(floorManager.DescendUnits( new List<GridElement> { scenario.player.units[1] }));
                scenario.currentTurn = ScenarioManager.Turn.Descent;
                
            break;
            case 1:
                yield return StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Descent, ScenarioManager.Scenario.Combat));
                EnemyManager prevEnemy = scenario.currentEnemy;

                yield return StartCoroutine(floorManager.TransitionFloors(true, false));
                scenario.player.nail.ToggleNailState(Nail.NailState.Falling);   
                yield return new WaitForSecondsRealtime(0.25f);

                for (int i = scenario.player.units.Count - 1; i >= 0; i--) {
                    toDescend.Add(scenario.player.units[i]);
                }
                if (prevEnemy.units.Count > 0) toDescend.Add(prevEnemy.units[0]);
            
                yield return StartCoroutine(floorManager.DescendUnits(toDescend, prevEnemy));
                
                bool validCoord = false;
                Vector2 spawn = Vector2.zero;

        // Find a valid coord that a player unit is not in
                while (!validCoord) {
                    validCoord = true;
                    
                    spawn = new Vector2(Random.Range(0,7), Random.Range(0,7));
                        
                    foreach(GridElement ge in floorManager.currentFloor.gridElements) 
                        if (ge.coord == spawn) validCoord = false;                    

                    if (floorManager.currentFloor.tiles.Find(sqr => sqr.coord == spawn).tileType == Tile.TileType.Blood) validCoord = false;
                }
                
                floorManager.currentFloor.slagSpawns = new List<Vector2>{ spawn };
                
                scenario.player.units.Insert(2, playerUnits[2]);
                //scenario.player.units[2].ui.overview.gameObject.SetActive(true);
                floorManager.currentFloor.RemoveElement(scenario.player.units[2]);
                yield return StartCoroutine(floorManager.DescendUnits( new List<GridElement> { scenario.player.units[2] }));
                yield return new WaitForSecondsRealtime(0.15f);
                floorManager.floorSequence.ThresholdCheck();
            break;
            case 2:
                Coroutine co = floorManager.StartCoroutine(floorManager.TransitionPackets());
                PersistentMenu.instance.musicController.SwitchMusicState(MusicController.MusicState.Game, true);
                yield return new WaitForSecondsRealtime(2f);
                yield return StartCoroutine(TutorialEnd());
                StartCoroutine(ScatterTurn());
                yield return co;
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
            bloodEncountered &&
            slotsEncountered)
            Debug.Log("Tutorial finished");
    }

    public static string ColorToRichText(string str, Color color, string font = "") {
        return "<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">" + str + "</color>";
    }

}
