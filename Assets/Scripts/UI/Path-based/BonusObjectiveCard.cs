using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonusObjectiveCard : ObjectiveCard {


    public override void Init(Objective _objective) {
        objective = _objective;
        objective.ObjectiveUpdateCallback += UpdateCard;
        UpdateCard(_objective);
        
    }

}
