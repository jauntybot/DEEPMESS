using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitUI : MonoBehaviour {

    public Unit unit;

    [Header("Canvas Elements")]
    public GameObject portraitPanel;
    public TMPro.TMP_Text unitName;
    public Image portrait;
    

    public virtual UnitUI Initialize(Unit u) {

        unit = u;
        gameObject.name = unit.name + " - Unit UI";
        unitName.text = u.name;
        portrait.sprite = u.portrait;


        u.ElementDestroyed += UnitDestroyed;
        return this;
    }

    public virtual void ToggleUnitPanel(bool active) {
        if (portraitPanel)
            portraitPanel.SetActive(active);
        //if (!active && equipmentPanel.activeSelf) ToggleEquipmentPanel(false);

    }


    private void UnitDestroyed(GridElement ge) {
        DestroyImmediate(gameObject);
    }

}
