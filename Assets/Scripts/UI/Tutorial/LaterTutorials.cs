using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaterTutorials : MonoBehaviour
{

    [SerializeField] Camera cam;
    string content;
    [SerializeField] DialogueTooltip tooltip;

    [HideInInspector] public bool healpadEncountered = false, equippadEncountered = false, deathReviveEncountered = false, slotsEncountered = false;

    public void StartListening(PlayerManager player) {
        Debug.Log("start listening");

    }

    public IEnumerator CheckFloorDef(FloorDefinition floor) {
        foreach (FloorDefinition.Spawn sp in floor.initSpawns) {
            // if (sp.asset.prefab.GetComponent<GridElement>() is TilePad tp) {
            //     if (!healpadEncountered && tp.buff == TilePad.Buff.Heal) yield return StartCoroutine(HealpadTut(sp.coord));
            //     if (!equippadEncountered && tp.buff == TilePad.Buff.Equip) yield return StartCoroutine(EquippadTut(sp.coord));
            // } 
            yield return null;
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
        tooltip.SetText(content, "", true);
        while (!tooltip.skip) {
            yield return null;
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
        tooltip.SetText(content, "", true);
        while (!tooltip.skip) {
            yield return null;
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
        content = "This is the slot machine. Here you can spin for new equipment to outfit your SLAGS with. It also heals us all a little.";
        tooltip.SetText(content, "", true);
        while (!tooltip.skip) {
            yield return null;
        }
        content = "You'll encounter these slot machines every 5 floors, so try your luck once in a while.";
        tooltip.SetText(content);
        TutorialSequence.instance.screenFade.SetTrigger("FadeOut");
        while (true) {
            yield return null;
            if (Input.GetMouseButtonDown(0)) break;
        }
        tooltip.transform.GetChild(0).gameObject.SetActive(false);
        CheckAllDone();
    }



   

}
