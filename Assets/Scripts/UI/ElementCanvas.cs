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
    [SerializeField] GameObject hpPipPrefab, dmgPipPrefab;
    [SerializeField] TMPro.TMP_Text energyText;

    public GameObject dmgPanel;
    [SerializeField] Animator dmgAnim;

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
            int dif = element.hpCurrent - hp.transform.childCount;
            for (int i = Mathf.Abs(dif); i > 0; i--) {
                if (dif < 0) {
                    if (hp.transform.childCount - i >= 0)
                       DestroyImmediate(hp.transform.GetChild(hp.transform.childCount - i).gameObject);
                } else if (dif > 0) {
                    Instantiate(hpPipPrefab, hp.transform);
                }
            }
            energyText.text = element.energyCurrent.ToString();
        }
    }

    public virtual IEnumerator DisplayDamageNumber(int dmg) {
      
        for (int i = dmgPanel.transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(dmgPanel.transform.GetChild(i).gameObject);
        

// Element is damaged
        if (dmg > 0) {              
            for (int i = 0; i < hp.transform.childCount - 1; i++)
                Instantiate(dmgPipPrefab, dmgPanel.transform);
            dmgAnim.SetBool("dmg", true);         
            for (int i = 0; i < hp.transform.childCount - 1 - dmg; i++)
                dmgPanel.transform.GetChild(i).GetComponent<Image>().enabled = false;
// Element is healed
        } else if (dmg < 0) {
            for (int i = 0; i < hp.transform.childCount + Mathf.Abs(dmg) - 1; i++)
                Instantiate(hpPipPrefab, dmgPanel.transform);
            dmgAnim.SetBool("dmg", false);
            for (int i = 0; i <= hp.transform.childCount - 1; i++)
                dmgPanel.transform.GetChild(i).GetComponent<Image>().enabled = false;
        }
        
        dmgAnim.gameObject.SetActive(true);
        
        while (dmgAnim.gameObject.activeSelf) {
            yield return null;
        }
    }

    public void ToggleStatsDisplay(bool state) {
        if (!disable)
            statDisplay.SetActive(state);
    }


}
