using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeButtonHoldHandler : MonoBehaviour, IUpdateSelectedHandler, IPointerDownHandler, IPointerUpHandler {

    UpgradeBranch branch;
    public Button upgradeButton;
    [SerializeField] Image radialProg;
    bool isPressed;
    [SerializeField] float holdDur;
    float confirmProg;


    void Start() {
        branch = GetComponentInParent<UpgradeBranch>();
        ResetProgress();
        DisplayProgress();
    }

    public void OnUpdateSelected(BaseEventData data) {
        if (upgradeButton.interactable == true) {
            if (isPressed) {
                ProgressConfirm();
            }
            DisplayProgress();
        } else {

        }
    }

    public void OnPointerDown(PointerEventData data) {
        isPressed = true;
        confirmProg = 0;
    }

    public void OnPointerUp(PointerEventData data) {
        isPressed = false;
        if (confirmProg < holdDur) confirmProg = 0;
    }

    public void ProgressConfirm() {
        if (confirmProg < holdDur) {
            confirmProg += Time.deltaTime;
        }
    }

    public void DisplayProgress() {
        radialProg.fillAmount = confirmProg/holdDur;
        if (confirmProg >= holdDur && upgradeButton.interactable) {
            upgradeButton.interactable = false;
            //upgradeButton.gameObject.GetComponent<Image>();
            branch.ProgressBranch();
        }
    }

    public void ResetProgress() {
        confirmProg = 0;
        DisplayProgress();
    }

}
