using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NailDisplay : UnitOverview
{

    [SerializeField] Animator anim;
    [SerializeField] Animator emptyPips, hpPips;

    public override UnitOverview Initialize(Unit u, Transform overviewLayoutParent) {
        unit = u;
        selectButton = overviewPanel.GetComponent<Button>();
        selectButton.onClick.AddListener(u.SelectUnitButton);

        mini.sprite = u.gfx[0].sprite;

        UpdateOverview();

        return this;

    }

    public override void UpdateOverview() {
        Debug.Log("nail pips: " );
        Nail n = (Nail)unit;
        switch (n.nailState) {
            default: break;
            case Nail.NailState.Buried:
                anim.SetBool("Falling", false);
                anim.SetBool("Primed", false);
            break;
            case Nail.NailState.Primed:
                anim.SetBool("Falling", false);
                anim.SetBool("Primed", true);
            break;
            case Nail.NailState.Falling:
                anim.SetBool("Falling", true);
            break;
        }

        UpdatePips();    
    }

    void UpdatePips() {
        emptyPips.SetInteger("Count", unit.hpMax);
        hpPips.SetInteger("Count", unit.hpCurrent);
    }

}
