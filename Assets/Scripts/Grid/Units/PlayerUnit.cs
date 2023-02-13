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
        if (equipment)  {
            grid.DisplayValidCoords(validActionCoords, equipment.gridColor);
            canvas.ToggleEquipmentDisplay(false);
        }
// Update action data by card
        /*
        if (equipment) {
            canvas.ToggleEquipmentDisplay(false);
            switch (equipment.action) {
                default: break;
                case EquipmentData.Action.Move:
                    UpdateValidMovement(equipment);
                break;
                case EquipmentData.Action.Attack:
                    UpdateValidAttack(equipment);
                break;
            }
        }
        */
    }

// Allow the player to click on this
    public override void EnableSelection(bool state) 
    {
        selectable = state;
        hitbox.enabled = state;
    }


/*
// Calculate and display valid move coordinates
    public override void UpdateValidMovement(EquipmentData e) 
    {
        base.UpdateValidMovement(e);
        grid.DisplayValidCoords(validActionCoords, 0);
    }

// Calculate and display valid attack coordinates
    public override void UpdateValidAttack(EquipmentData e) 
    {
        base.UpdateValidAttack(e);
        grid.DisplayValidCoords(validActionCoords, 1);
        foreach(Vector2 coord in validActionCoords) {
            if (grid.CoordContents(coord) is Unit u) {
                u.TargetElement(true);
            }
        }
    }
*/  
    public override void TargetElement(bool state)
    {
        base.TargetElement(state);
        if (elementCanvas && energyCurrent > 0) {
            PlayerUnitCanvas canvas = (PlayerUnitCanvas)elementCanvas;
            canvas.ToggleEquipmentDisplay(state);
        }
    }
}
