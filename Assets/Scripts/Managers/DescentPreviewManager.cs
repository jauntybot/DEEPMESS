using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DescentPreviewManager : MonoBehaviour
{
    ScenarioManager scenario;
    FloorManager floorManager;
    public List<DescentPreview> descentPreviews;
    public Grid currentFloor, alignmentFloor;
    [SerializeField] public GameObject upButton, downButton;

    [HideInInspector] public bool tut;

    void Start() {
        if (ScenarioManager.instance) {
            scenario = ScenarioManager.instance;
            tut = scenario.tutorial != null;
        }
        if (FloorManager.instance)
            floorManager = FloorManager.instance;
    }

    public IEnumerator PreviewFloor(bool down) {
        floorManager.transitioning = true;
        if (scenario.currentTurn == ScenarioManager.Turn.Player)
            UIManager.instance.endTurnButton.enabled = !down;
        if (down) {
            downButton.SetActive(false); upButton.SetActive(true);
            TogglePreivews(true);
            foreach(DescentPreview dp in descentPreviews) {
                if (dp.unit.grid == floorManager.currentFloor)
                    dp.UpdatePreview(dp.unit);
            }

            yield return StartCoroutine(floorManager.TransitionFloors(down, true));
            floorManager.transitioning = false;
        }
        else {
            downButton.SetActive(true); upButton.SetActive(false);
            TogglePreivews(false);
            
            yield return StartCoroutine(floorManager.TransitionFloors(down, true));
            
            floorManager.transitioning = false;
        }
    }

    public void InitialPreview() {
        foreach(DescentPreview dp in descentPreviews) {
            if (dp.unit is PlayerUnit)
                dp.anim.gameObject.SetActive(true);
        }
    }

    public void TogglePreivews(bool state) {
        foreach(DescentPreview dp in descentPreviews) {
            if (dp.unit.grid == floorManager.currentFloor || state == false)
                dp.anim.gameObject.SetActive(state);
        }
    }

    public void PreviewButton(bool down) {
        int dir = down ? 1 : -1;

        if (tut && floorManager.floorSequence.activePacket.packetType != FloorPacket.PacketType.Tutorial) {
            StartCoroutine(PeekTutorial(down));
        } else {
            scenario.player.DeselectUnit();
            if (!floorManager.transitioning && floorManager.floors.Count - 1 >= floorManager.currentFloor.index + dir) {
                StartCoroutine(PreviewFloor(down));
                if (UIManager.instance.gameObject.activeSelf)
                    UIManager.instance.PlaySound(down ? UIManager.instance.peekBelowSFX.Get() : UIManager.instance.peekAboveSFX.Get());
            }
        }

    }

    IEnumerator PeekTutorial(bool down) {

        tut = false;
        yield return scenario.tutorial.StartCoroutine(scenario.tutorial.PeekButton());
        PreviewButton(down);

    }

    public void UpdateFloors(Grid newFloor, Grid alignFloor) {
        currentFloor = newFloor;
        alignmentFloor = alignFloor;
        transform.parent = alignFloor.transform;
        transform.localPosition = Vector3.zero + new Vector3(0, 3);
        transform.localScale = Vector3.one;
    }
}
