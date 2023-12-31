using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Objective/Damage")]
public class DamageObjective : Objective {

    public enum ObjectiveType { SlagDamage, NailDamage, ShieldDamage, HammerDamage };
    [Header("Damage Conditions")]
    [SerializeField] ObjectiveType objectiveType;


    public override void Init() {
        base.Init();
        ObjectiveEventManager.AddListener<GridElementDamagedEvent>(OnDamage);
    }

    protected virtual void OnDamage(GridElementDamagedEvent evt) {
        switch (objectiveType) {
            default:
            case ObjectiveType.SlagDamage:
                if (evt.damageType != GridElement.DamageType.Heal && !evt.element.shield && (evt.element is PlayerUnit || evt.element is Nail)) progress++;
            break;
            case ObjectiveType.NailDamage:
                if (evt.damageType != GridElement.DamageType.Heal && !evt.element.shield && evt.element is Nail) progress++;
            break;
            case ObjectiveType.ShieldDamage:
                if (evt.element.shield) progress++;
            break;
            case ObjectiveType.HammerDamage:
                if (evt.element is EnemyUnit && evt.sourceEquip is HammerData) progress++;
            break;
        }

        ProgressCheck();
    }

}
