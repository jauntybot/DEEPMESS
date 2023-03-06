using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MetaDisplay : MonoBehaviour
{
    [SerializeField] TMPro.TMP_Text turnsLeftText;
    [SerializeField] TMPro.TMP_Text floorNumberText;
    [SerializeField] TMPro.TMP_Text dropChanceText;

    public void UpdateTurnsToDescend(int turnsToDescend) {    
        turnsLeftText.text = turnsToDescend.ToString();
    }

    public void UpdateCurrentFloor(int currentFloor) {
        floorNumberText.text = currentFloor.ToString();
    }

    public void UpdateDropChance(int chance) {
        dropChanceText.text = chance + "%";
    }
}
