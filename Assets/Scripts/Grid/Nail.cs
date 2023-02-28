using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Nail : Unit
{

    FloorManager floorManager;
    public MoveData nailDrop;

    protected override void Start()
    {
        base.Start();
        if (FloorManager.instance)
            floorManager = FloorManager.instance;
            selectedEquipment = equipment[0];
    }

    
}
