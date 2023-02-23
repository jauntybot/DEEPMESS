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

    public GameObject dmgPanel;
    [SerializeField] Color dmgColor, healColor;
    public Animator dmgNumber;
    public TMPro.TMP_Text dmgText;

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
            hpText.text = element.hpCurrent.ToString();
            energyText.text = element.energyCurrent.ToString();
        }
    }

    public virtual IEnumerator DisplayDamageNumber(int dmg) {
        dmgPanel.SetActive(true);
        if (dmg > 0) {
            dmgText.text = "-" + dmg;
            dmgPanel.GetComponent<Image>().color = dmgColor;
        }
        else if (dmg < 0) {
            dmgText.text = "+" + Mathf.Abs(dmg);
            dmgPanel.GetComponent<Image>().color = healColor;
        }
        while (dmgPanel.activeSelf) {
            yield return null;
        }
    }

    public void ToggleStatsDisplay(bool state) {
        if (!disable)
            statDisplay.SetActive(state);
    }


}
