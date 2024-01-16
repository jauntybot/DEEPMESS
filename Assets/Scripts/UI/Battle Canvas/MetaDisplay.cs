using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MetaDisplay : MonoBehaviour {

    [SerializeField] TMPro.TMP_Text enemiesRemainingText;
    [SerializeField] TMPro.TMP_Text floorNumberText;
    int currentMax;

    public void UpdateEnemiesRemaining(int enemiesRemaining) {    
        enemiesRemainingText.text = enemiesRemaining.ToString();
    }

    public void UpdateCurrentFloor(int floorsGot, int packetLength) {
        if (Mathf.Sign(packetLength) != -1) currentMax = packetLength;
        floorNumberText.text = floorsGot + "/" + currentMax;
    }

}
