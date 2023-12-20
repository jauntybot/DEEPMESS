using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MetaDisplay : MonoBehaviour {

    [SerializeField] TMPro.TMP_Text enemiesRemainingText;
    [SerializeField] TMPro.TMP_Text floorNumberText;

    public void UpdateEnemiesRemaining(int enemiesRemaining) {    
        enemiesRemainingText.text = enemiesRemaining.ToString();
    }

    public void UpdateCurrentFloor(int currentFloor) {
        floorNumberText.text = currentFloor.ToString();
    }

}
