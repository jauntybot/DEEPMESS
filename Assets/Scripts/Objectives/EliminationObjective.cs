using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Objective/Eliminations")]
public class EliminationObjective : Objective {
    
    public enum ObjectiveType { EnemyEliminations, EnemyCrushes, EnemyOnEnemyEliminations, AnvilEliminations, WallEliminations };
    
    [Header("Enemy Conditions")]
    [SerializeField] ObjectiveType objectiveType;
    protected List<GridElement> targetElements;

    public override void Init() {
        base.Init();
        ObjectiveEventManager.AddListener<GridElementDestroyedEvent>(OnElimination);
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

            break;
            case ObjectiveType.AnvilEliminations:
                if (evt.element is Anvil) progress++;
            break;
            case ObjectiveType.WallEliminations:
                if (evt.element is Wall) progress++;
            break;
        }

        ProgressCheck();
    }

}
