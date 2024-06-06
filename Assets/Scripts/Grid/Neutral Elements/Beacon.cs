using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beacon : Unit {

    

    public override void TargetElement(bool state) {
        base.TargetElement(state);
        if (selectable)
            gfx[0].color = state ? Color.white : new Color(0.7f,0.7f,0.7f,1);
    }

    public IEnumerator SelectBeacon() {
        UIManager.instance.ToggleBattleCanvas(false);
        ScenarioManager.instance.currentTurn = ScenarioManager.Turn.Event;
        yield return FloorManager.instance.StartCoroutine(FloorManager.instance.TransitionToSlimeHub(true));
        yield return new WaitForSecondsRealtime(1f);
        yield return ScenarioManager.instance.objectiveManager.StartCoroutine(ScenarioManager.instance.objectiveManager.ObjectiveSequence());
        yield return new WaitForSecondsRealtime(0.5f);
        yield return ScenarioManager.instance.player.StartCoroutine(ScenarioManager.instance.player.upgradeManager.UpgradeSequence());
        yield return FloorManager.instance.StartCoroutine(FloorManager.instance.TransitionToSlimeHub(false));
        ScenarioManager.instance.currentTurn = ScenarioManager.Turn.Player;
        StartCoroutine(DestroySequence());
        UIManager.instance.ToggleBattleCanvas(true);
    }

}
