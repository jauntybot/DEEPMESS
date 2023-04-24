using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitOverview : MonoBehaviour
{

    public Unit unit;
    [Header("Canvas Elements")]
    public GameObject portraitPanel, overviewPanel;
    public TMPro.TMP_Text unitName;
    public Image portrait;
    public Image gfx;

    public UnitOverview Initialize(Unit u) {

        unit = u;
        unitName.text = u.name;
        portrait.sprite = u.portrait;
        gfx.sprite = u.gfx[0].sprite;
        
        return this;
    }

}
