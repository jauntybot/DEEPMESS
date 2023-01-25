using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Inherited Unit; unit functionality dependent on player input

public class PlayerUnit : Unit {
    
// Called when an action is applied to a unit or to clear it's actions
    public override void UpdateAction(int index) 
    {
// Clear data
        foreach(GridElement ge in grid.gridElements) {
            ge.TargetElement(ge == this);
        }

        base.UpdateAction(index);
// Update action data by card
        switch (index) {
            default: break;
            case 1:
                UpdateValidMovement(moveCard);
            break;
            case 2:
                UpdateValidAttack(attackCard);
            break;
            case 3:

            break;
        }
    }

// Allow the player to click on this
    public override void EnableSelection(bool state) 
    {
        selectable = state;
        hitbox.enabled = state;
    }

// Calculate and display valid move coordinates
    public override void UpdateValidMovement(CardData card) 
    {
        base.UpdateValidMovement(card);
        grid.DisplayValidCoords(validMoveCoords, 0);
    }

// Calculate and display valid attack coordinates
    public override void UpdateValidAttack(CardData card) 
    {
        base.UpdateValidAttack(card);
        grid.DisplayValidCoords(validAttackCoords, 1);
        foreach(Vector2 coord in validAttackCoords) {
            if (grid.CoordContents(coord) is Unit u) {
                u.TargetElement(true);
            }
        }
    }
}
