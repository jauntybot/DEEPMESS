using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectiveUI : MonoBehaviour {


    [SerializeField] Image reward;
    [SerializeField] TMP_Text objectiveText, progressText;


    public void Init(string objective, int goal, Sprite _reward) {
        reward.sprite = _reward;
        objectiveText.text = objective;
        progressText.text = "(0/"+goal+")";
        objectiveText.fontMaterial.color = Color.white;
        progressText.fontMaterial.color = Color.white;
    } 


}
