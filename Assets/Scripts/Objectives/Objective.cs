using System.Collections;
using System.Collections.Generic;
using UnityEditor.Compilation;
using UnityEngine;

[CreateAssetMenu(menuName = "Objective/Basic")]
public class Objective : ScriptableObject {

    public string objectiveString;
    public int progress, goal;
    public bool resolved, succeeded;
    public delegate void OnObjectiveCondition(Objective objective);
    public event OnObjectiveCondition ObjectiveUpdateCallback;
    public event OnObjectiveCondition ObjectiveSuccessCallback;
    public event OnObjectiveCondition ObjectiveFailureCallback;


    public virtual void Init() {
        resolved = false;
        progress = 0;
    }

    public virtual void ProgressCheck(bool final = false) {
        if (!resolved) {
            if (progress >= goal) {
                //ObjectiveSuccessCallback?.Invoke(this);
                resolved = true;
            } else if (final) {
                //ObjectiveFailureCallback?.Invoke(this);
                resolved = true;
            } //else
            ObjectiveUpdateCallback?.Invoke(this);
        }
    }
}
