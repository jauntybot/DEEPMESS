using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// This component is required by all grid elements

public class HPDisplay : MonoBehaviour 
{
    [SerializeField] bool disable;
    GridElement element;
    public GameObject hpDisplay;
    [SerializeField] Color hpPipColor;
    [SerializeField] GameObject hpPipContainer;
    [SerializeField] Color defensePipColor;
    [SerializeField] GameObject defensePipContainer;
    [SerializeField] GameObject pipPrefab;
    List<Image> hpPips = new List<Image>();
    List<Image> defensePips = new List<Image>();
    public GameObject defenseShield;

    void Start() 
    {
        if (!disable) {
            element = GetComponent<GridElement>();
            Initialize();

            UpdateHPDisplay();
            ToggleHPDisplay(false);
        }
    }

    public void Initialize() {
        for (int i = 0; i < element.hpMax; i++) {
            Image pip = Instantiate(pipPrefab, hpPipContainer.transform).GetComponent<Image>();
            pip.color = hpPipColor;
            hpPips.Add(pip);
            pip = Instantiate(pipPrefab, defensePipContainer.transform).GetComponent<Image>();
            pip.color = defensePipColor;
            defensePips.Add(pip);
        }
    }

    public virtual void UpdateHPDisplay() {
        if (!disable) {
            for (int i = 0; i < element.hpCurrent; i++)
                hpPips[i].enabled = true;
            for (int i = element.hpCurrent; i < element.hpMax; i++)
                hpPips[i].enabled = false;

            
            if (element.defense > 0) {
                defensePipContainer.SetActive(true);
                defenseShield.SetActive(true);
            }
            else {
                defensePipContainer.SetActive(false);
                defenseShield.SetActive(false);
            }
            for (int i = 0; i < element.defense; i++)
                defensePips[i].enabled = true;
            for (int i = element.defense; i < element.hpMax; i++)
                defensePips[i].enabled = false;
        }
    }

    public void ToggleHPDisplay(bool state) {
        if (!disable)
            hpDisplay.SetActive(state);
    }
}
