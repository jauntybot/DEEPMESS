using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// This component is required by all grid elements
public class ElementCanvas : MonoBehaviour 
{
    [SerializeField] protected bool disable;
    protected GridElement element;
    int trackedHP;
    public GameObject statDisplay, hpContainer, hpPips, emptyHPPips, statusPips;
    [SerializeField] GameObject hpPipPrefab, apPipPrefab, dmgPipPrefab, emptyPipPrefab;
    //[SerializeField] GameObject badgePrefab;
    //[SerializeField] Sprite weakenedSprite, stunnedSprite;

    public GameObject dmgPanel;
    [SerializeField] Animator dmgAnim;

    [SerializeField] UnitOverview overview = null;

    public virtual void Initialize(GridElement ge) 
    {
        if (!disable) {
            element = ge;

            InstantiateMaxPips();
            UpdateStatsDisplay();
            ToggleStatsDisplay(false);
        }
        if (element is PlayerUnit u) {
            overview = u.ui.overview;
        } else if (element is Nail n) {
            overview = n.ui.overview;
        }
    }

    public virtual void InstantiateMaxPips() {
        if (!disable) {
            SizePipContainer(hpPips.GetComponent<RectTransform>());
            SizePipContainer(dmgPanel.GetComponent<RectTransform>());
            
            for (int i = element.hpMax - 1; i >= 0; i--) {
                Instantiate(emptyPipPrefab, emptyHPPips.transform);
                Instantiate(hpPipPrefab, hpPips.transform);
                Instantiate(dmgPipPrefab, dmgPanel.transform);
            }

            hpPips.SetActive(true);
        }
    }

    protected virtual void SizePipContainer(RectTransform rect) {
        rect.anchorMin = new Vector2(0.5f, 0.5f); rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        Vector3 delta = (element.hpMax <= 5) ? new Vector2((float)(0.2f * element.hpMax + 0.02 * (element.hpMax - 1)), 0.333f) : new Vector2((float)(0.2f * 5 + 0.02 * 4), 0.333f);
        rect.sizeDelta = (delta);
    }

    public virtual void UpdateStatsDisplay(int pre = -32) {
        if (!disable) {
            int cap = (pre == -32) ? element.hpCurrent - 1 : pre - 1;
            if (overview)
                overview.UpdateOverview(cap + 1);

            hpContainer.SetActive(true);
            for (int i = 0; i <= element.hpMax - 1; i++) {
                hpPips.transform.GetChild(i).gameObject.SetActive(i <= cap);
            }
// Status display
            // for (int i = statusPips.transform.childCount - 1; i >= 0; i--)
            //     Destroy(statusPips.transform.GetChild(i).gameObject);
            // if (element is Unit u) {
            //     if (u.conditions.Contains(Unit.Status.Weakened)) {
            //         GameObject badge = Instantiate(badgePrefab, statusPips.transform);
            //         badge.GetComponent<Image>().sprite = weakenedSprite;
            //     }
            // }
        }
    }

    public virtual IEnumerator DisplayDamageNumber(int dmg) {
      
        for (int i = dmgPanel.transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(dmgPanel.transform.GetChild(i).gameObject);
        
        dmgAnim.gameObject.SetActive(true);
        
        int r = element.hpCurrent - dmg;
// Element is damaged
        if (dmg > 0) {   
            UpdateStatsDisplay(r);        
            for (int i = 0; i <= element.hpMax - 1; i++) {
                GameObject pip = Instantiate(dmgPipPrefab, dmgPanel.transform);
                pip.GetComponent<Image>().enabled = i >= r;
                pip.gameObject.SetActive(i < element.hpCurrent);
            }
            dmgAnim.SetBool("dmg", true);         
// Element is healed
        } else if (dmg < 0) {
            for (int i = 0; i <= element.hpMax - 1; i++) {
                GameObject pip = Instantiate(hpPipPrefab, dmgPanel.transform);
                pip.GetComponent<Image>().enabled = i >= element.hpCurrent;
                pip.gameObject.SetActive(i <= r - 1);
            }
            dmgAnim.SetBool("dmg", false);
        }
        
        while (dmgAnim.gameObject.activeSelf) {
            yield return null;
        }
        UpdateStatsDisplay(r);
    }

    public void ToggleStatsDisplay(bool state) {
        if (!disable)
            statDisplay.SetActive(state);
    }


}
