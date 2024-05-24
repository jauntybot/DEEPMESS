using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Objective/Basic")]
public class Objective : ScriptableObject {

    public string objectiveTitleString;
    public string objectiveString;
    public FloorChunk.PacketType chunk;
    public int progress, goal;
    public int _goal;
    public enum Operator { OrMore, LessThanOrEqual }
    public Operator operation;
    public bool resolved, succeeded;
    public delegate void OnObjectiveCondition(Objective objective);
    public event OnObjectiveCondition ObjectiveUpdateCallback;


    public virtual Objective Init(int p = 0) {
        Restart(p);
        return this;
    }

    public virtual void Restart(int p = 0) {
        resolved = false;
        succeeded = false;
        progress = 0;
        int sign = operation == Operator.OrMore ? -1 : 1;
        _goal = goal + p * sign;
    }

    public virtual void ProgressCheck(bool final = false) {
        if (!resolved) {
            switch (operation) {
                case Operator.OrMore:
                    if (progress >= _goal) {
                        resolved = true;
                        succeeded = true;
                    } 
                break;
                case Operator.LessThanOrEqual:
                    if (progress > _goal) {
                        resolved = true;
                        succeeded = false;
                    }
                break;
            }
            if (final && !resolved) {
                resolved = true;
                switch (operation) {
                    case Operator.OrMore:
                        if (progress >= _goal) 
                            succeeded = true;
                    break;
                    case Operator.LessThanOrEqual:
                        if (progress <= _goal) 
                            succeeded = true;
                    break;
                };
            }

            ObjectiveUpdateCallback?.Invoke(this);
        }
    }

    public virtual void ClearObjective() {
        Restart();
        
    }
}
