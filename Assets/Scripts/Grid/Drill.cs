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

    
}
