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
        body = "<b>" + ColorToRichText("BULBS", keyColor) + "</b> are consumable items that <b>" + ColorToRichText("SLAGS", keyColor) + "</b> can pick up. Each Slag can hold 1 bulb that can be <b>" + ColorToRichText("THROWN", keyColor) + "</b> as a <b>" + ColorToRichText("FREE ACTION", keyColor) + "</b>." + '\n';
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
        body = "<b>" + ColorToRichText("SLAGS", keyColor) + "</b> that have been downed can be <b>" + ColorToRichText("REVIVED", keyColor) + "</b>. Strike the downed Slag with the <b>" + ColorToRichText("HAMMER", keyColor) + "</b> to <b>" + ColorToRichText("TRANSFUSE 1HP FROM THE NAIL", keyColor) + "</b>." + '\n';
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
        body = "This enemy can <b>" + ColorToRichText("EXPLODE", keyColor) + "</b>, dealing damage to <b>" + ColorToRichText("ALL SURROUNDING TILES", keyColor) + "</b>." + '\n';
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
        body = "Uh-oh, sounds like something <b>" + ColorToRichText("BIG", keyColor) + "</b> is coming..." + '\n';
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
        body = "This is the <b>" + ColorToRichText("STRONGEST ENEMY", keyColor) + "</b> we have faced yet! It can move further than other enemies. When it attacks, it damages <b>" + ColorToRichText("ALL SURROUNDING TILES", keyColor) + "</b>, causing everything to crash through to the floor below. You will <b>" + ColorToRichText("NOT", keyColor) + "</b> be able to <b>" + ColorToRichText("STRIKE THE NAIL", keyColor) + "</b> until we deal with this enemy." + '\n';
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
