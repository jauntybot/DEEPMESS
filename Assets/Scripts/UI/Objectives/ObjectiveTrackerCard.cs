using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveTrackerCard : ObjectiveCard {

    public override void Init(Objective _objective) {
        objective = _objective;
        objective.ObjectiveUpdateCallback += UpdateCard;
        int c = 0;
        switch (_objective.reward) {
            default:
            case SlagEquipmentData.UpgradePath.Shunt: break;
            case SlagEquipmentData.UpgradePath.Scab: c = 1; break;
            case SlagEquipmentData.UpgradePath.Sludge: c = 2; break;
        }
        if (nuggetAnim)
            nuggetAnim.SetInteger("Color", c);

        UpdateCard(objective);
    } 

}
