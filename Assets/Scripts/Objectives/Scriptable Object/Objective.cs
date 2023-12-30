using System.Collections;
using System.Collections.Generic;
using UnityEditor.Compilation;
using UnityEngine;

[CreateAssetMenu(menuName = "Objective/Basic")]
public class Objective : ScriptableObject {

    public string objectiveString;
    public int progress, goal;
    public enum Operator { OrMore, LessThan }
    public Operator operation;
    public SlagEquipmentData.UpgradePath reward;
    public bool resolved, succeeded;
    public delegate void OnObjectiveCondition(Objective objective);
    public event OnObjectiveCondition ObjectiveUpdateCallback;


    public virtual void Init() {
        resolved = false;
        succeeded = false;
        progress = 0;
        reward = (SlagEquipmentData.UpgradePath)Random.Range(0, 3);
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
                if (progress < goal) succeeded = true;
            }

            ObjectiveUpdateCallback?.Invoke(this);
        }
    }
}
