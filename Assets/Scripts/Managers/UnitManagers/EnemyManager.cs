using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class EnemyManager : UnitManager {

    [SerializeField] GameObject reinforcementPrefab;
    [SerializeField] Unit defaultUnit;
    public List<Unit> unitsToAct = new();
    [SerializeField] List<GridElement> pendingUnits = new();
    public List<GameObject> pendingPreviews = new();
    protected Coroutine actingUnitCo;

    [Header ("RELIC REFS - REMOVE")]
    [SerializeField] Unit tacklePrefab;

    public override IEnumerator Initialize(Grid _currentGrid) {
        yield return base.Initialize(_currentGrid);
        for (int i = 0; i <= units.Count - 1; i++)
            units[i].elementCanvas.UpdateTurnOrder(units.Count - i);
    }

    public override Unit SpawnUnit(Unit unit, Vector2 coord) {
        Unit u = base.SpawnUnit(unit, coord);
        //u.ElementDestroyed += DescentTriggerCheck;
        u.ElementDestroyed += CountDefeatedEnemy; 
        u.ElementDestroyed += StopActingUnit;
        return u;
    }

    public virtual Unit SpawnUnit(Vector2 coord, bool tackle) {
        Unit u = Instantiate(tackle ? tacklePrefab.gameObject : defaultUnit.gameObject, unitParent.transform).GetComponent<Unit>();
        if (tackle)
            scenario.player.SubscribeElement(u);
        else
            SubscribeElement(u);
        
        u.manager = tackle ? scenario.player : this;
        UIManager.instance.UpdatePortrait(u, false);

        u.StoreInGrid(currentGrid);
        u.UpdateElement(coord);
        u.grid.RemoveElement(u);

        
        u.ElementDestroyed += CountDefeatedEnemy; 
        u.ElementDestroyed += StopActingUnit;
        
        return u;
    }

    void CountDefeatedEnemy(GridElement ge) {
        Debug.Log("Enemy defeated");
        scenario.player.defeatedEnemies++;
        for (int i = 0; i <= units.Count - 1; i++)
            units[i].elementCanvas.UpdateTurnOrder(units.Count - i);
    }

    public virtual Unit SpawnBossUnit(Vector2 coord, Unit unit) {
        Unit u = SpawnUnit(unit, coord);
        
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
            u.selectable = true;
            if (!u.conditions.Contains(Unit.Status.Immobilized))
                u.moved = false;
            u.elementCanvas.UpdateStatsDisplay();
        }
        ResolveConditions();

        unitsToAct = new List<Unit>();
        for (int i = units.Count - 1; i >= 0; i--) {
            unitsToAct.Add(units[i]);
            units[i].ElementDestroyed += RemoveUnitToAct;
        }

        yield return StartCoroutine(DescendReinforcements());

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
            if (unitsToAct[0] is EnemyUnit enemy && !(unitsToAct[0] is EnemyStaticUnit && scatter)) {
                SelectUnit(enemy);
                StartCoroutine(scatter ? enemy.ScatterTurn() : enemy.CalculateAction());
                unitActing = true;

                while (unitActing) {
                    yield return null;
                    if (enemy == null) unitActing = false;
                }
                DeselectUnit();
                yield return new WaitForSecondsRealtime(0.125f);
                
                unitsToAct.Remove(enemy);
            } else unitsToAct.RemoveAt(0);
        }

        
        Unit lastUnit = null;
        if (units.Count > 0) lastUnit = units[0];
        if (!lastUnit || lastUnit is not BossUnit || (lastUnit is BossUnit && lastUnit.energyCurrent != 0)) {
            if (AddToReinforcements()) {
                yield return StartCoroutine(SpawnReinforcements());
                yield return new WaitForSecondsRealtime(1.5f);
            }
            EndTurn();
        }
    }

    void RemoveUnitToAct(GridElement element) {
        if (element is Unit unit && unitsToAct.Contains(unit))
            unitsToAct.Remove(unit);
    }

    public void StopActingUnit(GridElement ge = null) {
        if (actingUnitCo != null) {
            StopCoroutine(actingUnitCo);
            actingUnitCo = null;
        }
    }


    public void EndTurn() {
        EndTurnEvent evt = ObjectiveEvents.EndTurnEvent;
        evt.toTurn = ScenarioManager.Turn.Player;
        ObjectiveEventManager.Broadcast(evt);

        if (selectedUnit)
           DeselectUnit();
        foreach (Unit unit in units) {
            unit.UpdateAction();
            unit.TargetElement(false);
        }

        StartCoroutine(scenario.SwitchTurns());
    }

    public virtual void SeedUnits(Grid newGrid, bool toSelf) {
        EnemyManager eManager = (EnemyManager) newGrid.enemy;
        int insertIndex = newGrid.enemy.units.Count;
        for (int i = units.Count - 1; i >= 0; i--) {
            if (units[i] is BossUnit)
                newGrid.enemy.units.Insert(0, units[i]);
            else
                newGrid.enemy.units.Insert(insertIndex, units[i]);

// Update subscriptions
            newGrid.enemy.SubscribeElement(units[i]);
            units[i].manager = eManager;
    
            units[i].ElementDestroyed -= CountDefeatedEnemy;
            units[i].ElementDestroyed -= StopActingUnit;
            units[i].ElementDestroyed += eManager.CountDefeatedEnemy; 
            units[i].ElementDestroyed += eManager.StopActingUnit;

            units[i].transform.parent = newGrid.enemy.unitParent.transform;
            units[i].StoreInGrid(newGrid);
            units[i].UpdateElement(units[i].coord);
            units.RemoveAt(i);
        }

        for (int i = 0; i <= newGrid.enemy.units.Count - 1; i++)
            newGrid.enemy.units[i].elementCanvas.UpdateTurnOrder(newGrid.enemy.units.Count - i);

        //eManager.DescentTriggerCheck();
        UIManager.instance.metaDisplay.UpdateEnemiesRemaining(newGrid.enemy.units.Count);
        
        if (!toSelf)
           Destroy(this.gameObject);
    }

    public virtual bool AddToReinforcements() {
        bool spawn = false;

        if (units.Count < currentGrid.lvlDef.minEnemies) {
            int count = currentGrid.lvlDef.minEnemies - units.Count;
            for (int i = 0; i < count; i++) {
                Unit reinforcement = Reinforcement(scenario.tackleChance);
                if (reinforcement) {
                    pendingUnits.Add(reinforcement);
                    spawn = true;
                }
            }
        }
      
        return spawn;
    }

    IEnumerator SpawnReinforcements() {
        if (scenario.floorManager.tutorial.isActiveAndEnabled && !scenario.floorManager.tutorial.enemySpawnEncountered) {
            yield return scenario.floorManager.tutorial.StartCoroutine(scenario.floorManager.tutorial.EnemySpawn());
        }
        yield return null;
        pendingPreviews = new List<GameObject>();
        foreach (Unit u in pendingUnits) {
            GameObject obj = Instantiate(reinforcementPrefab, unitParent.transform);
            pendingPreviews.Add(obj);
            int sort = currentGrid.SortOrderFromCoord(u.coord);
            obj.transform.position = currentGrid.PosFromCoord(u.coord);
            LineRenderer lr = obj.GetComponentInChildren<LineRenderer>();
            lr.sortingOrder = sort;
            lr.startColor = new Color(1, 0, 0, 0.75f); lr.endColor = new Color(1, 0, 0, 0.75f);
            SpriteRenderer sr = obj.GetComponentInChildren<SpriteRenderer>();
            sr.sortingOrder = sort;


            u.PlaySound(u.selectedSFX);
            float t = 0;
            while (t < 0.25f) { t += Time.deltaTime; yield return null; }
            
            
            
            // obj.transform.localScale = Vector3.one * FloorManager.sqrSize;
            
            // SpriteShapeRenderer srr = obj.GetComponentInChildren<SpriteShapeRenderer>();
            // srr.color = new Color(1, 0, 0, 0.25f);
            // srr.sortingOrder = sort;

            // pendingUnitUIs.Add(obj);
        }
    }

    public virtual IEnumerator DescendReinforcements() {
        foreach (Unit u in pendingUnits) {
            if (u is EnemyUnit)
                units.Add(u);
            else
                scenario.player.units.Add(u);
            DescentPreview dp = Instantiate(unitDescentPreview, floorManager.previewManager.transform).GetComponent<DescentPreview>();
            dp.Initialize(u, floorManager.previewManager);
        }
        for (int i = pendingPreviews.Count - 1; i >= 0; i--) {
            Destroy(pendingPreviews[i].gameObject);
        }
        pendingPreviews = new();
        if (pendingUnits.Count > 0) {
            yield return StartCoroutine(floorManager.DescendUnits(pendingUnits, this));
            
        }
        pendingUnits = new List<GridElement>();

        transform.parent = currentGrid.transform;
    }

    public virtual Unit Reinforcement(int tackleChance = 0) {
        bool validCoord = false;
        Vector2 spawn = Vector2.zero;
        
        ShuffleBag<Vector2> spawns = new ShuffleBag<Vector2>();
        for (int x = 1; x <= 6; x++) {
            for (int y = 1; y <= 6; y++) {
                if (currentGrid.CoordContents(new Vector2(x,y)).Count == 0) {
                    spawns.Add(new Vector2(x,y));
                }    
            }
        }

        while (!validCoord && spawns.Count > 0) {
            validCoord = true;          
            
            spawn = spawns.Next();
            Debug.Log(spawn);
            
            if (pendingUnits.Find(u => u.coord == spawn)) validCoord = false;
            if (currentGrid.tiles.Find(sqr => sqr.coord == spawn).tileType == Tile.TileType.Bile) validCoord = false; 
            
        }

        if (!validCoord) return null;
// Tacklebox Relic
        bool tackle = false;
        if (tackleChance > 0) {
            int rnd = Random.Range(0, 101);
            if (rnd <= tackleChance) tackle = true;
        }
        Unit reinforcement = SpawnUnit(spawn, tackle);
        reinforcement.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = 0f;

        return reinforcement;
    }

    public virtual void InterruptReinforcements() {
        for (int i = pendingPreviews.Count - 1; i >= 0; i--) {
            Destroy(pendingPreviews[i].gameObject);
        }
        pendingPreviews = new();

        for (int i = pendingUnits.Count - 1; i >= 0; i--) {
            Destroy(pendingUnits[i].gameObject);
        }
    }
    protected override void RemoveUnit(GridElement ge) {
        base.RemoveUnit(ge);
        if (unitsToAct.Contains((Unit)ge)) unitsToAct.Remove((Unit)ge);
        if (pendingUnits.Contains((Unit)ge)) pendingUnits.Remove((Unit)ge);
        UIManager.instance.metaDisplay.UpdateEnemiesRemaining(units.Count);
    }
}
