using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Objective/Sequence")]
public class SequenceObjective : Objective {
    
    public enum ObjectiveType { TwoKillDescend, StandInBlood, FloorsNoDmg, PeekCount, DescendWithEnemies }
    [Header("Sequence Conditions")]
    [SerializeField] ObjectiveType objectiveType;

// Floors no damage
    bool floorFailed = false;


    public override void Init() {
        base.Init();
        switch(objectiveType) {
            default: break;
            case ObjectiveType.TwoKillDescend:
                ObjectiveEventManager.AddListener<GridElementDestroyedEvent>(TwoKillDescendCheck);
                ObjectiveEventManager.AddListener<FloorDescentEvent>(TwoKillDescendCheck);
            break;
            case ObjectiveType.StandInBlood:
                ObjectiveEventManager.AddListener<UnitConditionEvent>(StandInBloodCheck);
            break;
            case ObjectiveType.FloorsNoDmg:
                ObjectiveEventManager.AddListener<GridElementDamagedEvent>(NoDamageDescentCheck);
                ObjectiveEventManager.AddListener<FloorDescentEvent>(NoDamageDescentCheck);
            break;
            case ObjectiveType.PeekCount:
                ObjectiveEventManager.AddListener<FloorPeekEvent>(FloorPeek);
            break;
            case ObjectiveType.DescendWithEnemies:
                ObjectiveEventManager.AddListener<FloorDescentEvent>(DescendWithEnemies);
            break;
        }
    }

// Two kill descend
    protected virtual void TwoKillDescendCheck(GridElementDestroyedEvent evt) {
        if (evt.element is EnemyUnit && progress <= 1) progress++;
        ProgressCheck();
    }

    protected virtual void TwoKillDescendCheck(FloorDescentEvent evt) {
        if (progress >= 2) progress++;
        ProgressCheck();
        if (!resolved) progress = 0;
    }

// Stand in blood
    protected virtual void StandInBloodCheck(UnitConditionEvent evt) {
        if (evt.condition == Unit.Status.Restricted && evt.target is PlayerUnit) {
            if (evt.undo) progress--;
            else progress++;
        }
        ProgressCheck();
    }

// Floors no damage
    protected virtual void NoDamageDescentCheck(GridElementDamagedEvent evt) {
        if (evt.element is PlayerUnit || evt.element is Nail) floorFailed = true;
    }

    protected virtual void NoDamageDescentCheck(FloorDescentEvent evt) {
        if (!floorFailed) progress++;
        floorFailed = false;
        ProgressCheck();
    }

// Peek count
    protected virtual void FloorPeek(FloorPeekEvent evt) {
        if (evt.down) progress++;
        ProgressCheck();
    }

// Descend with enemies
    protected virtual void DescendWithEnemies(FloorDescentEvent evt) {
        if (evt.enemyDescentsCount > progress)
            progress = evt.enemyDescentsCount; 
        ProgressCheck();
    }

}
