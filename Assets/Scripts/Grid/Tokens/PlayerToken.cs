using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Inherited Token; token functionality dependent on player input

public class PlayerToken : Token {
    
// Called when an action is applied to a token or to clear it's actions
    public override void UpdateAction(Card card = null) 
    {
// Clear data
        validAttackCoords = null;
        validMoveCoords = null;
        grid.DisableGridHighlight();
// Update action data by card
        if (card) {
            switch (card.data.action) {
                default: break;
                case CardData.Action.Move:
                    UpdateValidMovement();
                break;
                case CardData.Action.Attack:
                    UpdateValidAttack();
                break;
                case CardData.Action.Defend:

                break;
            }
        }
    }

// Allow the player to click on this
    public override void EnableSelection(bool state) 
    {
        selectable = state;
        hitbox.enabled = state;
    }

// Calculate and display valid move coordinates
    public override void UpdateValidMovement() 
    {
        base.UpdateValidMovement();
        grid.DisplayValidCoords(validMoveCoords, 0);
    }

// Calculate and display valid attack coordinates
    public override void UpdateValidAttack() 
    {
        base.UpdateValidAttack();
        grid.DisplayValidCoords(validAttackCoords, 1);
    }
}
