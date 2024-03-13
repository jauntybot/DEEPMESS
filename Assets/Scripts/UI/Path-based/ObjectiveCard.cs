using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectiveCard : MonoBehaviour {

    public Objective objective;
    [SerializeField] protected Animator nuggetAnim;
    [SerializeField] protected TMP_Text objectiveTitle, objectiveText, progressText;


    public virtual void Init(Objective _objective) {
        objective = _objective;
        objective.ObjectiveUpdateCallback += UpdateCard;
        
        UpdateCard(objective);
    } 

    public virtual void Unsub() {
        if (objective) {
            objective.ClearObjective();
            objective.ObjectiveUpdateCallback -= UpdateCard;
        }
        Destroy(gameObject);
    }


    protected virtual void UpdateCard(Objective ob) {
        if (objectiveTitle)
            objectiveTitle.text = objective.objectiveTitleString;
        if (objectiveText) {
            objectiveText.text = objective.objectiveString;
            objectiveText.fontMaterial.color = Color.white;
        }
        if (progressText) {
            progressText.text = "("+objective.progress+"/"+objective._goal+")";
            progressText.fontMaterial.color = Color.white;
        }
        
    }


}
