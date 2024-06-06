using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveTrackerCard : ObjectiveCard {

    [SerializeField] Animator anim;

    public override void Init(Objective _objective) {
        base.Init(_objective);
        anim.SetTrigger("Reroll");
    }

    public override void UpdateCard(Objective ob) {
        base.UpdateCard(ob);
        if (ob.succeeded) anim.SetTrigger("Score");

    }

    public override void Unsub() {
        base.Unsub();
        Destroy(gameObject);
    }
}
