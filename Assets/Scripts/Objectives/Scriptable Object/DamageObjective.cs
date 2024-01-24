using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Objective/Damage")]
public class DamageObjective : Objective {

    public enum ObjectiveType { SlagDamage, NailDamage, ShieldDamage, HammerDamage, EnemyOnEnemyDamage };
    [Header("Damage Conditions")]
    [SerializeField] ObjectiveType objectiveType;


    public override Objective Init(SlagEquipmentData.UpgradePath path) {
        ObjectiveEventManager.AddListener<GridElementDamagedEvent>(OnDamage);
        return base.Init(path);
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
            case ObjectiveType.EnemyOnEnemyDamage:
                if (evt.element is EnemyUnit && evt.source is EnemyUnit && evt.damageType != GridElement.DamageType.Crush) progress++;
            break;
        }

        ProgressCheck();
    }

}
