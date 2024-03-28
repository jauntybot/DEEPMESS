using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Objective/Eliminations")]
public class EliminationObjective : Objective {
    
    public enum ObjectiveType { EnemyEliminations, EnemyCrushes, EnemyOnEnemyEliminations, DestroyAnvils, DestroyWalls, AnvilEliminations, BigGrabEliminations, ShieldEliminations, DescentEliminations };
    [Header("Elimination Conditions")]
    [SerializeField] ObjectiveType objectiveType;

    public override Objective Init(bool reward, int p) {
        ObjectiveEventManager.AddListener<GridElementDestroyedEvent>(OnElimination);
        return base.Init(reward, p);
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
            case ObjectiveType.DescentEliminations:
                if (evt.element is EnemyUnit && ScenarioManager.instance.currentTurn == ScenarioManager.Turn.Descent) progress++;
            break;
        }

        ProgressCheck();
    }
    public override void ClearObjective() {
        base.ClearObjective();
        ObjectiveEventManager.RemoveListener<GridElementDestroyedEvent>(OnElimination);
    }

}
