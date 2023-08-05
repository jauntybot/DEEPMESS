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
    
    public DialogueTooltip tooltip;
    public Animator screenFade;
    [HideInInspector] public string header, body;
    public bool blinking = false;
    bool cont = false;
    bool enemyBehavior = false;

    [Header("GIF Serialization")]
    [SerializeField] RuntimeAnimatorController hittingTheNailAnim;
    [SerializeField] RuntimeAnimatorController hittingEnemiesAnim, anvilAnim, bigThrowAnim, shieldAnim;

    [Header("Button Highlights")]
    [SerializeField] GameObject buttonHighlight;
    [SerializeField] Transform peekButton, undoButton;

    public void Initialize(ScenarioManager manager) {
        scenario = manager;
        floorManager = scenario.floorManager;
        GetComponent<LaterTutorials>().StartListening(scenario.player);

        floorManager.floorSequence.currentThreshold = FloorPacket.PacketType.Tutorial;    
        floorManager.floorSequence.floorsTutorial = 3;
        floorManager.floorSequence.localPackets.Add(tutorialPacket);
        
        enemyBehavior = false;
    }
    
    public IEnumerator Tutorial() {
        yield return StartCoroutine(floorManager.DescendUnits(new List<GridElement>{ scenario.player.units[3] }));
        //foreach (Unit unit in scenario.player.units) unit.gameObject.SetActive(false);
        yield return new WaitForSecondsRealtime(0.15f);
        scenario.player.nail.ToggleNailState(Nail.NailState.Primed);
        yield return StartCoroutine(GenerateNextTutorialFloor(floorManager.floorSequence.GetFloor(true)));
        yield return new WaitForSecondsRealtime(0.15f);

        yield return StartCoroutine(SplashMessage());
        
        yield return StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Descent, ScenarioManager.Scenario.Combat));
        floorManager.currentFloor.RemoveElement(scenario.player.units[0]);
        floorManager.playerDropOverrides = new List<Vector2>{ tutorialPacket.firstFloors[0].initSpawns.Find(s => s.asset.prefab.GetComponent<PlayerUnit>()).coord };
        yield return StartCoroutine(floorManager.DescendUnits(new List<GridElement> { scenario.player.units[0] }));
        yield return new WaitForSecondsRealtime(0.15f);

        yield return StartCoroutine(ExplainMovement());
        yield return scenario.StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Player));

        while (true) {
            yield return null;
            if (scenario.player.units[0].coord.x == scenario.player.nail.coord.x || scenario.player.units[0].coord.y == scenario.player.nail.coord.y)
                break;
        }

        yield return StartCoroutine(HittingTheNail());
        cont = false;
        while (!cont) yield return null;
        yield return StartCoroutine(GenerateNextTutorialFloor(floorManager.floorSequence.GetFloor(true)));
        scenario.currentTurn = ScenarioManager.Turn.Descent;
        yield return StartCoroutine(EnemyTurn());

        yield return new WaitForSecondsRealtime(0.25f);
        yield return StartCoroutine(NailPriming());
        while (true) {
            yield return null;
            if (scenario.player.units[0].coord.x == scenario.currentEnemy.units[0].coord.x || scenario.player.units[0].coord.y == scenario.currentEnemy.units[0].coord.y)
                break;
        }
        yield return new WaitForSecondsRealtime(0.25f);
        yield return StartCoroutine(HittingAnEnemy());
        while (true) {
            yield return null;
            if (scenario.player.units[0].energyCurrent == 0 || scenario.player.units[1].energyCurrent == 0) break;
        }

        yield return StartCoroutine(OnTurnMoveAndAP());

        cont = false;
        while (!cont) yield return null;

        yield return StartCoroutine(EnemyTurn());
        yield return new WaitForSecondsRealtime(0.25f);
        yield return StartCoroutine(Equipment());
        
        yield return scenario.StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Player));
        //PersistentMenu.instance.musicController.SwitchMusicState(MusicController.MusicState.Game, true);

    }

    public IEnumerator GenerateNextTutorialFloor(FloorDefinition def) {
        yield return StartCoroutine(floorManager.TransitionFloors(true, false));

        yield return StartCoroutine(floorManager.GenerateFloor(def));
      
        floorManager.previewManager.UpdateFloors(floorManager.floors[floorManager.currentFloor.index]);

        yield return new WaitForSeconds(0.5f);
    
        yield return StartCoroutine(floorManager.previewManager.PreviewFloor(false, false));
    }

    public IEnumerator BlinkTile(Vector2 coord, bool move = true) {
        blinking = true;
        Tile sqr = FloorManager.instance.currentFloor.sqrs.Find(sqr => sqr.coord == coord);
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
        tooltip.SetText(new Vector3(100,0,0), body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }
        
        header = "";
        body = "We'll split off some more egregore for you to control, three mortal Slags to wield the tools of this depraved excavation. It's time to dig deep and make a mess.";
        tooltip.SetText(new Vector3(100,0,0), body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
        }
        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator ExplainMovement() {
        screenFade.gameObject.SetActive(true);

        body = "This is one of our Slags, Flathead. It can move around the floor and use the Hammer to strike me and the Nail, sending everything down to the next floor. Click on the Slag to move it in line with the Nail." + '\n';
        tooltip.SetText(new Vector3(100,0,0), body, header, true);

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
        tooltip.SetText(new Vector3(100,0,0), body, header, true, new List<RuntimeAnimatorController>{ hittingTheNailAnim });

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);

    }

    public IEnumerator NailPriming() {
        screenFade.gameObject.SetActive(true);

        header = "Nail Priming";
        body = "Since I'm wrapped around the Nail, I need some time to get ready for another descent. When the Nail is not primed, it can't be hit by the Hammer." + '\n';
        tooltip.SetText(new Vector3(100,0,0), body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator HittingAnEnemy() {
        screenFade.gameObject.SetActive(true);

        header = "Hitting Enemies";
        body = "The Hammer can be bounced between Slags. Strike an enemy and select Squigglespike, the other Slag, to bounce it to." + '\n';
        tooltip.SetText(new Vector3(100,0,0), body, header, true, new List<RuntimeAnimatorController>{ hittingEnemiesAnim });

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);

    }

    public IEnumerator OnTurnMoveAndAP() {
        screenFade.gameObject.SetActive(true);

        header = "Player Turn: Move and Action";
        body = "On your turn, each Slag can move and take an action. Descending down to the next floor ends your turn." + '\n';
        tooltip.SetText(new Vector3(100,0,0), body, header, true);

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
        tooltip.SetText(new Vector3(100,0,0), body, header, true, new List<RuntimeAnimatorController>{ anvilAnim, bigThrowAnim, shieldAnim });

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
        tooltip.SetText(new Vector3(100,0,0), body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator TutorialDescend() {
        
        yield return StartCoroutine(scenario.SwitchTurns(ScenarioManager.Turn.Descent, ScenarioManager.Scenario.Combat));
        yield return StartCoroutine(floorManager.TransitionFloors(true, false));
        yield return new WaitForSecondsRealtime(0.25f);

        switch(scenario.floorManager.floors.Count) {
            case 2:
                floorManager.currentFloor.RemoveElement(scenario.player.units[0]);
                floorManager.playerDropOverrides = new List<Vector2>();
                yield return StartCoroutine(floorManager.DescendUnits(new List<GridElement> { scenario.player.units[0], scenario.player.units[3] }));
                floorManager.playerDropOverrides = new List<Vector2>{ tutorialPacket.floors[0].initSpawns.Find(s => s.asset.prefab.GetComponent<PlayerUnit>()).coord };
                floorManager.currentFloor.RemoveElement(scenario.player.units[1]);
                yield return StartCoroutine(floorManager.DescendUnits( new List<GridElement> { scenario.player.units[1] }));
                yield return new WaitForSecondsRealtime(0.15f);


            break;
            case 3:
                floorManager.currentFloor.RemoveElement(scenario.player.units[0]); floorManager.currentFloor.RemoveElement(scenario.player.units[1]);
                floorManager.playerDropOverrides = new List<Vector2>();
                yield return StartCoroutine(floorManager.DescendUnits(new List<GridElement> { scenario.player.units[0], scenario.player.units[1], scenario.player.units[3] }));
                floorManager.playerDropOverrides = new List<Vector2>{ tutorialPacket.floors[1].initSpawns.Find(s => s.asset.prefab.GetComponent<PlayerUnit>()).coord };
                floorManager.currentFloor.RemoveElement(scenario.player.units[2]);
                yield return StartCoroutine(floorManager.DescendUnits( new List<GridElement> { scenario.player.units[2] }));
                yield return new WaitForSecondsRealtime(0.15f);
                floorManager.floorSequence.ThresholdCheck();
            break;
        }

        cont = true;
    }
    public IEnumerator EnemyTurn() {
        scenario.prevTurn = scenario.currentTurn;
        scenario.currentTurn = ScenarioManager.Turn.Enemy;
        scenario.player.StartEndTurn(false);
        if (scenario.uiManager.gameObject.activeSelf)
            yield return StartCoroutine(scenario.messagePanel.PlayMessage(MessagePanel.Message.Antibody));

        
        if (scenario.prevTurn == ScenarioManager.Turn.Descent)
            yield return StartCoroutine(scenario.currentEnemy.TakeTurn(true));
        else {
            if (!enemyBehavior) {
                yield return StartCoroutine(EnemyBehavior());
                enemyBehavior = true;
            }
            yield return StartCoroutine(scenario.currentEnemy.TakeTurn(false));
        }
    }

    public void SwitchTurns() {
        StartCoroutine(EnemyTurn());
    }

}
