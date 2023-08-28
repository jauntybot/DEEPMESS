using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayOptionalTooltips : MonoBehaviour
{


    public static GameplayOptionalTooltips instance;
    ScenarioManager scenario;
    [HideInInspector] public string header, body;

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

    public IEnumerator TileBulb() {
        Debug.Log("Tile bulb");
        bulbEncountered = true;
        while (ScenarioManager.instance.currentTurn != ScenarioManager.Turn.Player) {
            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.25f);
        screenFade.gameObject.SetActive(true);

        header = "BULBS";
        body = "Bulbs are consumable items that your Slags can pick up. Each Slag can hold 1 bulb that can be used or thrown as a free action" + '\n';
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

        Debug.Log("Death revive");
        while (scenario.currentTurn != ScenarioManager.Turn.Player) {
            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.25f);
        Debug.Log("start");
        screenFade.gameObject.SetActive(true);

        header = "SLAG REVIVE";
        body = "Slags that have been downed can be revived. Strike the downed Slag with the Hammer to transfuse 1HP from the Nail. The Slag will come back with its move and action refreshed." + '\n';
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
        body = "This enemy can explode, dealing damage to all the tiles around it. Hover over enemy portraits to learn more about their abilities." + '\n';
        tooltip.SetText(body, header, true, new List<RuntimeAnimatorController>{ basophicAnim });

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
            
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }


    public IEnumerator Preboss() {
        prebossEncountered = true;
        while (ScenarioManager.instance.currentTurn != ScenarioManager.Turn.Player) {
            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.25f);
        screenFade.gameObject.SetActive(true);

        header = "DANGER AHEAD";
        body = "Uh-oh, sounds like something big is coming..." + '\n';
        tooltip.SetText(body, header);

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
        body = "This is the strongest enemy we have faced yet! It can move further than other enemies. When it attacks, it slams down on the floor, causing everything to crash through to the floor below. You will not be able to strike the Nail until we deal with this enemy." + '\n';
        tooltip.SetText(body, header);

        while (!tooltip.skip) {
            yield return new WaitForSecondsRealtime(1/Util.fps);
        }

        screenFade.SetTrigger("FadeOut");
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }


}
