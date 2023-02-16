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
        input.UpdateAction(input.equipment[1]);
        foreach (Vector2 coord in input.validActionCoords) 
        {
            if (input.ValidCommand(coord)) {
                SelectUnit(input);
                GridElement target = selectedUnit.grid.CoordContents(coord);          
                currentGrid.DisplayValidCoords(input.validActionCoords, input.selectedEquipment.gridColor);
                yield return new WaitForSecondsRealtime(0.5f);
                Coroutine co = StartCoroutine(input.selectedEquipment.UseEquipment(input, target));
                DeselectUnit();
                currentGrid.DisableGridHighlight();
                yield return co;
                yield break;
            }
        }
        yield return new WaitForSecondsRealtime(2); 
// Move scan
        input.UpdateAction(input.equipment[0]);
        Vector2 targetCoord = input.SelectOptimalCoord(input.pathfinding);
        if (Mathf.Sign(targetCoord.x) == 1) {
            SelectUnit(input);
            currentGrid.DisplayValidCoords(input.validActionCoords, input.selectedEquipment.gridColor);
            yield return new WaitForSecondsRealtime(0.5f);
            Coroutine co = StartCoroutine(input.selectedEquipment.UseEquipment(input, currentGrid.sqrs.Find(sqr => sqr.coord == targetCoord)));
            DeselectUnit();
            currentGrid.DisableGridHighlight();
            yield return co;
        }

// Second attack scan
        input.UpdateAction(input.equipment[1]);
        foreach (Vector2 coord in input.validActionCoords) 
        {
            if (input.ValidCommand(coord)) {       
                SelectUnit(input);   
                GridElement target = selectedUnit.grid.CoordContents(coord);          
                currentGrid.DisplayValidCoords(input.validActionCoords, input.selectedEquipment.gridColor);
                yield return new WaitForSecondsRealtime(0.5f);
                Coroutine co = StartCoroutine(input.selectedEquipment.UseEquipment(input, target));
                DeselectUnit();
                currentGrid.DisableGridHighlight();
                yield return co;            
                yield return new WaitForSecondsRealtime(1);
            }
        }
        yield return new WaitForSecondsRealtime(2);
        currentGrid.DisableGridHighlight();

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

            units[i].transform.parent = newGrid.enemy.transform;
            units[i].StoreInGrid(newGrid);

            units.RemoveAt(i);
        }
        DestroyImmediate(this.gameObject);
    }
}
