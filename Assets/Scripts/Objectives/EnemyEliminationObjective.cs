using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Objective/Eliminations/Enemy")]
public class EnemyEliminationObjective : EliminationObjective {
    
    public enum EnemyObjectiveType { Eliminations, Crushes, EnemyDamage };
    
    [Header("Enemy Conditions")]
    [SerializeField] EnemyObjectiveType objectiveType;
    ScenarioManager scenario;
    EnemyManager currentEnemy;

    public override void Init() {
        base.Init();
        if (ScenarioManager.instance)
            scenario = ScenarioManager.instance;
        scenario.floorManager.EnemySeeded += UpdateCurrentTargets;
    }

    void UpdateCurrentTargets() {
        currentEnemy = scenario.currentEnemy;    
        foreach(Unit enemy in currentEnemy.units) {
            if (!targetElements.Contains(enemy)) {
                targetElements.Add(enemy);
                enemy.ElementDestroyed += OnElimination;
            }
        }
    }

    protected override void OnElimination(GridElement ge) {
        switch (objectiveType) {
            default:
            case EnemyObjectiveType.Eliminations:
                if (ge is EnemyUnit) progress ++;
            break;
            case EnemyObjectiveType.Crushes:

            break;
            case EnemyObjectiveType.EnemyDamage:

            break;
        }

        base.OnElimination(ge);
    }

}
