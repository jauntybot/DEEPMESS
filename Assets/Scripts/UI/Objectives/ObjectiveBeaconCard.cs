using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveBeaconCard : ObjectiveCard {
    
    [SerializeField] Animator anim;
    [SerializeField] Button button;
    [SerializeField] TMPro.TMP_Text buttonTMP;
    

    public override void UpdateCard(Objective ob) {
        base.UpdateCard(ob);
        button.onClick.RemoveAllListeners();
        button.interactable = true;
        if (ob.succeeded) {
            anim.SetTrigger("Score");
            buttonTMP.text = "COLLECT";
            button.onClick.AddListener(Collect);
        } else {
            buttonTMP.text = "REROLL";
            button.onClick.AddListener(Reroll);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
    }

    public override void Unsub() {
        if (objective) {
            objective.ClearObjective();
            objective.ObjectiveUpdateCallback -= UpdateCard;
        }
    }

    public virtual void Collect() {
        Reroll(true);
    }
    
    public virtual void Reroll() {
        anim.SetTrigger("Reroll");
        GetComponentInParent<ObjectiveManager>().RollObjectiveCard(this, false);
        DisableButton();
    }
    
    public virtual void Reroll(bool collect = false) {
        GetComponentInParent<ObjectiveManager>().RollObjectiveCard(this, collect);
        DisableButton();
    }
    public virtual void DisableButton() {
        buttonTMP.text = "";
        button.onClick.RemoveAllListeners();
        button.interactable = false;
    }
}
