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
    public bool bulbEncountered = false, deathReviveEncountered = false, basophicEncountered = false, prebossEncountered = false, bossEncountered = false;

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

    public IEnumerator EquipTmentooltips(int index) {
        switch(index) {
            case 1:
                screenFade.gameObject.SetActive(true);

                header = "";
                body = "You unlocked the Anvil. Drop an <b>" + ColorToRichText("ANVIL", keyColor) + "</b> and move away. The Anvil <b>" + ColorToRichText("ATTRACTS ENEMIES", keyColor) + "</b> and descends with units, <b>" + ColorToRichText("CRUSHING ANYTHING IT LANDS ON.", keyColor) + "</b>" + '\n';
                tooltip.SetText(body, header, true, new List<RuntimeAnimatorController>{ anvilAnim });

                while (!tooltip.skip) {
                    yield return new WaitForSecondsRealtime(1 / Util.fps);
                }
                screenFade.SetTrigger("FadeOut");
                tooltip.transform.GetChild(0).gameObject.SetActive(false);
            break;
            case 2:
                screenFade.gameObject.SetActive(true);

                header = "";
                body = "You unlocked the <b>" + ColorToRichText("BIG GRAB", keyColor) + "</b>. <b>" + ColorToRichText("GRAB", keyColor) + "</b> and <b>" + ColorToRichText("THROW", keyColor) + "</b> enemies with this equipment." + '\n';
                tooltip.SetText(body, header, true, new List<RuntimeAnimatorController>{ bigThrowAnim });

                while (!tooltip.skip) {
                    yield return new WaitForSecondsRealtime(1 / Util.fps);
                }
                screenFade.SetTrigger("FadeOut");
                tooltip.transform.GetChild(0).gameObject.SetActive(false);
            break;
        }

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


    public IEnumerator Preboss() {
        prebossEncountered = true;
        Debug.Log("Wait for player turn preboss");
        while (ScenarioManager.instance.currentTurn != ScenarioManager.Turn.Player) {
            yield return null;
        }
        Debug.Log("Player turn preboss");
        yield return new WaitForSecondsRealtime(0.25f);
        screenFade.gameObject.SetActive(true);

        header = "DANGER AHEAD";
        body = "Sounds like trouble's knocking, <b>" + ColorToRichText("big time", keyColor) + "</b>. Nail stays put till the dust settles. <b>" + ColorToRichText("Your dance, squish", keyColor) + "</b>. We'll be chillin' up here." + '\n';
        tooltip.SetText(body, header, true);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
        }

        screenFade.SetTrigger("FadeOut");
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


    public static string ColorToRichText(string str, Color color) {
        return "<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">" + str + "</color>";
    }

}
