using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : UnitManager {

    public delegate void OnEnemyCondition(GridElement ge);
    public event OnEnemyCondition WipedOutCallback;
    private Coroutine ongoingTurn;

    public override IEnumerator Initialize()
    {
        yield return base.Initialize();
    }

    public override Unit SpawnUnit(Vector2 coord, Unit unit)
    {
        Unit u = base.SpawnUnit(coord, unit);
        u.ElementDestroyed += DescentTriggerCheck;
        return u;
    }

    public void DescentTriggerCheck(GridElement ge) {
        if (units.Count <= 0) {
            Debug.Log("Trigger descent");
            EndTurnEarly();
            scenario.player.TriggerDescent();
        }
    }

    public IEnumerator TakeTurn(bool scatter) {

// Reset manager and units for turn
        foreach(Unit u in units) {
            u.energyCurrent = u.energyMax;
            if (!u.conditions.Contains(Unit.Status.Immobilized))
                u.moved = false;
            u.elementCanvas.UpdateStatsDisplay();
        }

        yield return new WaitForSecondsRealtime(1/Util.fps);
// Loop through each unit to take it's action
        for (int i = units.Count - 1; i >= 0; i--) 
        {
// Check if the player loses before continuing turn
            bool lose = true;
            foreach (Unit u in scenario.player.units) {
                if (u is not Nail && !u.conditions.Contains(Unit.Status.Disabled)) {
                    lose = false;
                    break;
                }
            }
            if (lose || scenario.player.units.Find(u => u is Nail) == null) {
                EndTurnEarly();
                break;
            }
// Take either scatter action or normal action
            EnemyUnit enemy = units[i] as EnemyUnit;
            if (!scatter) {
                ongoingTurn = StartCoroutine(CalculateAction(enemy));
                yield return ongoingTurn;
                yield return new WaitForSecondsRealtime(0.125f);
            } else {
                ongoingTurn = StartCoroutine(ScatterTurn(enemy));
                yield return ongoingTurn;
                yield return new WaitForSecondsRealtime(0.125f);
            }
        }
        EndTurn();
    }

    public void EndTurnEarly() {
        if (ongoingTurn != null) {
            StopCoroutine(ongoingTurn);
            ongoingTurn = null;
        }
    }

    public IEnumerator CalculateAction(EnemyUnit input) {
// First attack scan
        input.UpdateAction(input.equipment[1]);
        foreach (Vector2 coord in input.validActionCoords) 
        {
            if (input.ValidCommand(coord, input.selectedEquipment)) {
                SelectUnit(input);
                GridElement target = null;
                foreach (GridElement ge in selectedUnit.grid.CoordContents(coord))
                    target = ge;
                currentGrid.DisplayValidCoords(input.validActionCoords, input.selectedEquipment.gridColor);
                yield return new WaitForSecondsRealtime(0.5f);
                Coroutine co = StartCoroutine(input.selectedEquipment.UseEquipment(input, target));
                currentGrid.UpdateSelectedCursor(false, Vector2.one * -32);
                currentGrid.DisableGridHighlight();
                yield return co;
                DeselectUnit();
                yield return new WaitForSecondsRealtime(1.25f);
                yield break;
            }
        }
// Move scan
        if (!input.moved) {
            input.UpdateAction(input.equipment[0], input.moveMod);
            Vector2 targetCoord = input.SelectOptimalCoord(input.pathfinding);
            if (Mathf.Sign(targetCoord.x) == 1) {
                SelectUnit(input);
                currentGrid.DisplayValidCoords(input.validActionCoords, input.selectedEquipment.gridColor);
                yield return new WaitForSecondsRealtime(0.5f);
                Coroutine co = StartCoroutine(input.selectedEquipment.UseEquipment(input, currentGrid.sqrs.Find(sqr => sqr.coord == targetCoord)));
                currentGrid.UpdateSelectedCursor(false, Vector2.one * -32);
                currentGrid.DisableGridHighlight();
                yield return co;
                DeselectUnit();
            }
        }

// Second attack scan
        input.UpdateAction(input.equipment[1]);
        foreach (Vector2 coord in input.validActionCoords) 
        {
            if (input.ValidCommand(coord, input.selectedEquipment)) {       
                SelectUnit(input);   
                GridElement target = null;
                foreach (GridElement ge in selectedUnit.grid.CoordContents(coord))
                    target = ge;
                currentGrid.DisplayValidCoords(input.validActionCoords, input.selectedEquipment.gridColor);
                yield return new WaitForSecondsRealtime(0.5f);
                Coroutine co = StartCoroutine(input.selectedEquipment.UseEquipment(input, target));
                currentGrid.UpdateSelectedCursor(false, Vector2.one * -32);
                currentGrid.DisableGridHighlight();
                yield return co;            
                DeselectUnit();
                yield return new WaitForSecondsRealtime(1.25f);
            }
        }
        currentGrid.DisableGridHighlight();
        Debug.Log("Enemy finished");
    }

    public IEnumerator ScatterTurn(EnemyUnit input) {
        input.UpdateAction(input.equipment[0], input.moveMod);
        Vector2 targetCoord = input.SelectOptimalCoord(EnemyUnit.Pathfinding.Random);
        if (Mathf.Sign(targetCoord.x) == 1) {
            SelectUnit(input);
            currentGrid.DisplayValidCoords(input.validActionCoords, input.selectedEquipment.gridColor);
            yield return new WaitForSecondsRealtime(0.5f);
            Coroutine co = StartCoroutine(input.selectedEquipment.UseEquipment(input, currentGrid.sqrs.Find(sqr => sqr.coord == targetCoord)));
            currentGrid.UpdateSelectedCursor(false, Vector2.one * -32);
            currentGrid.DisableGridHighlight();
            yield return co;
            DeselectUnit();
        }
    }

    public void EndTurn() {
        if (selectedUnit)
           DeselectUnit();
        foreach (Unit unit in units) {
            unit.UpdateAction();
            unit.TargetElement(false);
        }

        StartCoroutine(scenario.SwitchTurns());
    }

    public virtual void SeedUnits(Grid newGrid) {
        for (int i = units.Count - 1; i >= 0; i--) {
            newGrid.enemy.units.Add(units[i]);
            newGrid.enemy.SubscribeElement(units[i]);
            units[i].manager = newGrid.enemy;

            units[i].transform.parent = newGrid.enemy.transform;
            units[i].StoreInGrid(newGrid);
            units[i].UpdateElement(units[i].coord);
            units.RemoveAt(i);
        }
        UIManager.instance.metaDisplay.UpdateEnemiesRemaining(newGrid.enemy.units.Count);
        DestroyImmediate(this.gameObject);
    }

    protected override void RemoveUnit(GridElement ge)
    {
        base.RemoveUnit(ge);
        UIManager.instance.metaDisplay.UpdateEnemiesRemaining(units.Count);
    }
}
