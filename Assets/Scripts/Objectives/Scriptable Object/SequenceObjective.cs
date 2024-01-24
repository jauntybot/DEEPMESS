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


    public override Objective Init(SlagEquipmentData.UpgradePath path) {
        switch(objectiveType) {
            default: break;
            case ObjectiveType.TwoKillDescend:
                ObjectiveEventManager.AddListener<GridElementDestroyedEvent>(TwoKillDescendCheck);
                ObjectiveEventManager.AddListener<FloorDescentEvent>(TwoKillDescendCheck);
                ObjectiveEventManager.AddListener<EndTurnEvent>(TwoKillDescendCheck);
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
        return base.Init(path);
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

    protected virtual void TwoKillDescendCheck(EndTurnEvent evt) {
        if (evt.toTurn == ScenarioManager.Turn.Player && !resolved) progress = 0;
        ProgressCheck();
    }

// Stand in blood
    protected virtual void StandInBloodCheck(UnitConditionEvent evt) {
        if (evt.condition == Unit.Status.Restricted && evt.target is PlayerUnit) {
            if (evt.undo) {
                progress--;
                if (progress < goal) resolved = false;  
            } 
            else progress++;
        }
        ProgressCheck();
    }

// Floors no damage
    protected virtual void NoDamageDescentCheck(GridElementDamagedEvent evt) {
        if (evt.element is PlayerUnit || evt.element is Nail) {
            floorFailed = true;
            progress = 0;
        }
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
