using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaterTutorials : MonoBehaviour
{

    [SerializeField] Camera cam;
    string content;
    [SerializeField] Tooltip tooltip;

    [HideInInspector] public bool healpadEncountered = false, equippadEncountered = false, deathReviveEncountered = false, slotsEncountered = false;

    public void StartListening(PlayerManager player) {
        Debug.Log("start listening");
        foreach (Unit u in player.units) {
            if (u is PlayerUnit pu) {
                pu.ElementDisabled += StartDeathTut;
                
            }
        }
    }

    public IEnumerator CheckFloorDef(FloorDefinition floor) {
        foreach (FloorDefinition.Spawn sp in floor.initSpawns) {
            if (sp.asset.ge is TilePad tp) {
                if (!healpadEncountered && tp.buff == TilePad.Buff.Heal) yield return StartCoroutine(HealpadTut(sp.coord));
                if (!equippadEncountered && tp.buff == TilePad.Buff.Equip) yield return StartCoroutine(EquippadTut(sp.coord));
            } 
        }
    }

    public void CheckAllDone() {
        if (healpadEncountered &&
            equippadEncountered &&
            deathReviveEncountered &&
            slotsEncountered)
            Destroy(TutorialSequence.instance.gameObject);
    }

    public IEnumerator HealpadTut(Vector2 coord) {
        healpadEncountered = true;
        StartCoroutine(TutorialSequence.instance.BlinkTile(coord));
        TutorialSequence.instance.screenFade.gameObject.SetActive(true);
        content = "This floor has a HEALPAD. It will heal SLAGS for 2HP, so make sure you step on it before an ANTIBODY does!";
        tooltip.SetText( new Vector2(-550,400), content);
        while (true) {
            yield return null;
            if (Input.GetMouseButtonDown(0)) break;
        }
        TutorialSequence.instance.screenFade.SetTrigger("FadeOut");
        while (true) {
            yield return null;
            if (Input.GetMouseButtonDown(0)) break;
        }
        TutorialSequence.instance.blinking = false;
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
        CheckAllDone();
    }

    public IEnumerator EquippadTut(Vector2 coord) {
        equippadEncountered = true;
        StartCoroutine(TutorialSequence.instance.BlinkTile(coord));
        TutorialSequence.instance.screenFade.gameObject.SetActive(true);
        content = "That's a EQUIPPAD. It will refresh the equipment of the SLAG that steps on it, letting them use it more than once on the current floor.";
        tooltip.SetText(new Vector2(-550,400), content);
        while (true) {
            yield return null;
            if (Input.GetMouseButtonDown(0)) break;
        }
        TutorialSequence.instance.screenFade.SetTrigger("FadeOut");
        while (true) {
            yield return null;
            if (Input.GetMouseButtonDown(0)) break;
        }
        TutorialSequence.instance.blinking = false;
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
        CheckAllDone();
    }

    public IEnumerator SlotsTut() {
        slotsEncountered = true;
        TutorialSequence.instance.screenFade.gameObject.SetActive(true);
        content = "This is the slot machine. Here you can spin for new equipment to outfit your SLAGS with, or gain a little health back for the whole crew, including me.";
        tooltip.SetText(new Vector2(420, 50), content);
        while (true) {
            yield return null;
            if (Input.GetMouseButtonDown(0)) break;
        }
        content = "You'll encounter these slot machines every 5 floors, so try your luck once in a while.";
        tooltip.SetText(new Vector2(420, 390), content);
        TutorialSequence.instance.screenFade.SetTrigger("FadeOut");
        while (true) {
            yield return null;
            if (Input.GetMouseButtonDown(0)) break;
        }
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
        CheckAllDone();
    }

    void StartDeathTut(GridElement blank) {
        if (!deathReviveEncountered)
            StartCoroutine(DeathRevivTut());
    }

    public IEnumerator DeathRevivTut() {
        while (ScenarioManager.instance.currentTurn != ScenarioManager.Turn.Player) yield return null;
        yield return new WaitForSecondsRealtime(0.25f);

        deathReviveEncountered = true;
        TutorialSequence.instance.screenFade.gameObject.SetActive(true);
        content = "Oh no, one of your SLAGS has been downed. Don't worry, we can work together to bring it back into the fight.";
        tooltip.SetText(new Vector2(-550,400), content);
        while (true) {
            yield return null;
            if (Input.GetMouseButtonDown(0)) break;
        }
        TutorialSequence.instance.screenFade.SetTrigger("FadeOut");
        content = "You can hit the downed SLAG with the HAMMER to take 1 of my HP and get the unit back on its feet.";
        tooltip.SetText(content);
        while (true) {
            yield return null;
            if (Input.GetMouseButtonDown(0)) break;
        }
        content = "You can bounce the hammer back to any of your SLAGS, including the one you just revived." + '\n' + "The downed SLAG will come back with its move and action refreshed.";
        tooltip.SetText(content);
        while (true) {
            yield return null;
            if (Input.GetMouseButtonDown(0)) break;
        }
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
        CheckAllDone();
    }

}
