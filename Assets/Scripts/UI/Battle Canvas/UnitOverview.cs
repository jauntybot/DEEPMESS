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
    [SerializeField] protected GameObject moveDisable, equipmentDisable, hammerPossession, hammerDisable;
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
        UpdateOverview(u.hpCurrent);

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

    public virtual void UpdateOverview(int value) {

        mini.color = unit.conditions.Contains(Unit.Status.Disabled) ? new Color(0.5f, 0.5f, 0.5f) : Color.white;
        hammerPossession.SetActive(unit.equipment.Find(e => e is HammerData) != null);
        hammerDisable.SetActive(unit.energyCurrent <= 0);
        
        moveDisable.SetActive(unit.moved);

        equipment.sprite = unit.equipment[1].icon;
        equipmentDisable.SetActive(unit.usedEquip || unit.energyCurrent <= 0);

        for (int i = 0; i <= unit.hpMax - 1; i++) 
            hpPips.transform.GetChild(i).gameObject.SetActive(i <= value);
        
    }


}
