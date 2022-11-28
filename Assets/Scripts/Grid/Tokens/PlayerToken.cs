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
        List<Vector2> tempCoords = GetAdjacent(moveAdjacency, coord, moveRange, moveDirections);
// loop through complete coord list to remove invalid moves
        for (int i = tempCoords.Count - 1; i >= 0; i--) {
            bool valid = true;

// check if another grid element occupies this space
            foreach (GridElement ge in grid.gridElements) {
                if (tempCoords[i] == ge.coord) valid = false;
            }
// remove from list if invalid
            if (!valid) tempCoords.Remove(tempCoords[i]);
        }
        validMoveCoords = tempCoords;
        grid.DisplayValidCoords(validMoveCoords, 0);
    }

// Calculate and display valid attack coordinates
    public override void UpdateValidAttack() 
    {
        List<Vector2> tempCoords = GetAdjacent(attackAdjacency, coord, attackRange, attackDirections);
// loop through complete coord list to remove invalid moves
        for (int i = tempCoords.Count - 1; i >= 0; i--) {
            bool valid = true;

// check if a friendly grid element occupies this space
            foreach (GridElement ge in grid.gridElements) {
                if (ge is Token t) {
                    if (t.owner == Owner.Player && tempCoords[i] == ge.coord) 
                        valid = false;
                }
            }
// remove from list if invalid
            if (!valid) tempCoords.Remove(tempCoords[i]);
        }
        validAttackCoords = tempCoords;
        grid.DisplayValidCoords(validAttackCoords, 1);
    }
}
