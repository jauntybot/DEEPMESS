using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedManager : TokenManager {

    
    public IEnumerator TakeTurn() {

        yield return new WaitForSecondsRealtime(1/Util.fps);
        while (scenario.currentTurn == ScenarioManager.Turn.Enemy) {
            for (int i = tokens.Count - 1; i >= 0; i--) 
            {
                yield return new WaitForSecondsRealtime(1/Util.fps/2);
                yield return StartCoroutine(CalculateAction(tokens[i]));
            }
            EndTurn();
        }
    }

    public IEnumerator CalculateAction(Token input) {
        input.UpdateValidAttack();
        input.UpdateValidMovement();
    
    // Attack scan
        foreach (Vector2 coord in input.validAttackCoords) 
        {
            if (grid.CoordContents(coord) is Token t) {
                if (t.owner == Token.Owner.Player) {
                    selectedToken = input;
                    Debug.Log("enemy attacks");
                    yield return StartCoroutine(AttackWithToken(coord));
                    yield break;
                }
            }
        }


        Token closestTkn = scenario.player.tokens[0];
        if (closestTkn) {
            foreach (Token tkn in scenario.player.tokens) {
                if (Vector2.Distance(tkn.coord, input.coord) < Vector2.Distance(closestTkn.coord, input.coord))
                    closestTkn = tkn;
                    Debug.Log(closestTkn.name);
            }
            Vector2 closestCoord = Vector2.one * -16;
            foreach(Vector2 coord in input.validMoveCoords) {
                if (Vector2.Distance(coord, closestTkn.coord) < Vector2.Distance(closestCoord, closestTkn.coord)) 
                    closestCoord = coord;
            }
                // If there is a valid closest coord
            if (Mathf.Sign(closestCoord.x) == 1) {
                selectedToken = input;
                yield return StartCoroutine(MoveToken(closestCoord));
            }
        }
    }

    public void EndTurn() {
        selectedToken = null;
        foreach (Token token in tokens) {
            token.validAttackCoords = null;
            token.validMoveCoords = null;
        }

        scenario.SwitchTurns();
    }
}
