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
            yield return StartCoroutine(CalculateAction(enemy));
            
            // for (int e = 1; e <= enemy.maxEnergy; e++) 
            // {
            //     yield return new WaitForSecondsRealtime(0.05f);
            //     yield return StartCoroutine(CalculateAction(enemy));
            // }
        }
        EndTurn();
    }

    public IEnumerator CalculateAction(EnemyUnit input) {
// First attack scan
        input.UpdateValidAttack(input.attackCard);
        foreach (Vector2 coord in input.validActionCoords) 
        {
            if (currentGrid.CoordContents(coord) is Unit t) {
                if (t.owner == Unit.Owner.Player) {
                    SelectUnit(input);
                    currentGrid.DisplayValidCoords(input.validActionCoords, 1);
                    foreach(Vector2 c in input.validActionCoords) {
                        if (currentGrid.CoordContents(c) is Unit u) {
                            u.TargetElement(true);
                        }
                    }
                    yield return new WaitForSecondsRealtime(0.5f);
                    yield return StartCoroutine(AttackWithUnit(selectedUnit, coord));
                    yield break;
                }
            }
        }
// Move scan
        input.UpdateValidMovement(input.moveCard);
        Unit closestTkn = scenario.player.units[0];
        if (closestTkn) {
            foreach (Unit tkn in scenario.player.units) {
                if (Vector2.Distance(tkn.coord, input.coord) < Vector2.Distance(closestTkn.coord, input.coord))
                    closestTkn = tkn;
                    Debug.Log(closestTkn.name);
            }
            Vector2 closestCoord = Vector2.one * -32;
            foreach(Vector2 coord in input.validActionCoords) {
                if (Vector2.Distance(coord, closestTkn.coord) < Vector2.Distance(closestCoord, closestTkn.coord)) 
                    closestCoord = coord;
            }
// If there is a valid closest coord
            if (Mathf.Sign(closestCoord.x) == 1) {
                SelectUnit(input);
                currentGrid.DisplayValidCoords(input.validActionCoords, 0);
                yield return new WaitForSecondsRealtime(0.5f);
                yield return StartCoroutine(MoveUnit(selectedUnit, closestCoord));
            }
        }
// Second attack scan
        input.UpdateValidAttack(input.attackCard);
        foreach (Vector2 coord in input.validActionCoords) 
        {
            if (currentGrid.CoordContents(coord) is Unit t) {
                if (t.owner == Unit.Owner.Player) {
                    SelectUnit(input);
                    currentGrid.DisplayValidCoords(input.validActionCoords, 1);
                    foreach(Vector2 c in input.validActionCoords) {
                        if (currentGrid.CoordContents(c) is Unit u) {
                            u.TargetElement(true);
                        }
                    }
                    yield return new WaitForSecondsRealtime(0.5f);
                    yield return StartCoroutine(AttackWithUnit(selectedUnit, coord));
                    yield break;
                }
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

    public override IEnumerator AttackWithUnit(Unit unit, Vector2 attackAt)
    {
        currentGrid.DisableGridHighlight();
        yield return base.AttackWithUnit(unit, attackAt);
        
    }

    public override IEnumerator MoveUnit(Unit unit, Vector2 moveTo, int cost = 0)
    {
        currentGrid.DisableGridHighlight();
        yield return base.MoveUnit(unit, moveTo);
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
