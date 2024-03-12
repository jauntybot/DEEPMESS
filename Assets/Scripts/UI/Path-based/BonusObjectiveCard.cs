using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BonusObjectiveCard : ObjectiveCard {

    public bool nugget;
    [SerializeField] Sprite nuggetSprite, relicSprite;
    [SerializeField] Image rewardImage;
    [SerializeField] TooltipTrigger trigger;

    public override void Init(Objective _objective) {
        objective = _objective;
        objective.ObjectiveUpdateCallback += UpdateCard;

        if (objective.nuggetReward) {
            rewardImage.sprite = nuggetSprite;
            trigger.header = "Upgrade Nugget";
        } else {
            rewardImage.sprite = relicSprite;
            trigger.header = "Relic";
        }
        
        trigger.content = objective.objectiveString;
    }

    protected override void UpdateCard(Objective ob) {}
}
