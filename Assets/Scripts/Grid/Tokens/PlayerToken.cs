using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerToken : Token {
    


    public override void UpdateValidMoves(Card card = null) {
        validMoveCoords = new List<Vector2>();


        foreach (Path path in card.data.paths) {
            bool blocked = false;
            for (int i = 0; i <= path.moveTo.Count - 1; i ++) {
                Vector2 newMove = new Vector2(coord.x + path.moveTo[i].x, coord.y + path.moveTo[i].y);
                if (newMove.x > Grid.gridSize - 1 || newMove.x < 0 || newMove.y > Grid.gridSize - 1 || newMove.y < 0)
                    blocked = true;
                foreach (GridElement ge in grid.gridElements) {
                    if (newMove == ge.coord) blocked = true;
                }
                if (!blocked) validMoveCoords.Add(newMove);
            }
        }
        grid.DisplayValidMoves(validMoveCoords);
    }


    public override void EnableSelection(bool state) {
        selectable = state;
        hitbox.enabled = state;
    }

}
