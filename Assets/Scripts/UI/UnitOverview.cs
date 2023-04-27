using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitOverview : MonoBehaviour
{

    public Unit unit;
    [Header("Canvas Elements")]
    public GameObject overviewPanel;
    protected Button selectButton;
    public Image mini, equipment;
    [SerializeField] protected GameObject moveDisable, equipmentDisable;
    [SerializeField] protected Transform hpPips, emptyHpPips;
    [SerializeField] protected GameObject hpPipPrefab, emptyPipPrefab;

    public virtual UnitOverview Initialize(Unit u, Transform overviewLayoutParent) {

        unit = u;
        selectButton = overviewPanel.GetComponent<Button>();
        selectButton.onClick.AddListener(u.SelectUnitButton);


        overviewPanel.transform.SetParent(overviewLayoutParent);
        mini.sprite = u.gfx[0].sprite;
        equipment.sprite = unit.equipment[1].icon;

        InstantiateMaxPips();
        UpdateOverview();

        return this;
    }

    public virtual void InstantiateMaxPips() {
        for (int i = unit.hpMax - 1; i >= 0; i--) {
            Instantiate(emptyPipPrefab, emptyHpPips.transform);
            Instantiate(hpPipPrefab, hpPips.transform);
            RectTransform rect = hpPips.GetComponent<RectTransform>();
        }
        hpPips.gameObject.SetActive(true);
    }

    public virtual void UpdateOverview() {

        mini.sprite = unit.gfx[0].sprite;
        mini.color = unit.conditions.Contains(Unit.Status.Disabled) ? new Color(0.5f, 0.5f, 0.5f) : Color.white;
        
        moveDisable.SetActive(unit.moved);

        equipment.sprite = unit.equipment[1].icon;
        equipmentDisable.SetActive(unit.usedEquip || unit.energyCurrent <= 0);

        for (int i = 0; i <= unit.hpMax - 1; i++) 
            hpPips.transform.GetChild(i).gameObject.SetActive(i <= unit.hpCurrent - 1);
        
    }

    public void ToggleOverview(bool state) {
        overviewPanel.SetActive(state);
    }

}
