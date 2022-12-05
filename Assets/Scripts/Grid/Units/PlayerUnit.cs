using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Inherited Unit; unit functionality dependent on player input

public class PlayerUnit : Unit {
    
// Called when an action is applied to a unit or to clear it's actions
    public override void UpdateAction(Card card = null) 
    {
// Clear data
        base.UpdateAction(card);
// Update action data by card
        if (card) {
            switch (card.data.action) {
                default: break;
                case CardData.Action.Move:
                    UpdateValidMovement(card.data);
                break;
                case CardData.Action.Attack:
                    UpdateValidAttack(card.data);
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
    }
}
