using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayOptionalTooltips : MonoBehaviour {


    public static GameplayOptionalTooltips instance;
    ScenarioManager scenario;
    [HideInInspector] public string header, body;
    [SerializeField] Color keyColor;
    public DialogueTooltip tooltip;
    public Animator screenFade;
    bool tooltipToggle;
    [SerializeField] RuntimeAnimatorController reviveAnim, bulbAnim;
    public bool bulbEncountered = false, deathReviveEncountered = false, prebossEncountered = false, bossEncountered = false,
        pathsEncountered = false, objectivesEncountered = false, bloatedBulbEncountered = false, beaconEncountered = false, beaconObjectivesEncountered = false, beaconScratchOffEncountered = false;

    void Awake() {
        if (GameplayOptionalTooltips.instance) {
            DestroyImmediate(this);
            return;
        }
        GameplayOptionalTooltips.instance = this;
    }

    public void Initialize() {
        scenario = ScenarioManager.instance;
        for (int i = 0; i <= scenario.player.units.Count - 1; i++) {
            if (scenario.player.units[i] is PlayerUnit pu) {
                pu.ElementDisabled += StartDeathTut;
            }
        }

        //LoadTooltips();
    }

    public void LoadTooltips() {
        Debug.Log("Load tooltips");
        UserData user = PersistentDataManager.instance.userData;
        tooltipToggle = user.tooltipToggle;
        
        bulbEncountered = user.tooltipsEncountered.ContainsKey("bulbEncountered") && user.tooltipsEncountered["bulbEncountered"];
        deathReviveEncountered = user.tooltipsEncountered.ContainsKey("deathReviveEncountered") && user.tooltipsEncountered["deathReviveEncountered"];
        bossEncountered = user.tooltipsEncountered.ContainsKey("bossEncountered") && user.tooltipsEncountered["bossEncountered"];
        objectivesEncountered = user.tooltipsEncountered.ContainsKey("objectivesEncountered") && user.tooltipsEncountered["objectivesEncountered"];
        bloatedBulbEncountered = user.tooltipsEncountered.ContainsKey("bloatedBulbEncountered") && user.tooltipsEncountered["bloatedBulbEncountered"];
        beaconEncountered = user.tooltipsEncountered.ContainsKey("beaconEncountered") && user.tooltipsEncountered["beaconEncountered"];
        beaconObjectivesEncountered = user.tooltipsEncountered.ContainsKey("beaconObjectivesEncountered") && user.tooltipsEncountered["beaconObjectivesEncountered"];
        beaconScratchOffEncountered = user.tooltipsEncountered.ContainsKey("beaconScratchOffEncountered") && user.tooltipsEncountered["beaconScratchOffEncountered"];
    }

    public IEnumerator Paths() {
        if (tooltipToggle) yield break;
        pathsEncountered = true;

        header = "CHUNKS";
        body = "<b>" + ColorToRichText("Pick your path", keyColor) + "</b>, squish. Big Slime upstairs has intel on what lies below." ; //Get a sense of the <b>" + ColorToRichText("danger", keyColor) + "</b>, and a peek at the <b>" + ColorToRichText("rewards", keyColor) + "</b>." + '\n'
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator TileBulb() {
        if (tooltipToggle) yield break;
        bulbEncountered = true;
        while (ScenarioManager.instance.currentTurn != ScenarioManager.Turn.Player) {
            yield return null;
        }
        yield return new WaitForSecondsRealtime(1.25f);
        screenFade.gameObject.SetActive(true);

        header = "BULBS";
        body = "<b>" + ColorToRichText("Bulbs", keyColor) + "</b>, handy munchies for your Slags. Step on them to grab, 1 per Slag. <b>" + ColorToRichText("Bulbs are a free action", keyColor) + "</b>, so don't hoard them." + '\n';
        tooltip.SetText(body, header, true, false, new List<RuntimeAnimatorController>{ bulbAnim });

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator BloatedBulb() {
        if (tooltipToggle) yield break;
        bloatedBulbEncountered = true;

        while (ScenarioManager.instance.currentTurn != ScenarioManager.Turn.Player) {
            yield return null;
        }
        yield return new WaitForSecondsRealtime(1.25f);
        screenFade.gameObject.SetActive(true);

        header = "BLOATED BULB";
        body = "Woah squish, that's a big bulb! Why don't you <b>" + ColorToRichText("crack it open", keyColor) + "</b> and see what's inside?";
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator Beacon() {
        if (tooltipToggle) yield break;
        beaconEncountered = true;
        while (ScenarioManager.instance.currentTurn != ScenarioManager.Turn.Player) {
            yield return null;
        }
        yield return new WaitForSecondsRealtime(1.25f);
        screenFade.gameObject.SetActive(true);

        header = "BEACON";
        body = "Yo, squish. That tower there is one of my direct lines. Select it and <b>" + ColorToRichText("use its action", keyColor) + "</b> in the bottom left <b>" + ColorToRichText("before you get started", keyColor) + ".";
        tooltip.SetText(body, header, true, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator Objectives() {
        if (tooltipToggle) yield break;
        objectivesEncountered = true;

        header = "TASKS";
        body = "Hold up a sec, I got a list for ya. Knock these out, and I'll toss some <b>" + ColorToRichText("slime bux", keyColor) + "</b> your way for the hustle.";
        tooltip.SetText(body, header, true, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }

        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }


    public IEnumerator BeaconObjectives() {
        if (tooltipToggle) yield break;
        beaconObjectivesEncountered = true;

        header = "TASKS";
        body = "Keep me updated on your progress—<b>" + ColorToRichText("cash in", keyColor) + "</b> completed tasks or <b>" + ColorToRichText("switch 'em out", keyColor) + "</b> if ya want.";
        tooltip.SetText(body, header, true, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }

        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator BeaconScratchOff() {
        if (tooltipToggle) yield break;
        beaconScratchOffEncountered = true;

        header = "LUCKY LIXXX";
        body = "Ey, I got a <b>" + ColorToRichText("lotto card", keyColor) + "</b> here just for youse. Give it a scratch and see what's cookin', then <b>" + ColorToRichText("take your pick of the goodies.", keyColor) + "</b>";
        tooltip.SetText(body, header, true, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }

        tooltip.transform.GetChild(0).gameObject.SetActive(false);

    }

    void StartDeathTut(GridElement blank) {
        if (!deathReviveEncountered) {
            StartCoroutine(DeathRevivTut());
            for (int i = 0; i <= scenario.player.units.Count - 1; i++) {
                if (scenario.player.units[i] is PlayerUnit pu) {
                    pu.ElementDisabled -= StartDeathTut;
                }
            }
        }
    }

     public IEnumerator DeathRevivTut() {
        if (tooltipToggle) yield break;
        deathReviveEncountered = true;

        while (scenario.currentTurn != ScenarioManager.Turn.Player) {
            yield return null;
        }
        yield return new WaitForSecondsRealtime(1.25f);
        screenFade.gameObject.SetActive(true);

        header = "SLAG REVIVE";
        body = "Slags get <b>" + ColorToRichText("downed", keyColor) + "</b>, no biggie. Hammer the downed one to <b>" + ColorToRichText("revive", keyColor) + "</b> or let 'em nap. Your call, squish." + '\n';
        tooltip.SetText(body, header, true, false, new List<RuntimeAnimatorController>{ reviveAnim });

        while (!tooltip.skip) 
            yield return new WaitForSecondsRealtime(1/Util.fps);   

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }


    public IEnumerator Preboss() {        
        header = "DANGER AHEAD";
        body = "Sounds like trouble's knocking, <b>" + ColorToRichText("big time", keyColor) + "</b>. Nail stays up here until you <b>" + ColorToRichText("clear the danger", keyColor) + "</b>." + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
        }

        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator Boss() {
        if (tooltipToggle) yield break;
        bossEncountered = true;
        while (ScenarioManager.instance.currentTurn != ScenarioManager.Turn.Player) {
            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.25f);
        screenFade.gameObject.SetActive(true);

        header = "BIG THREAT";
        body = "That's the toughest nut yet. It's a ground-shaker, <b>" + ColorToRichText("its attacks cause descents", keyColor) + "</b>. Careful, squish. This one means business." + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator BossSlain() {        
        screenFade.gameObject.SetActive(true);

        header = "NAIL INCOMING";
        body = "Cracked the big guy! <b>" + ColorToRichText("Brace yourself", keyColor) + "</b>, massive impact incoming!" + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator WhatsAhead() {        
        header = "WHAT'S AHEAD";
        body = "Look below us! I can see the real juice coming up. I can't wait for you to see it too! Until next time..." + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
        }

        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }


    static string ColorToRichText(string str, Color color) {
        return "<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">" + str + "</color>";
    }

}
