using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Objective/Eliminations/Basic")]
public class EliminationObjective : Objective {

    protected List<GridElement> targetElements;

    protected virtual void OnElimination(GridElement ge) {
        
        ProgressCheck();
    }

}
