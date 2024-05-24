using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Objective/Equipment")]
public class EquipmentObjective : Objective {

    public enum ObjectiveType { BigGrab, Shield, Anvil, EnemyExplosions };
    [Header("Equipment Conditions")] 
    [SerializeField] ObjectiveType objectiveType;


    public override Objective Init(int p) {
        ObjectiveEventManager.AddListener<OnEquipmentUse>(OnEquipmentUse);
        return base.Init(p);
    }


    protected virtual void OnEquipmentUse(OnEquipmentUse evt) {
        switch (objectiveType) {
            case ObjectiveType.BigGrab:
                if (evt.data is BigGrabData) progress++;
            break;
            case ObjectiveType.Anvil:
                if (evt.data is AnvilData) progress++;
            break;
            case ObjectiveType.Shield:
                if (evt.data is ShieldData) progress++;
            break;
            case ObjectiveType.EnemyExplosions:
                if (evt.data is SelfDetonate && evt.target == null) progress++;
            break;
        }

        ProgressCheck();
    }

    public override void ClearObjective() {
        base.ClearObjective();
        ObjectiveEventManager.RemoveListener<OnEquipmentUse>(OnEquipmentUse);
    }
}
