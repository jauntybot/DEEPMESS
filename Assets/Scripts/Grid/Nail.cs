using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Nail : Unit
{
    [Range(10,100)]
    public int collisionChance = 75;

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
