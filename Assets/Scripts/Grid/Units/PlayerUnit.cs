using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Inherited Unit; unit functionality dependent on player input

public class PlayerUnit : Unit {

    [HideInInspector] public PlayerUnitCanvas canvas;
    
    protected override void Start() {
        base.Start();
        canvas = (PlayerUnitCanvas)elementCanvas;
    }

// Called when an action is applied to a unit or to clear it's actions
    public override void UpdateAction(EquipmentData equipment = null) 
    {
        base.UpdateAction(equipment);
    }

// Allow the player to click on this
    public override void EnableSelection(bool state) 
    {
        selectable = state;
        hitbox.enabled = state;
    }

    public override void TargetElement(bool state)
    {
        base.TargetElement(state);
        PlayerUnitCanvas canvas = (PlayerUnitCanvas)elementCanvas;
        canvas.ToggleEquipmentDisplay(state);
        if (energyCurrent == 0) canvas.ToggleEquipmentDisplay(false);
    }
}
