using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Objective/Exclusive")]
public class ExclusiveObjectives : Objective {

    Objective rolledObjective;
    [SerializeField] List<Objective> exclusiveObjectives;

    public override Objective Init(int p) {
        rolledObjective = exclusiveObjectives[Random.Range(0, exclusiveObjectives.Count)];
        return rolledObjective.Init(p);
    }


}
