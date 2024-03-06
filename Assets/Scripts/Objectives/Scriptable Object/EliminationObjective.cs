using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Objective/Eliminations")]
public class EliminationObjective : Objective {
    
    public enum ObjectiveType { EnemyEliminations, EnemyCrushes, EnemyOnEnemyEliminations, DestroyAnvils, DestroyWalls, AnvilEliminations, BigGrabEliminations, ShieldEliminations };
    [Header("Elimination Conditions")]
    [SerializeField] ObjectiveType objectiveType;

    public override Objective Init() {
        ObjectiveEventManager.AddListener<GridElementDestroyedEvent>(OnElimination);
        return base.Init();
    }

    protected virtual void OnElimination(GridElementDestroyedEvent evt) {
        switch (objectiveType) {
            default:
            case ObjectiveType.EnemyEliminations:
                if (evt.element is EnemyUnit) progress++;
            break;
            case ObjectiveType.EnemyCrushes:
                if (evt.element is EnemyUnit && evt.damageType == GridElement.DamageType.Crush) progress++;
            break;
            case ObjectiveType.EnemyOnEnemyEliminations:
            if (evt.source) {
                if (evt.element is EnemyUnit && evt.source is EnemyUnit) progress++;
            }
            break;
            case ObjectiveType.DestroyAnvils:
                if (evt.element is Anvil) progress++;
            break;
            case ObjectiveType.DestroyWalls:
                if (evt.element is Wall) progress++;
            break;
            case ObjectiveType.AnvilEliminations:
            if (evt.source) {
                if (evt.element is EnemyUnit && evt.source is Anvil) progress++;
            }
            break;
            case ObjectiveType.BigGrabEliminations:
            if (evt.sourceEquip) {
                if (evt.element is EnemyUnit && evt.sourceEquip is BigGrabData) progress++;
            }
            break;
            case ObjectiveType.ShieldEliminations:
                if (evt.source && evt.sourceEquip) {
                    if (evt.element is EnemyUnit && (evt.source.shield || evt.sourceEquip is ShieldData)) progress++;
                }
            break;
        }

        ProgressCheck();
    }

}
