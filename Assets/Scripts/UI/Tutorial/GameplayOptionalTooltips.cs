using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayOptionalTooltips : MonoBehaviour
{


    public static GameplayOptionalTooltips instance;
    ScenarioManager scenario;
    [HideInInspector] public string header, body;
    [SerializeField] Color keyColor;
    public DialogueTooltip tooltip;
    public Animator screenFade;
    [SerializeField] RuntimeAnimatorController anvilAnim, bigThrowAnim, basophicAnim, reviveAnim, bulbAnim;
    public bool bulbEncountered = false, deathReviveEncountered = false, basophicEncountered = false, prebossEncountered = false, bossEncountered = false,
        vacuoleEncountered = true, objectivesEncountered = false, rewardsEncountered = false;

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
    }

    public IEnumerator Objectives() {
        objectivesEncountered = true;

        header = "OBJECTIVES";
        body = "<b>" + ColorToRichText("Pick your path", keyColor) + "</b>, squish. Big Slime upstairs has intel on what lies below. Get a sense of the <b>" + ColorToRichText("danger", keyColor) + "</b>, and a peek at the <b>" + ColorToRichText("rewards", keyColor) + "</b>." + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator TileBulb() {
        Debug.Log("Tile bulb");
        bulbEncountered = true;
        while (ScenarioManager.instance.currentTurn != ScenarioManager.Turn.Player) {
            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.25f);
        screenFade.gameObject.SetActive(true);

        header = "BULBS";
        body = "<b>" + ColorToRichText("Bulbs", keyColor) + "</b>, handy munchies for your Slags. Step on them to grab, 1 per Slag. <b>" + ColorToRichText("Bulbs are a free action", keyColor) + "</b>, so don't hoard them." + '\n';
        tooltip.SetText(body, header, true, new List<RuntimeAnimatorController>{ bulbAnim });

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator Rewards() {
        rewardsEncountered = true;

        header = "OBJECTIVES";
        body = "Nice getting through that, squish. Use any <b>" + ColorToRichText("Nuggets", keyColor) + "</b> you bag to power up and take any <b>" + ColorToRichText("Relics", keyColor) + "</b> to tweak our excavation in wild ways.";
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }

        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    void StartDeathTut(GridElement blank) {
        if (!deathReviveEncountered) {
            StartCoroutine(DeathRevivTut());
        }
        else {
            for (int i = 0; i <= scenario.player.units.Count - 1; i++) {
                if (scenario.player.units[i] is PlayerUnit pu) {
                    pu.ElementDisabled -= StartDeathTut;
                }
            }
        }
    }

     public IEnumerator DeathRevivTut() {
        deathReviveEncountered = true;

        while (scenario.currentTurn != ScenarioManager.Turn.Player) {
            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.25f);
        screenFade.gameObject.SetActive(true);

        header = "SLAG REVIVE";
        body = "Slags get <b>" + ColorToRichText("downed", keyColor) + "</b>, no biggie. Hammer the downed one to <b>" + ColorToRichText("revive", keyColor) + "</b> or let 'em nap. Your call, squish." + '\n';
        tooltip.SetText(body, header, true, new List<RuntimeAnimatorController>{ reviveAnim });

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
        bossEncountered = true;
        while (ScenarioManager.instance.currentTurn != ScenarioManager.Turn.Player) {
            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.25f);
        screenFade.gameObject.SetActive(true);

        header = "BIG THREAT";
        body = "That's the toughest nut yet. It's a ground-shaker, <b>" + ColorToRichText("causing everything to crash below", keyColor) + "</b>. Careful, squish. This one means business." + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator BossSlain() {        
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


    public static string ColorToRichText(string str, Color color) {
        return "<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">" + str + "</color>";
    }

}
