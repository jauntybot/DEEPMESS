using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Objective/Eliminations")]
public class EliminationObjective : Objective {

    void OnElimination() {
        

        progress++;
    }

}
