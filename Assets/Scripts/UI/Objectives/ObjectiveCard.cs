using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectiveCard : MonoBehaviour {

    public Objective objective;
    [SerializeField] Image reward;
    [SerializeField] TMP_Text objectiveText, progressText;


    public virtual void Init(Objective _objective, Sprite rewardSprite) {
        objective = _objective;
        objective.ObjectiveUpdateCallback += UpdateCard;
        if (reward)
            reward.sprite = rewardSprite;

        UpdateCard(objective);
    } 


    void UpdateCard(Objective ob) {
        objectiveText.text = objective.objectiveString;
        progressText.text = "("+objective.progress+"/"+objective.goal+")";
        objectiveText.fontMaterial.color = Color.white;
        progressText.fontMaterial.color = Color.white;
        
        TMPro.FontStyles style = TMPro.FontStyles.Normal;
        if (ob.resolved) 
            style = TMPro.FontStyles.Strikethrough;
        Color color = Color.white;
        if (ob.resolved) {
            if (ob.succeeded) color = Color.green;
            else color = Color.red;
        }

        objectiveText.fontStyle = style;
        progressText.fontStyle = style;
        objectiveText.color = color;
        progressText.color = color;
    }


}
