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

    public IEnumerator SplashMessage() {
        header = "";
        body = "Listen up, squish! We're on a big brain mission. Gotta feast on <b>" + ColorToRichText("tasty thoughts", keyColor) + "</b>, yeah? Scavenge, gobble, we'll make this place our own." + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }

        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }


    public IEnumerator Objectives() {
        objectivesEncountered = true;

        header = "OBJECTIVES";
        body = "The big slime's got a to-do list. Check 'em off, <b>" + ColorToRichText("score tasty god nuggets", keyColor) + "</b>. Upgrade gear, get stronger. Fail? No biggie, just a hiccup. No whining, squish, just keep grinding." + '\n';
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
        body = "<b>" + ColorToRichText("Bulbs", keyColor) + "</b>, handy munchies for your Slags. Step on them to grab, 1 per Slag. Use it or chuck it on the grid. <b>" + ColorToRichText("Bulbs are a free action", keyColor) + "</b>, so don't hoard them." + '\n';
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
        body = "Hope you <b>" + ColorToRichText("nailed those objectives", keyColor) + "</b>. Use any nuggets you bagged to power up, squish. Apply 'em to <b>" + ColorToRichText("Slags", keyColor) + "</b> or even the <b>" + ColorToRichText("Hammer", keyColor) + "</b>. Make those Slags sing.";
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
        body = "Slags get downed, no biggie. Hammer the downed one to <b>" + ColorToRichText("bring 'em back with 1 HP", keyColor) + "</b>. Revive or let 'em nap. Your call, squish." + '\n';
        tooltip.SetText(body, header, true, new List<RuntimeAnimatorController>{ reviveAnim });

        while (!tooltip.skip) 
            yield return new WaitForSecondsRealtime(1/Util.fps);   

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }


    public IEnumerator Basophic() {
        basophicEncountered = true;
        while (ScenarioManager.instance.currentTurn != ScenarioManager.Turn.Player) {
            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.25f);
        screenFade.gameObject.SetActive(true);

        header = "NEW DANGERS";
        body = "This <b>" + ColorToRichText("baddie's a bomb", keyColor) + "</b>. After readying up, it will damage <b>" + ColorToRichText("all the tiles around it", keyColor) + "</b> when it pops. Handle with care or deal with the damage." + '\n';
        tooltip.SetText(body, header, true, new List<RuntimeAnimatorController>{ basophicAnim });

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator Vacuole() {
        vacuoleEncountered = true;
        while (ScenarioManager.instance.currentTurn != ScenarioManager.Turn.Player) {
            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.25f);
        screenFade.gameObject.SetActive(true);

        header = "NEW DANGERS";
        body = "Stuck in place, this one. <b>" + ColorToRichText("Shoots thorns in every direction", keyColor) + "</b>. No retreat, just a barrage of pain. Stay clear or become a pincushion, squish." + '\n';
        tooltip.SetText(body, header, true, new List<RuntimeAnimatorController>{ basophicAnim });

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }


    public IEnumerator Preboss() {        
        header = "DANGER AHEAD";
        body = "Sounds like trouble's knocking, <b>" + ColorToRichText("big time", keyColor) + "</b>. Nail stays put till the dust settles. <b>" + ColorToRichText("Your dance, squish", keyColor) + "</b>. We'll be chillin' up here." + '\n';
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
        body = "That orange's the toughest nut yet! Move? Far. Attack? It's a ground-shaker, <b>" + ColorToRichText("causing everything to crash below", keyColor) + "</b>. Careful, squish. This one means business." + '\n';
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
