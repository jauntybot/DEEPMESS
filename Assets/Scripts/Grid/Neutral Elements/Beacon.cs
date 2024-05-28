using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beacon : GridElement {



    public override void TargetElement(bool state) {
        base.TargetElement(state);
        if (selectable)
            gfx[0].color = state ? Color.white : new Color(0.7f,0.7f,0.7f,1);
    }

    public IEnumerator SelectBeacon(PlayerManager pm) {
        UIManager.instance.ToggleBattleCanvas(false);
        yield return pm.scenario.objectiveManager.StartCoroutine(pm.scenario.objectiveManager.ObjectiveSequence());
        yield return pm.StartCoroutine(pm.upgradeManager.UpgradeSequence());
        StartCoroutine(DestroySequence());
        UIManager.instance.ToggleBattleCanvas(true);
    }

}
