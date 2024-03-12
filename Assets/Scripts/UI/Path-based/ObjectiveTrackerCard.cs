using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveTrackerCard : ObjectiveCard {

    [SerializeField] Color successColor, failColor;

    public override void Init(Objective _objective) {
        objective = _objective;
        objective.ObjectiveUpdateCallback += UpdateCard;

        UpdateCard(objective);
    }

    protected override void UpdateCard(Objective ob) {
        base.UpdateCard(ob);

        TMPro.FontStyles style = TMPro.FontStyles.Normal;
        Color color = Color.white;

        if (ob.resolved) {
            if (ob.succeeded) color = successColor;
            else {
                color = failColor;
                style = TMPro.FontStyles.Strikethrough;
                nuggetAnim.GetComponent<Image>().color = new Color (0.75f, 0.75f, 0.75f, 1);
            }
        } else 
            nuggetAnim.GetComponent<Image>().color = Color.white;

        
        objectiveText.fontStyle = style;
        progressText.fontStyle = style;
        objectiveText.color = color;
        progressText.color = color;
    }
}
