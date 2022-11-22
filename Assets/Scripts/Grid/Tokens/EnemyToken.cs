using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyToken : Token {

    [SerializeField] int moveRange;

    public override void UpdateValidMoves(Card card = null) {
        List<Vector2> tempCoords = new List<Vector2>();

        for (int i = 0; i < moveRange; i++) {
            tempCoords.Add(new Vector2 (coord.x + i, coord.y));
            tempCoords.Add(new Vector2 (coord.x - i, coord.y)); 
            tempCoords.Add(new Vector2 (coord.x, coord.y + i)); 
            tempCoords.Add(new Vector2 (coord.x, coord.y - i));
            for (int r = 0; r < i; r++) {
                tempCoords.Add(new Vector2 (coord.x + i, coord.y + r - i));
                tempCoords.Add(new Vector2 (coord.x - i, coord.y - r + i)); 
                tempCoords.Add(new Vector2 (coord.x + r - i, coord.y + i)); 
                tempCoords.Add(new Vector2 (coord.x - r + i, coord.y - i));
            }
        }

        for (int i = tempCoords.Count - 1; i >= 0; i--) {
            bool blocked = false;
            if (tempCoords[i].x > Grid.gridSize - 1 || tempCoords[i].x < 0 || tempCoords[i].y > Grid.gridSize - 1 || tempCoords[i].y < 0)
                blocked = true;
                foreach (GridElement ge in grid.gridElements) {
                    if (tempCoords[i] == ge.coord) blocked = true;
                }
            if (blocked) tempCoords.Remove(tempCoords[i]);
        }
        validMoveCoords = tempCoords;
        grid.DisplayValidMoves(validMoveCoords);
    }
}
