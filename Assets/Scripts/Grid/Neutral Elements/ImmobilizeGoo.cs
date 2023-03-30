using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImmobilizeGoo : GroundElement
{

    protected override void Start() {
        base.Start();
        transform.SetAsFirstSibling();
    }

    public override void OnSharedSpace(GridElement sharedWith) {
        base.OnSharedSpace(sharedWith);

        if (sharedWith is Unit u) {
            u.ApplyCondition(Unit.Status.Immobilized);
        }
    }
}
