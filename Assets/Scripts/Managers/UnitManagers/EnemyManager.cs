using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.U2D;

public class EnemyManager : UnitManager {


    [SerializeField] Unit defaultUnit;
    [SerializeField] GameObject pendingUnitGFX;
    public List<Unit> unitsToAct = new List<Unit>();
    [SerializeField] List<GridElement> pendingUnits = new List<GridElement>();
    [HideInInspector] public List<GameObject> pendingUnitUIs = new List<GameObject>();
    public delegate void OnEnemyCondition(GridElement ge);
    public event OnEnemyCondition WipedOutCallback;
    protected Coroutine actingUnitCo;

    public override IEnumerator Initialize(Grid _currentGrid)
    {
        yield return base.Initialize(_currentGrid);
    }

    public override Unit SpawnUnit(Vector2 coord, Unit unit)
    {
        Unit u = base.SpawnUnit(coord, unit);
        //u.ElementDestroyed += DescentTriggerCheck;
        u.ElementDestroyed += CountDefeatedEnemy; 
        u.ElementDestroyed += StopActingUnit;
        return u;
    }

    void CountDefeatedEnemy(GridElement ge) {
        scenario.player.defeatedEnemies++;
    }

    public virtual Unit SpawnBossUnit(Vector2 coord, Unit unit) {
        Unit u = SpawnUnit(coord, unit);
        
        units.Remove(u);
        units.Insert(0, u);

        u.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = 0;
        return u;
    }

    public virtual void DescentTriggerCheck(GridElement ge = null) {
        if (scenario.currentEnemy == this && scenario.currentTurn != ScenarioManager.Turn.Descent) {
            if (units.Count <= 0) {
                StopActingUnit();
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

        yield return StartCoroutine(DescendReinforcements());

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
            if (lose || scenario.player.nail.conditions.Contains(Unit.Status.Disabled)) 
                break;

// Yield to selected acting EnemyUnit coroutine
            if (unitsToAct[0] is EnemyUnit enemy) {
                SelectUnit(enemy);
                StartCoroutine(scatter ? enemy.ScatterTurn() : enemy.CalculateAction());
                unitActing = true;
                Debug.Log("Enemy unit acting");

                while (unitActing) {
                    yield return null;
                    if (enemy == null) unitActing = false;
                }
                    DeselectUnit();
                yield return new WaitForSecondsRealtime(0.125f);
                
                Debug.Log("Enemy unit finished acting");
                unitsToAct.Remove(enemy);
            } else {

            }
        }

        if (SpawnReinforcements()) yield return new WaitForSecondsRealtime(1.5f);
        

        EndTurn();
    }

    public void StopActingUnit(GridElement ge = null) {
        if (actingUnitCo != null) {
            StopCoroutine(actingUnitCo);
            actingUnitCo = null;
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
            if (units[i] is BossUnit)
                newGrid.enemy.units.Insert(0, units[i]);
            else
                newGrid.enemy.units.Add(units[i]);

// Update subscriptions
            newGrid.enemy.SubscribeElement(units[i]);
            units[i].manager = eManager;
            //units[i].ElementDestroyed += eManager.DescentTriggerCheck;
            //units[i].ElementDestroyed -= DescentTriggerCheck;
            units[i].ElementDestroyed -= currentGrid.RemoveElement;

            units[i].transform.parent = newGrid.enemy.transform;
            units[i].StoreInGrid(newGrid);
            units[i].UpdateElement(units[i].coord);
            units.RemoveAt(i);
        }
        //eManager.DescentTriggerCheck();
        UIManager.instance.metaDisplay.UpdateEnemiesRemaining(newGrid.enemy.units.Count);
        Destroy(this.gameObject);
    }

    public virtual bool SpawnReinforcements() {
        if (pendingUnits.Count > 0) {
            foreach (Unit u in pendingUnits) 
                units.Add(u);
        }
        bool spawn = false;

        pendingUnits = new List<GridElement>();
        if (units.Count < currentGrid.lvlDef.minEnemies) {
            int count = currentGrid.lvlDef.minEnemies - units.Count;
            for (int i = 0; i < count; i++) {
                Unit reinforcement = Reinforcement();
                if (reinforcement)
                    pendingUnits.Add(reinforcement);
            }
            spawn = true;
        }

        pendingUnitUIs = new List<GameObject>();
        foreach (Unit u in pendingUnits) {
            GameObject obj = Instantiate(pendingUnitGFX, unitParent.transform);
            obj.SetActive(true);
            
            obj.transform.localScale = Vector3.one * FloorManager.sqrSize;
            obj.transform.position = currentGrid.PosFromCoord(u.coord);
            int sort = currentGrid.SortOrderFromCoord(u.coord);
            Debug.Log(u.coord + ", " + obj.transform.position);
            
            SpriteShapeRenderer srr = obj.GetComponentInChildren<SpriteShapeRenderer>();
            LineRenderer lr = obj.GetComponentInChildren<LineRenderer>();
            srr.color = new Color(1, 0, 0, 0.25f);
            srr.sortingOrder = sort;
            lr.startColor = new Color(1, 0, 0, 0.75f); lr.endColor = new Color(1, 0, 0, 0.75f);
            lr.sortingOrder = sort;

            pendingUnitUIs.Add(obj);
        }
        
        return spawn;
    }

    public virtual IEnumerator DescendReinforcements() {
        for (int i = pendingUnitUIs.Count - 1; i >= 0; i--) {
            Destroy(pendingUnitUIs[i]);
        }
        pendingUnitUIs = new List<GameObject>();
        if (pendingUnits.Count > 0) {
            yield return StartCoroutine(floorManager.DescendUnits(pendingUnits));
        }
        transform.parent = currentGrid.transform;
    }

    public virtual Unit Reinforcement() {
        bool validCoord = false;
        Vector2 spawn = Vector2.zero;
        List<Vector2> attemptedSpawns = new List<Vector2>();
        while (!validCoord && attemptedSpawns.Count <= 35) {
            validCoord = true;
            spawn = new Vector2(Random.Range(1,6), Random.Range(1,6));

            if (attemptedSpawns.Contains(spawn)) {
                validCoord = false;
            } else {
                attemptedSpawns.Add(spawn);
                if (pendingUnits.Find(u => u.coord == spawn)) validCoord = false;
                foreach (GridElement ge in currentGrid.gridElements) {
                    if (ge.coord == spawn) validCoord = false;
                }

                if (currentGrid.tiles.Find(sqr => sqr.coord == spawn).tileType == Tile.TileType.Bile) validCoord = false; 
            }
        }
        if (!validCoord) return null;
        Unit reinforcement = SpawnUnit(spawn, defaultUnit);
        RemoveUnit(reinforcement);
        currentGrid.RemoveElement(reinforcement);
        reinforcement.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = 0f;
        return reinforcement;
    }

    public virtual void InterruptReinforcements() {
        for (int i = pendingUnitUIs.Count - 1; i >= 0; i--) {
            Destroy(pendingUnitUIs[i]);
        }
        pendingUnitUIs = new List<GameObject>();

        for (int i = pendingUnits.Count - 1; i >= 0; i--) {
            StartCoroutine(pendingUnits[i].DestroySequence());
        }
    }
    protected override void RemoveUnit(GridElement ge) {
        base.RemoveUnit(ge);
        if (unitsToAct.Contains((Unit)ge)) unitsToAct.Remove((Unit)ge);
        if (pendingUnits.Contains((Unit)ge)) pendingUnits.Remove((Unit)ge);
        UIManager.instance.metaDisplay.UpdateEnemiesRemaining(units.Count);
    }
}
