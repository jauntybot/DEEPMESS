using System.Collections;
using System.Collections.Generic;
using UnityEditor.Compilation;
using UnityEngine;

[CreateAssetMenu(menuName = "Objective/Basic")]
public class Objective : ScriptableObject {

    public string objectiveString;
    public int progress, goal;
    public bool resolved, succeeded;
    public SlagEquipmentData.UpgradePath reward;
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
            if (progress >= goal) {
                resolved = true;
                succeeded = true;
            } 
            if (final) 
                resolved = true;

            ObjectiveUpdateCallback?.Invoke(this);
        }
    }
}
