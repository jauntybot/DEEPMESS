using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beacon : Unit {

    [SerializeField] SFX callDownSFX;
    public override event OnElementUpdate ElementDestroyed;

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
        yield return ScenarioManager.instance.objectiveManager.StartCoroutine(ScenarioManager.instance.objectiveManager.ObjectiveSequence(true));
        yield return new WaitForSecondsRealtime(0.5f);
        yield return ScenarioManager.instance.player.StartCoroutine(ScenarioManager.instance.player.upgradeManager.UpgradeSequence());
        PlaySound(callDownSFX); // Placeholder param for call down SFX
        yield return new WaitForSecondsRealtime(0.75f);
        yield return FloorManager.instance.StartCoroutine(FloorManager.instance.TransitionToSlimeHub(false));
        ScenarioManager.instance.currentTurn = ScenarioManager.Turn.Player;
        //  StartCoroutine(DestroySequence());
        UIManager.instance.ToggleBattleCanvas(true);
    }

    public override IEnumerator DestroySequence(DamageType dmgType = DamageType.Unspecified, GridElement source = null, GearData sourceEquip = null) {
        if (!destroyed) 
            destroyed = true;

        ElementDestroyed?.Invoke(this);
        ObjectiveEventManager.Broadcast(GenerateDestroyEvent(dmgType, source, sourceEquip));        
        
        gfxAnim.SetTrigger("Destroy");
        PlaySound(destroyedSFX);
        float timer = 0f;
        while (timer < 2f) {
            yield return null;
            timer += Time.deltaTime;
        }

        if (gameObject != null)
            Destroy(gameObject);
    }

}
