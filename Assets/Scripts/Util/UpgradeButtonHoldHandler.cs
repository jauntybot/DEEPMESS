using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeButtonHoldHandler : MonoBehaviour, IUpdateSelectedHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler {

    UnitUpgradeUI ui;
    Image radialProg;
    bool isPressed;
    [SerializeField] float holdDur;
    float confirmProg;


    public void Init(UnitUpgradeUI _ui) {
        ui = _ui;
        ResetProgress();
        DisplayProgress();
    }

    public void OnPointerEnter(PointerEventData data) {
        foreach (NuggetSlot slot in ui.slots) {
            if (slot.filled)
                slot.DisplayPopup(true);
        }
        if (ui.upgrade.selectedParticle)
            ui.CurrentSlot().DisplayPopup(true);
    }

    public void OnPointerExit(PointerEventData data) {
        foreach (NuggetSlot slot in ui.slots) {
            slot.DisplayPopup(false);
        }
    }

    public void OnUpdateSelected(BaseEventData data) {
        if (ui.previewParticle) {
            if (isPressed) {
                ProgressConfirm();
            }
            DisplayProgress();
        }
    }

    public void OnPointerDown(PointerEventData data) {
        isPressed = true;
        confirmProg = 0;
        radialProg = ui.CurrentSlot().radialFill;
        radialProg.GetComponent<AudioSource>().enabled = true;
        if (ui.previewParticle) {
            Image image = ui.previewParticle.GetComponentInChildren<Image>();
            image.color = new Color(image.color.r, image.color.g, image.color.b, 1);
        }
    }

    public void OnPointerUp(PointerEventData data) {
        isPressed = false;
        radialProg.GetComponent<AudioSource>().enabled = false;
        if (confirmProg < holdDur) {
            confirmProg = 0;
            if (ui.previewParticle) {
                Image image = ui.previewParticle.GetComponentInChildren<Image>();
                image.color = new Color(image.color.r, image.color.g, image.color.b, 0.6f);
            }
        }
    }

    public void ProgressConfirm() {
        if (confirmProg < holdDur) {
            confirmProg += Time.deltaTime;
        }
    }

    public void DisplayProgress() {
        if (radialProg)
            radialProg.fillAmount = confirmProg/holdDur;

        if (confirmProg >= holdDur) {
            ui.ApplyUpgrade();
        }
    }

    public void ResetProgress() {
        confirmProg = 0;
        DisplayProgress();
    }

}
