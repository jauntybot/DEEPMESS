using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeTooltip : Tooltip {

    [SerializeField] Animator nugget;


    public virtual void SetText(string content = "", string header = "", bool clickToSkip = false, int nug = 0) {
        base.SetText(content, header, clickToSkip);
        nugget.SetInteger("Color", nug);
    }

}
