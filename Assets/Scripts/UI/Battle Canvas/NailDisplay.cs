using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NailDisplay : UnitOverview
{

    [SerializeField] Sprite buried, primed;

    public override UnitOverview Initialize(Unit u, Transform overviewLayoutParent) {
        unit = u;
        selectButton = overviewPanel.GetComponent<Button>();
        selectButton.onClick.AddListener(u.SelectUnitButton);

        mini.sprite = u.gfx[0].sprite;

        InstantiateMaxPips();
        UpdateOverview(u.hpCurrent);

        return this;

    }

    public override void InstantiateMaxPips() {
        for (int i = unit.hpMax - 1; i >= 0; i--) {
            GameObject pip = Instantiate(emptyPipPrefab, emptyHpPips.transform);
            //pip.transform.rotation = Quaternion.Euler(0,0,90);
            pip = Instantiate(hpPipPrefab, hpPips.transform);
            //pip.transform.rotation = Quaternion.Euler(0,0,90);
            
        }
        hpPips.gameObject.SetActive(true);
    }

    public override void UpdateOverview(int value)
    {
        Nail n = (Nail)unit;
        mini.sprite = n.nailState == Nail.NailState.Primed ? primed : buried;

        for (int i = 0; i <= unit.hpMax - 1; i++) 
            hpPips.transform.GetChild(i).gameObject.SetActive(i <= value);
    }

}
