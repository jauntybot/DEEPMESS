using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// This component is required by all grid elements

public class HPDisplay : MonoBehaviour 
{
    [SerializeField] GridElement element;
    public GameObject hpDisplay;
    public Slider hpSlider, defenseSlider;
    public GameObject defenseShield;

    void Start() 
    {
        element = GetComponent<GridElement>();
        UpdateHPDisplay();
        ToggleHPDisplay(false);
    }

    public virtual void UpdateHPDisplay() {
        if (hpSlider) {
            hpSlider.maxValue = element.hpMax;
            hpSlider.value = element.hpCurrent;
        }
        if (defenseSlider) {
            if (element.defense > 0) {
                defenseSlider.gameObject.SetActive(true);
                defenseShield.SetActive(true);
            }
            else {
                defenseSlider.gameObject.SetActive(false);
                defenseShield.SetActive(false);
            }
            defenseSlider.maxValue = element.hpMax;
            defenseSlider.value = element.defense;
        }
    }

    public void ToggleHPDisplay(bool state) {
        if (hpDisplay)
            hpDisplay.SetActive(state);
    }
}
