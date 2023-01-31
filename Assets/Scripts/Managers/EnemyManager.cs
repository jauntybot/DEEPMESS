using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : UnitManager {

    public delegate void OnEnemyCondition(GridElement ge);
    public event OnEnemyCondition WipedOutCallback;

    public override IEnumerator Initialize()
    {
        if (ScenarioManager.instance) scenario = ScenarioManager.instance;
        if (FloorManager.instance) floorManager = FloorManager.instance;
        currentGrid = floorManager.currentFloor;
        yield return null;
    }

    public IEnumerator TakeTurn() {

        yield return new WaitForSecondsRealtime(1/Util.fps);
        for (int i = units.Count - 1; i >= 0; i--) 
        {
            EnemyUnit enemy = units[i] as EnemyUnit;
            for (int e = 1; e <= enemy.maxEnergy; e++) 
            {
                yield return new WaitForSecondsRealtime(0.05f);
                yield return StartCoroutine(CalculateAction(enemy));
            }
        }
        EndTurn();
    }

    public IEnumerator CalculateAction(EnemyUnit input) {
        input.UpdateValidAttack(input.attackCard);
        input.UpdateValidMovement(input.moveCard);
    
// Attack scan
        foreach (Vector2 coord in input.validAttackCoords) 
        {
            if (currentGrid.CoordContents(coord) is Unit t) {
                if (t.owner == Unit.Owner.Player) {
                    SelectUnit(input);
                    currentGrid.DisplayValidCoords(input.validAttackCoords, 1);
                    foreach(Vector2 c in input.validAttackCoords) {
                        if (currentGrid.CoordContents(c) is Unit u) {
                            u.TargetElement(true);
                        }
                    }
                    yield return new WaitForSecondsRealtime(0.75f);
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
                currentGrid.DisplayValidCoords(input.validMoveCoords, 0);
                yield return new WaitForSecondsRealtime(0.75f);
                yield return StartCoroutine(MoveUnit(closestCoord));
                yield break;
            }
        }
    }

    public void EndTurn() {
        if (selectedUnit)
           DeselectUnit(true);
        foreach (Unit unit in units) {
            unit.UpdateAction();
            unit.TargetElement(false);
        }

        StartCoroutine(scenario.SwitchTurns());
    }

    public override IEnumerator AttackWithUnit(Vector2 attackAt)
    {
        currentGrid.DisableGridHighlight();
        yield return base.AttackWithUnit(attackAt);
        
    }

    public override IEnumerator MoveUnit(Vector2 moveTo)
    {
        currentGrid.DisableGridHighlight();
        yield return base.MoveUnit(moveTo);
    }

    public virtual void SeedUnits(Grid newGrid) {
        for (int i = units.Count - 1; i >= 0; i--) {
            newGrid.enemy.units.Add(units[i]);
            newGrid.enemy.SubscribeElement(units[i]);

            units[i].transform.parent = newGrid.enemy.transform;
            units[i].StoreInGrid(newGrid);

            units.RemoveAt(i);
        }
        DestroyImmediate(this.gameObject);
    }
}
