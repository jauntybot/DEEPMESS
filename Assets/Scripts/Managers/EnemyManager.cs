using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : UnitManager {

    
    public IEnumerator TakeTurn() {

        yield return new WaitForSecondsRealtime(1/Util.fps);
        while (scenario.currentTurn == ScenarioManager.Turn.Enemy) {
            for (int i = units.Count - 1; i >= 0; i--) 
            {
                EnemyUnit enemy = units[i] as EnemyUnit;
                for (int e = 1; e <= enemy.maxEnergy; e++) 
                {
                    yield return new WaitForSecondsRealtime(0.1f);
                    yield return StartCoroutine(CalculateAction(enemy));
                }
            }
            EndTurn();
        }
    }

    public IEnumerator CalculateAction(EnemyUnit input) {
        input.UpdateValidAttack(input.attackCard);
        input.UpdateValidMovement(input.moveCard);
    
// Attack scan
        foreach (Vector2 coord in input.validAttackCoords) 
        {
            if (grid.CoordContents(coord) is Unit t) {
                if (t.owner == Unit.Owner.Player) {
                    SelectUnit(input);
                    grid.DisplayValidCoords(input.validAttackCoords, 1);
                    yield return new WaitForSecondsRealtime(0.4f);
                    yield return StartCoroutine(AttackWithUnit(coord));
                    yield break;
                }
            }
        }

// Move scan
        Unit closestTkn = scenario.player.units[0];
        if (closestTkn) {
            foreach (Unit tkn in scenario.player.units) {
                if (Vector2.Distance(tkn.coord, input.coord) < Vector2.Distance(closestTkn.coord, input.coord))
                    closestTkn = tkn;
                    Debug.Log(closestTkn.name);
            }
            Vector2 closestCoord = Vector2.one * -32;
            foreach(Vector2 coord in input.validMoveCoords) {
                if (Vector2.Distance(coord, closestTkn.coord) < Vector2.Distance(closestCoord, closestTkn.coord)) 
                    closestCoord = coord;
            }
// If there is a valid closest coord
            if (Mathf.Sign(closestCoord.x) == 1) {
                SelectUnit(input);
                grid.DisplayValidCoords(input.validMoveCoords, 0);
                yield return new WaitForSecondsRealtime(0.4f);
                yield return StartCoroutine(MoveUnit(closestCoord));
                yield break;
            }
        }
    }

    public void EndTurn() {
        if (selectedUnit)
           DeselectUnit();
        foreach (Unit unit in units) {
            unit.UpdateAction();
        }

        scenario.SwitchTurns();
    }

    public override IEnumerator AttackWithUnit(Vector2 attackAt)
    {
        grid.DisableGridHighlight();
        yield return base.AttackWithUnit(attackAt);
        
    }

    public override IEnumerator MoveUnit(Vector2 moveTo)
    {
        grid.DisableGridHighlight();
        yield return base.MoveUnit(moveTo);
    }
}
