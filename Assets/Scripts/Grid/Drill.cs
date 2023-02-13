using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Drill : PlayerUnit
{

    FloorManager floorManager;
    public MoveData drillDrop;

    protected override void Start()
    {
        base.Start();
        if (FloorManager.instance)
            floorManager = FloorManager.instance;
            selectedEquipment = equipment[0];
    }

    public override IEnumerator TakeDamage(int dmg, GridElement source)
    {
        if (source is PlayerUnit ally) {
            yield return null;
            StartCoroutine(floorManager.DescendFloors());
        } else
            yield return base.TakeDamage(dmg);
    }

}
