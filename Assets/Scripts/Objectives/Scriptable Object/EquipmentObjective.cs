using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Objective/Equipment")]
public class EquipmentObjective : Objective {

    public enum ObjectiveType { BigGrabThrows, EnemyExplosions };
    [Header("Equipment Conditions")] 
    [SerializeField] ObjectiveType objectiveType;


    public override Objective Init() {
        ObjectiveEventManager.AddListener<OnEquipmentUse>(OnEquipmentUse);
        return base.Init();
    }


    protected virtual void OnEquipmentUse(OnEquipmentUse evt) {
        switch (objectiveType) {
            case ObjectiveType.BigGrabThrows:
                if (evt.data is BigGrabData && evt.target is EnemyUnit) progress++;
            break;
            case ObjectiveType.EnemyExplosions:
                if (evt.data is SelfDetonate) progress++;
            break;
        }

        ProgressCheck();
    }
}
