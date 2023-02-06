using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// This component is required by all grid elements

public class HPDisplay : MonoBehaviour 
{
    [SerializeField] bool disable;
    GridElement element;
    public GameObject statDisplay, hp, energy;
    [SerializeField] TMPro.TMP_Text hpText;
    [SerializeField] TMPro.TMP_Text energyText;

    public GameObject defenseShield;

    void Start() 
    {
        if (!disable) {
            element = GetComponent<GridElement>();

            UpdateHPDisplay();
            ToggleHPDisplay(false);
        }
    }

    public virtual void UpdateHPDisplay() {
        if (!disable) {
            if (element.energyMax == 0) energy.SetActive(false);
            hpText.text = element.hpCurrent + "/" + element.hpMax;
            energyText.text = element.energyCurrent + "/" + element.energyMax;
        }
    }

    public void ToggleHPDisplay(bool state) {
        if (!disable)
            statDisplay.SetActive(state);
    }
}
