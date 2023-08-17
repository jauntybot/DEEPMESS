using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : UnitManager {


    [SerializeField] List<Unit> unitsToAct = new List<Unit>();
    public delegate void OnEnemyCondition(GridElement ge);
    public event OnEnemyCondition WipedOutCallback;
    protected Coroutine ongoingTurn;

    public override IEnumerator Initialize(Grid _currentGrid)
    {
        yield return base.Initialize(_currentGrid);
    }

    public override Unit SpawnUnit(Vector2 coord, Unit unit)
    {
        Unit u = base.SpawnUnit(coord, unit);
        u.ElementDestroyed += DescentTriggerCheck;
        u.ElementDestroyed += CountDefeatedEnemy; 
        return u;
    }

    void CountDefeatedEnemy(GridElement ge) {
        scenario.player.defeatedEnemies++;
    }

    public virtual Unit SpawnBossUnit(Vector2 coord, Unit unit) {
        Unit u = SpawnUnit(coord, unit);
        
        u.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = 0;
        return u;
    }

    public virtual void DescentTriggerCheck(GridElement ge = null) {
        if (scenario.currentEnemy == this && scenario.currentTurn != ScenarioManager.Turn.Descent) {
            if (units.Count <= 0) {
                EndTurnEarly();
                floorManager.Descend(false, false);
            }
        }
    }

    public virtual IEnumerator TakeTurn(bool scatter) {
// Reset manager and units for turn
        foreach(Unit u in units) {
            u.energyCurrent = u.energyMax;
            if (!u.conditions.Contains(Unit.Status.Immobilized))
                u.moved = false;
            u.elementCanvas.UpdateStatsDisplay();
        }
        ResolveConditions();

        yield return new WaitForSecondsRealtime(1/Util.fps);

        unitsToAct = new List<Unit>();
        for (int i = units.Count - 1; i >= 0; i--)
            unitsToAct.Add(units[i]);

// Loop through each unit to take it's action
        
        while (unitsToAct.Count > 0) {
// Check if the player loses before continuing turn
            bool lose = true;
            foreach (Unit u in scenario.player.units) {
                if (u is not Nail && !u.conditions.Contains(Unit.Status.Disabled)) {
                    lose = false;
                    break;
                }
            }
            if (lose || scenario.player.nail.conditions.Contains(Unit.Status.Disabled)) {
                EndTurnEarly();
                break;
            }
// Take either scatter action or normal action
            EnemyUnit enemy = unitsToAct[0] as EnemyUnit;
            unitsToAct.Remove(enemy);
            if (!scatter) {
                ongoingTurn = StartCoroutine(enemy.CalculateAction());
                yield return ongoingTurn;
                yield return new WaitForSecondsRealtime(0.125f);
            } else {
                ongoingTurn = StartCoroutine(enemy.ScatterTurn());
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
        EnemyManager eManager = (EnemyManager) newGrid.enemy;
        for (int i = units.Count - 1; i >= 0; i--) {
            newGrid.enemy.units.Add(units[i]);

// Update subscriptions
            newGrid.enemy.SubscribeElement(units[i]);
            units[i].manager = eManager;
            units[i].ElementDestroyed += eManager.DescentTriggerCheck;
            units[i].ElementDestroyed -= DescentTriggerCheck;
            units[i].ElementDestroyed -= currentGrid.RemoveElement;

            units[i].transform.parent = newGrid.enemy.transform;
            units[i].StoreInGrid(newGrid);
            units[i].UpdateElement(units[i].coord);
            units.RemoveAt(i);
        }
        eManager.DescentTriggerCheck();
        UIManager.instance.metaDisplay.UpdateEnemiesRemaining(newGrid.enemy.units.Count);
        Destroy(this.gameObject);
    }

    protected override void RemoveUnit(GridElement ge)
    {
        base.RemoveUnit(ge);
        if (unitsToAct.Contains((Unit)ge)) unitsToAct.Remove((Unit)ge);
        UIManager.instance.metaDisplay.UpdateEnemiesRemaining(units.Count);
    }
}
