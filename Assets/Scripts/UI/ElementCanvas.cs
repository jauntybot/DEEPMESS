using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// This component is required by all grid elements
public class ElementCanvas : MonoBehaviour 
{
    [SerializeField] protected bool disable;
    protected GridElement element;
    public GameObject statDisplay, hpPips, hpInt, apPips;
    [SerializeField] GameObject hpPipPrefab, apPipPrefab, dmgPipPrefab;
    [SerializeField] TMPro.TMP_Text hpText;

    public GameObject dmgPanel;
    [SerializeField] Animator dmgAnim;

    public virtual void Initialize(GridElement ge) 
    {
        if (!disable) {
            element = ge;

            UpdateStatsDisplay();
            ToggleStatsDisplay(false);
        }
        if (element is PlayerUnit) {
            apPips.SetActive(true);
        }
    }

    public virtual void UpdateStatsDisplay() {
        if (!disable) {
            if (element.hpCurrent <= 5) {
                hpPips.SetActive(true); hpInt.SetActive(false);
                int dif = element.hpCurrent - hpPips.transform.childCount;
                for (int i = Mathf.Abs(dif); i > 0; i--) {
                    if (dif < 0) {
                        if (hpPips.transform.childCount - i >= 0)
                        DestroyImmediate(hpPips.transform.GetChild(hpPips.transform.childCount - i).gameObject);
                    } else if (dif > 0) {
                        Instantiate(hpPipPrefab, hpPips.transform);
                    }
                }
            } else {
                hpPips.SetActive(false); hpInt.SetActive(true);
                hpText.text = element.hpCurrent.ToString();
            }
            if (element is PlayerUnit) {
                int dif = element.energyCurrent - apPips.transform.childCount;
                for (int i = Mathf.Abs(dif); i > 0; i--) {
                    if (dif < 0) {
                        if (apPips.transform.childCount - i >= 0)
                        DestroyImmediate(apPips.transform.GetChild(apPips.transform.childCount - i).gameObject);
                    } else if (dif > 0) {
                        Instantiate(apPipPrefab, apPips.transform);
                    }
                }
            }
        }
    }

    public virtual IEnumerator DisplayDamageNumber(int dmg) {
      
        for (int i = dmgPanel.transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(dmgPanel.transform.GetChild(i).gameObject);
        
        dmgAnim.gameObject.SetActive(true);
        
        int r = hpPips.transform.childCount > 0? hpPips.transform.childCount - 1: 0;   
// Element is damaged
        if (dmg > 0) {           
            for (int i = 0; i <= r; i++)
                Instantiate(dmgPipPrefab, dmgPanel.transform);
            dmgAnim.SetBool("dmg", true);         
            for (int i = 0; i <= r; i++)
                if (i <= r - dmg)
                    dmgPanel.transform.GetChild(i).GetComponent<Image>().enabled = false;
                else 
                    dmgPanel.transform.GetChild(i).GetComponent<Image>().enabled = true;
// Element is healed
        } else if (dmg < 0) {
            for (int i = 0; i <= r + Mathf.Abs(dmg); i++)
                Instantiate(hpPipPrefab, dmgPanel.transform);
            dmgAnim.SetBool("dmg", false);
            for (int i = 0; i <= r; i++)
                dmgPanel.transform.GetChild(i).GetComponent<Image>().enabled = false;
        }
        
        
        
        while (dmgAnim.gameObject.activeSelf) {
            yield return null;
        }
    }

    public void ToggleStatsDisplay(bool state) {
        if (!disable)
            statDisplay.SetActive(state);
    }


}
