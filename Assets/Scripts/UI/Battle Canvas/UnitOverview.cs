using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitOverview : MonoBehaviour {

    public Unit unit;
    [Header("Canvas Elements")]
    public GameObject overviewPanel;
    protected Button selectButton;
    public Image mini;
    [SerializeField] protected GameObject movePip, actionPip, hammerPossession, bulbPossession;
    [SerializeField] protected Transform hpPips, emptyHpPips;
    [SerializeField] protected GameObject hpPipPrefab, emptyPipPrefab;
    [SerializeField] Image bulbImage;
    [SerializeField] protected Sprite healBulb, supportBulb, debuffBulb;

    public virtual UnitOverview Initialize(Unit u, Transform overviewLayoutParent) {

        unit = u;
        gameObject.name = unit.name + " Overview";
        selectButton = overviewPanel.GetComponent<Button>();
        selectButton.onClick.AddListener(u.SelectUnitButton);


        overviewPanel.transform.SetParent(overviewLayoutParent);
        mini.sprite = u.gfx[0].sprite;

        InstantiateMaxPips();
        UpdateOverview(u.hpCurrent);

        return this;
    }

    public virtual void InstantiateMaxPips() {
        for (int i = emptyHpPips.transform.childCount - 1; i >= 0; i--)
            Destroy(emptyHpPips.transform.GetChild(i).gameObject);
        for (int i = hpPips.transform.childCount - 1; i >= 0; i--)
            Destroy(hpPips.transform.GetChild(i).gameObject);

        SizePipContainer(hpPips.transform.GetComponent<RectTransform>());
        for (int i = unit.hpMax - 1; i >= 0; i--) {
            Instantiate(emptyPipPrefab, emptyHpPips.transform);
            Instantiate(hpPipPrefab, hpPips.transform);
            RectTransform rect = hpPips.GetComponent<RectTransform>();
        }
        hpPips.gameObject.SetActive(true);
    }

    protected virtual void SizePipContainer(RectTransform rect) {
        rect.anchorMin = new Vector2(0.5f, 0.5f); rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        Vector3 delta = new Vector2((float)(14 * unit.hpMax + 2 * (unit.hpMax - 1)), 21);
        rect.sizeDelta = (delta);
    }

    public virtual void UpdateOverview(int value) {

        mini.color = unit.conditions.Contains(Unit.Status.Disabled) ? new Color(0.5f, 0.5f, 0.5f) : Color.white;
        hammerPossession.SetActive(unit.equipment.Find(e => e is HammerData) != null);
        BulbEquipmentData bulb = (BulbEquipmentData)unit.equipment.Find(e => e is BulbEquipmentData);
        bulbPossession.SetActive(bulb != null);
        if (bulb) {
            if (bulb is SupportBulbData sup) {
                if (sup.supportType == SupportBulbData.SupportType.Surge)
                    bulbImage.sprite = supportBulb;
                else if (sup.supportType == SupportBulbData.SupportType.Heal)
                    bulbImage.sprite = healBulb;
            } else if (bulb is DebuffBulbData)
                bulbImage.sprite = debuffBulb;  

        }

        movePip.SetActive(!unit.moved);
        actionPip.SetActive(unit.energyCurrent > 0);

        //if (unit.ui) equipment.sprite = unit.ui.equipButtons[0].data.icon;
        //equipmentDisable.SetActive(unit.usedEquip);

        for (int i = 0; i <= unit.hpMax - 1; i++) 
            hpPips.transform.GetChild(i).gameObject.SetActive(i < value);
    }


}
