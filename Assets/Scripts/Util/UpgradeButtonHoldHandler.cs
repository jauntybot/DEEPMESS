using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeButtonHoldHandler : MonoBehaviour, IUpdateSelectedHandler, IPointerDownHandler, IPointerUpHandler {

    UnitUpgradeUI ui;
    public Button upgradeButton;
    [SerializeField] Image radialProg;
    bool isPressed;
    [SerializeField] float holdDur;
    float confirmProg;


    public void Init(UnitUpgradeUI _ui) {
        ui = _ui;
        ResetProgress();
        DisplayProgress();
    }

    public void OnUpdateSelected(BaseEventData data) {
        if (ui.previewParticle) {
            if (isPressed) {
                ProgressConfirm();
            }
            DisplayProgress();
        } else {
            ResetProgress();
        }
    }

    public void OnPointerDown(PointerEventData data) {
        isPressed = true;
        confirmProg = 0;
        radialProg.transform.SetParent(ui.CurrentSlot());
        radialProg.transform.SetSiblingIndex(0);
        radialProg.transform.localPosition = Vector3.zero;
        if (ui.previewParticle) {
            Image image = ui.previewParticle.GetComponentInChildren<Image>();
            image.color = new Color(image.color.r, image.color.g, image.color.b, 1);
        }
    }

    public void OnPointerUp(PointerEventData data) {
        isPressed = false;
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
