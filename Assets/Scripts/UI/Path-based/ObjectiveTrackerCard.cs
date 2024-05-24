using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveTrackerCard : ObjectiveCard {

    [SerializeField] Color successColor, failColor;

    protected override void UpdateCard(Objective ob) {
        base.UpdateCard(ob);

        TMPro.FontStyles style = TMPro.FontStyles.Normal;
        Color color = Color.white;

        if (ob.resolved) {
            if (ob.succeeded) color = successColor;
            else {
                color = failColor;
                style = TMPro.FontStyles.Strikethrough;
            }
        }

        
        objectiveText.fontStyle = style;
        progressText.fontStyle = style;
        objectiveText.color = color;
        progressText.color = color;
    }

    public override void Unsub() {
        base.Unsub();
        Destroy(gameObject);
    }
}
