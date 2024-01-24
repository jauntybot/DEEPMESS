using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Objective/Basic")]
public class Objective : ScriptableObject {

    public string objectiveTitleString;
    public string objectiveString;
    public int progress, goal;
    public enum Operator { OrMore, LessThanOrEqual }
    public Operator operation;
    public SlagEquipmentData.UpgradePath reward;
    public bool resolved, succeeded;
    public delegate void OnObjectiveCondition(Objective objective);
    public event OnObjectiveCondition ObjectiveUpdateCallback;


    public virtual Objective Init(SlagEquipmentData.UpgradePath path) {
        reward = path;
        Restart();
        return this;
    }

    public virtual void Restart() {
        resolved = false;
        succeeded = false;
        progress = 0;
    }

    public virtual void ProgressCheck(bool final = false) {
        if (!resolved) {
            switch (operation) {
                case Operator.OrMore:
                    if (progress >= goal) {
                        resolved = true;
                        succeeded = true;
                    } 
                break;
                case Operator.LessThanOrEqual:
                    if (progress > goal) {
                        resolved = true;
                        succeeded = false;
                    }
                break;
            }
            if (final && !resolved) {
                resolved = true;
                switch (operation) {
                    case Operator.OrMore:
                        if (progress >= goal) 
                            succeeded = true;
                    break;
                    case Operator.LessThanOrEqual:
                        if (progress <= goal) 
                            succeeded = true;
                    break;
                };
            }

            ObjectiveUpdateCallback?.Invoke(this);
        }
    }
}
