using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// This component is required by all grid elements
public class ElementCanvas : MonoBehaviour 
{
    [SerializeField] protected bool disable;
    protected GridElement element;
    public GameObject statDisplay, hp, energy;
    [SerializeField] TMPro.TMP_Text hpText;
    [SerializeField] TMPro.TMP_Text energyText;

    public virtual void Initialize(GridElement ge) 
    {
        if (!disable) {
            element = ge;

            UpdateStatsDisplay();
            ToggleStatsDisplay(false);
        }
    }

    public virtual void UpdateStatsDisplay() {
        if (!disable) {
            if (element.energyMax == 0) energy.SetActive(false);
            hpText.text = element.hpCurrent + "/" + element.hpMax;
            energyText.text = element.energyCurrent + "/" + element.energyMax;
        }
    }

    public void ToggleStatsDisplay(bool state) {
        if (!disable)
            statDisplay.SetActive(state);
    }


}
