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
    [SerializeField] public Animator peekFrameAnim, peekEyeAnim;
    [SerializeField] bool previewing;

    [HideInInspector] public bool tut;

    void Start() {
        if (ScenarioManager.instance) {
            scenario = ScenarioManager.instance;
        }
        if (FloorManager.instance)
            floorManager = FloorManager.instance;
    }

    public IEnumerator PreviewFloor(bool down) {
        scenario.player.GridMouseOver(new Vector2(-32, -32), false);
        floorManager.transitioning = true;
        if (scenario.currentTurn == ScenarioManager.Turn.Player)
            UIManager.instance.endTurnButton.interactable = !down;
        if (down) {
            peekFrameAnim.SetBool("Active", true); peekEyeAnim.SetBool("Active", true);
            TogglePreivews(true);
            foreach(DescentPreview dp in descentPreviews) {
                if (dp.unit.grid == floorManager.currentFloor)
                    dp.UpdatePreview(dp.unit);
            }

            yield return StartCoroutine(floorManager.TransitionFloors(down, true));
            floorManager.transitioning = false;
        } else {
            peekFrameAnim.SetBool("Active", false); peekEyeAnim.SetBool("Active", false);
            TogglePreivews(false);
     
            yield return StartCoroutine(floorManager.TransitionFloors(down, true));
            
            floorManager.transitioning = false;
        }
    }

    public void InitialPreview() {
        UpdateFloors(null, currentFloor);
        foreach(DescentPreview dp in descentPreviews) 
            dp.UpdatePreview(dp.unit);
        
        foreach(DescentPreview dp in descentPreviews) {
            if (dp.unit is PlayerUnit)
                dp.anim.gameObject.SetActive(true);
        }
    }

    public void TogglePreivews(bool state) {
        foreach(DescentPreview dp in descentPreviews) {
            if (dp.unit.grid == floorManager.currentFloor || state == false) {
                dp.anim.gameObject.SetActive(state);
                dp.HighlightTile(state);
            }
        }

        if (scenario.currentEnemy) {
            foreach (GameObject obj in scenario.currentEnemy.pendingUnitUIs)
                    obj.SetActive(!state);
        }
    }

    public void PreviewButton() {
        int dir = previewing ? 1 : -1;
        previewing = !previewing;
        
        FloorPeekEvent evt = ObjectiveEvents.FloorPeekEvent;
        evt.down = dir != 1;
        ObjectiveEventManager.Broadcast(evt);

        if (tut && floorManager.floorSequence.activePacket.packetType != FloorPacket.PacketType.Tutorial) {
            StartCoroutine(PeekTutorial(previewing));
        } else {
            scenario.player.DeselectUnit();
            if (!floorManager.transitioning && floorManager.floors.Count - 1 >= floorManager.currentFloor.index - dir) {
                StartCoroutine(PreviewFloor(previewing));
                if (UIManager.instance.gameObject.activeSelf)
                    UIManager.instance.PlaySound(previewing ? UIManager.instance.peekBelowSFX.Get() : UIManager.instance.peekAboveSFX.Get());
            }
        }

    }

    IEnumerator PeekTutorial(bool down) {

        tut = false;
        yield return scenario.tutorial.StartCoroutine(scenario.tutorial.PeekButton());
        PreviewButton();

    }

    public void UpdateFloors(Grid newFloor, Grid alignFloor) {
        currentFloor = newFloor;
        alignmentFloor = alignFloor;
        transform.parent = alignFloor.transform;
        if (newFloor)
            transform.localPosition = Vector3.zero + new Vector3(0, 3);
        else
            transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
    }
}
