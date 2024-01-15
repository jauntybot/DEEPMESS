using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NuggetSlot : MonoBehaviour {

    public Image radialFill;
    public bool filled;
    [SerializeField] GameObject sparks, filledSFX;
    [SerializeField] GameObject popUp;
    [SerializeField] TMP_Text titleTMP, modifierTMP;

    public void FillSlot() {
        filled = true;
        sparks.SetActive(true);
        filledSFX.SetActive(true);
    }

    
    public void DisplayPopup(bool active) {
        popUp.SetActive(active);
    }
    
    public void UpdateModifier(string title, string body) {
        titleTMP.text = title;
        modifierTMP.text = body;
    }




}
