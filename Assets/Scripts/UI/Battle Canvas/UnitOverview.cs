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
    public HPPips hPPips;
    [SerializeField] protected GameObject movePip, actionPip, hammerPossession, bulbPossession;

    [SerializeField] Image bulbImage;
    [SerializeField] protected Sprite healBulb, supportBulb, debuffBulb;

    public virtual UnitOverview Initialize(Unit u, Transform overviewLayoutParent) {

        unit = u;
        gameObject.name = unit.name + " Overview";
        selectButton = overviewPanel.GetComponent<Button>();
        selectButton.onClick.AddListener(u.SelectUnitButton);

        overviewPanel.transform.SetParent(overviewLayoutParent);
        mini.sprite = u.gfx[0].sprite;

        hPPips.Init(u);
        UpdateOverview(u.hpCurrent);

        return this;
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

        hPPips.UpdatePips(value);
    }


}
