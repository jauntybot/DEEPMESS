using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeButtonHoldHandler : MonoBehaviour, IUpdateSelectedHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler {

    UpgradeSlot slot;
    bool isPressed;
    [SerializeField] float holdDur;
    float confirmProg;


    public void Init(UpgradeSlot s) {
        slot = s;
        ResetProgress();
        DisplayProgress();
    }

    public void OnPointerEnter(PointerEventData data) {}

    public void OnPointerExit(PointerEventData data) {}

    public void OnUpdateSelected(BaseEventData data) {
        if (slot.selectable) {
            if (isPressed) {
                ProgressConfirm();
            }
            DisplayProgress();
        }
    }

    public void OnPointerDown(PointerEventData data) {
        if (slot.selectable) {
            isPressed = true;
            confirmProg = 0;
            slot.radialFill.GetComponent<AudioSource>().enabled = true;
           
        }
    }

    public void OnPointerUp(PointerEventData data) {
        isPressed = false;
        slot.radialFill.GetComponent<AudioSource>().enabled = false;
        if (confirmProg < holdDur) {
            confirmProg = 0;
        }
    }

    public void ProgressConfirm() {
        if (confirmProg < holdDur) {
            confirmProg += Time.deltaTime;
        }
    }

    public void DisplayProgress() {
        slot.radialFill.fillAmount = confirmProg/holdDur;

        if (confirmProg >= holdDur) {
            slot.ApplyUpgrade();
        }
    }

    public void ResetProgress() {
        confirmProg = 0;
        DisplayProgress();
    }

}
