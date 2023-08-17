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

    int descents;

    [Header("GIF Serialization")]
    [SerializeField] RuntimeAnimatorController hittingTheNailAnim;
    [SerializeField] RuntimeAnimatorController hittingEnemiesAnim, anvilAnim, bigThrowAnim, shieldAnim, bulbAnim, basophicAnim, reviveAnim;

    [Header("Button Highlights")]
    [SerializeField] GameObject buttonHighlight;
    [SerializeField] Transform peekButton, undoButton;
    GameObject destroyHighlight;

    [Header("Gameplay Optional Tooltips")]
    bool enemyBehavior = false;
    public bool hittingEnemies, undoEncountered, nailDamageEncountered, bloodEncountered, collisionEncountered, bulbEncountered, basophicEncountered, deathReviveEncountered, slotsEncountered = false;
    List<Coroutine> tooltipCoroutines = new List<Coroutine>();


    public void Initialize(ScenarioManager manager) {
        scenario = manager;
        floorManager = scenario.floorManager;

        floorManager.floorSequence.currentThreshold = FloorPacket.PacketType.Tutorial;    
        floorManager.floorSequence.floorsTutorial = 3;
        floorManager.floorSequence.localPackets.Add(tutorialPacket);
        
        hittingEnemies = false; enemyBehavior = false; undoEncountered = false; bulbEncountered = false; deathReviveEncountered = false; slotsEncountered = false;
        descents = 0;

        floorManager.GenerateFloor(floorManager.floorSequence.GetFloor(true), true);
        floorManager.GenerateFloor(floorManager.floorSequence.GetFloor(true));
    }
    
    public IEnumerator Tutorial() {
        for (int i = 0; i <= scenario.player.units.Count - 1; i++) {
            playerUnits.Add(scenario.player.units[i]);
            if (scenario.player.units[i] is PlayerUnit pu) {
                pu.ElementDisabled += StartDeathTut;
            }
        }
        scenario.player.units[1].ui.overview.gameObject.SetActive(false); scenario.player.units[2].ui.overview.gameObject.SetActive(false);
        scenario.player.units.RemoveAt(1); scenario.player.units.RemoveAt(1);


        yield return StartCoroutine(floorManager.DescendUnits(new List<GridElement>{ scenario.player.units[1] }));
        //foreach (Unit unit in scenario.player.units) unit.gameObject.SetActive(false);
        yield return new WaitForSecondsRealtime(0.15f);
        scenario.player.nail.ToggleNailState(Nail.NailState.Primed);
        yield return new WaitForSecondsRealtime(0.75f);
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

        header = "WELCOME to DEEPMESS";
        body = "I am Bubbletack, an ancillary of the Slimemind that clings above. We've been making good progress through the thick skull of the Grand Designer, unraveling the secrets of the head as we go." + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }
        
        header = "";
        body = "We'll split off some more egregore for you to control, three mortal Slags to wield the tools of this depraved excavation. It's time to dig deep and make a mess.";
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
        }
        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator ExplainMovement() {
        screenFade.gameObject.SetActive(true);

        body = "This is one of our Slags, Flathead. It can move around the floor and use the Hammer to strike me and the Nail, sending everything down to the next floor. Click on the Slag to move it in line with the Nail." + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);

    }

    public IEnumerator HittingTheNail() {
        screenFade.gameObject.SetActive(true);

        header = "Hitting the Nail";
        body = "The Hammer is our main tool. Throw it in a straight line to strike a target, then select a Slag for the Hammer to bounce back to. Strike the Nail with the Hammer and catch it with the Slag." + '\n';
        tooltip.SetText(body, header, true, new List<RuntimeAnimatorController>{ hittingTheNailAnim });
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
              
       
        GameObject highlight = Instantiate(buttonHighlight, scenario.player.units[0].ui.equipButtons[1].gameObject.transform);
        highlight.transform.SetSiblingIndex(0); highlight.transform.localPosition = Vector3.zero; highlight.GetComponent<Animator>().SetBool("Active", true);
        highlight.GetComponent<Animator>().keepAnimatorStateOnDisable = true;
        PlayerUnit pu = (PlayerUnit)scenario.player.units[0];
        while (pu.hammerUses == 0) yield return new WaitForSecondsRealtime(1/Util.fps);
        tooltip.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition = prevPos;
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
        Destroy(highlight);

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

    public IEnumerator NailPriming() {
        screenFade.gameObject.SetActive(true);

        header = "Nail Priming";
        body = "Since I'm wrapped around the Nail, I need some time to get ready for another descent. When the Nail is not primed, it can't be hit by the Hammer." + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator HittingAnEnemy() {
        screenFade.gameObject.SetActive(true);

        header = "Hitting Enemies";
        body = "The Hammer can be bounced between Slags. Strike an enemy and select Spike, the other Slag, to bounce it to." + '\n';
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
        GameObject highlight = Instantiate(buttonHighlight, scenario.player.units[0].ui.equipButtons[1].gameObject.transform);
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

        header = "Player Turn: Move and Action";
        body = "On your turn, each Slag can move and take an action. Descending down to the next floor ends your turn." + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }


    public IEnumerator Equipment() {
        screenFade.gameObject.SetActive(true);

        header = "Equipment";
        body = "Slags' equipment can be used once per floor. Each Slag has a unique ability that can give you a big advantage on the current floor or the one below." + '\n';
        tooltip.SetText(body, header, true, new List<RuntimeAnimatorController>{ anvilAnim, bigThrowAnim, shieldAnim });

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);

    }

    public IEnumerator EnemyBehavior() {
        screenFade.gameObject.SetActive(true);

        header = "Enemy Behavior";
        body = "Enemies can move and attack on their turn. These Monophics can move 2 tiles and strike anything next to them." + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
        CheckAllDone();
    }

    public IEnumerator ScatterTurn() {
        while (ScenarioManager.instance.currentTurn != ScenarioManager.Turn.Player) {
            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.25f);
        screenFade.gameObject.SetActive(true);

        header = "Enemy Scatter";
        body = "When you land on a floor, enemies scatter but won't attack." + '\n';
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

    public IEnumerator Basophic() {
        Debug.Log("Basophic");
        basophicEncountered = true;
        while (ScenarioManager.instance.currentTurn != ScenarioManager.Turn.Player) {
            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.25f);
        screenFade.gameObject.SetActive(true);

        header = "Basophic Enemy";
        body = "This Basophic enemy can explode, dealing damage to all the tiles around it. Hover over enemy portraits to learn more about their abilities." + '\n';
        tooltip.SetText(body, header, true, new List<RuntimeAnimatorController>{ basophicAnim });

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
        CheckAllDone();
    }

    public IEnumerator NailDamage() {
        nailDamageEncountered = true;
        while (ScenarioManager.instance.currentTurn != ScenarioManager.Turn.Player) {
            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.25f);
        screenFade.gameObject.SetActive(true);

        header = "Nail Damage";
        body = "I deal damage back to enemies that strike the Nail." + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
        CheckAllDone();
    }

    public IEnumerator TileBulb() {
        Debug.Log("Tile bulb");
        bulbEncountered = true;
        while (ScenarioManager.instance.currentTurn != ScenarioManager.Turn.Player) {
            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.25f);
        screenFade.gameObject.SetActive(true);

        header = "Bulbs";
        body = "Bulbs are consumable items that your Slags can pick up. Each Slag can hold 1 bulb that can be used by that Slag or thrown as a free action." + '\n';
        tooltip.SetText(body, header, true, new List<RuntimeAnimatorController>{ bulbAnim });

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
        CheckAllDone();
    }

    public IEnumerator PeekButton() {
        
        Destroy(destroyHighlight);
        header = "Peek Button";
        body = "The peek button lets you preview the next floor. You can see where enemies and other hazards are located." + '\n';
        brTooltip.SetText(body, header, true);

        while (!brTooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }

        brTooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator BloodTiles() {
        screenFade.gameObject.SetActive(true);
        bloodEncountered = true;

        header = "Blood Tiles";
        body = "Blood tiles prevent Slags from using the Hammer or equipment while the Slag is standing on it." + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
        CheckAllDone();
    }
    
    public IEnumerator UndoTutorial() {
        
        header = "Undo Button";
        body = "You can undo any Slags' movement. Once any Slag performs an action, however, you can't undo any previous moves. Plan accordingly." + '\n';
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

        header = "Descent Damage";
        body = "Slags and enemies crush anything they land on, but take damage as a result." + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
        CheckAllDone();
    }

    void StartDeathTut(GridElement blank) {
        if (!deathReviveEncountered)
            StartCoroutine(DeathRevivTut());
    }

     public IEnumerator DeathRevivTut() {
        deathReviveEncountered = true;

        while (ScenarioManager.instance.currentTurn != ScenarioManager.Turn.Player) {
            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.25f);

        screenFade.gameObject.SetActive(true);

        header = "Unit Revive";
        body = "Slags that have been downed can brought back into the fight. Strike the downed Slag with the Hammer to transfer 1HP from me to the Slag. It will come back with its move and action refreshed." + '\n';
        tooltip.SetText(body, header, true, new List<RuntimeAnimatorController>{ reviveAnim });

        while (!tooltip.skip) 
            yield return new WaitForSecondsRealtime(1/Util.fps);   

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
                scenario.player.units[1].ui.overview.gameObject.SetActive(true);
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
                scenario.player.units[2].ui.overview.gameObject.SetActive(true);
                floorManager.currentFloor.RemoveElement(scenario.player.units[2]);
                yield return StartCoroutine(floorManager.DescendUnits( new List<GridElement> { scenario.player.units[2] }));
                yield return new WaitForSecondsRealtime(0.15f);
                floorManager.floorSequence.ThresholdCheck();
            break;
            case 2:
                StartCoroutine(ScatterTurn());
                yield return floorManager.StartCoroutine(floorManager.TransitionPackets());
                PersistentMenu.instance.musicController.SwitchMusicState(MusicController.MusicState.Game, true);
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
            basophicEncountered && 
            bulbEncountered &&
            deathReviveEncountered &&
            slotsEncountered)
            Destroy(gameObject);
    }


}
