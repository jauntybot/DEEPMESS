using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Objective/Basic")]
public class Objective : ScriptableObject {

    public string objectiveTitleString;
    public string objectiveString;
    public int progress, goal;
    public enum Operator { OrMore, LessThan }
    public Operator operation;
    public SlagEquipmentData.UpgradePath reward;
    public bool resolved, succeeded;
    public delegate void OnObjectiveCondition(Objective objective);
    public event OnObjectiveCondition ObjectiveUpdateCallback;


    public virtual void Init() {
        reward = (SlagEquipmentData.UpgradePath)Random.Range(0, 3);
        Restart();
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
                case Operator.LessThan:
                    if (progress >= goal) {
                        resolved = true;
                    }
                break;
            }
            if (final) {
                resolved = true;
                switch (operation) {
                    case Operator.OrMore:
                        if (progress >= goal) 
                            succeeded = true;
                    break;
                    case Operator.LessThan:
                        if (progress < goal) 
                            succeeded = true;
                    break;
                };
            }

            ObjectiveUpdateCallback?.Invoke(this);
        }
    }
}
