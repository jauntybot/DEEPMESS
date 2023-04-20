using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Class that controls the player's functionality, recieves input from PlayerController and ScenarioManager
// tbh I'm not commenting this one bc it's the most heavily edited and refactors are incoming

[RequireComponent(typeof(PlayerController))]
public class PlayerManager : UnitManager {
    

    [HideInInspector] public PlayerController pc;


    [Header("PLAYER MANAGER")]
    public LoadoutManager loadout;
    public Nail nail;
    public List<HammerData> hammerActions;
    [SerializeField] public GameObject nailPrefab, hammerPrefab, hammerPickupPrefab;


    public Dictionary<Unit, Vector2> undoableMoves = new Dictionary<Unit, Vector2>();
    public List<Unit> undoOrder;
    private GridElement prevCursorTarget = null;
    private bool prevCursorTargetState = false;

// Get rid of this reference somehow
    [SerializeField] EndTurnBlinking turnBlink;

    #region Singleton (and Awake)
    public static PlayerManager instance;
    private void Awake() {
        if (PlayerManager.instance) {
            Debug.Log("Warning! More than one instance of PlayerManager found!");
            return;
        }
        PlayerManager.instance = this;
    }
    #endregion

    public override IEnumerator Initialize()
    {
        yield return base.Initialize();

        List<Unit> initU = new List<Unit>() {
            SpawnUnit(new Vector2(3,4), loadout.unitPrefabs[0]),
            SpawnUnit(new Vector2(4,4), loadout.unitPrefabs[1]),
            SpawnUnit(new Vector2(3,3), loadout.unitPrefabs[2])
        };
        yield return StartCoroutine(loadout.Initialize(initU));

        SpawnHammer((PlayerUnit)units[0], hammerActions);
        foreach (Unit u in initU) {
            StartCoroutine(floorManager.DropUnit(u, u.transform.position, currentGrid.PosFromCoord(u.coord)));
        }
        
        nail = (Nail)SpawnUnit(new Vector3(3, 3), nailPrefab.GetComponent<Nail>());
        nail.gameObject.transform.parent = unitParent.transform;
        yield return StartCoroutine(DropNail());


        pc = GetComponent<PlayerController>();
        if (FloorManager.instance) floorManager = FloorManager.instance;
    }

// Spawn a new instance of a hammer and update hammer actions
    public virtual void SpawnHammer(PlayerUnit unit, List<HammerData> hammerData) {
        GameObject h = Instantiate(hammerPrefab, unit.transform.position, Quaternion.identity, unit.transform);
        h.GetComponentInChildren<SpriteRenderer>().sortingOrder = unit.gfx[0].sortingOrder;
        unit.gfx.Add(h.GetComponentInChildren<SpriteRenderer>());
        foreach(HammerData equip in hammerData) {
            unit.equipment.Insert(unit.equipment.Count, equip);
            equip.EquipEquipment(unit);
            equip.AssignHammer(h, nail);
        }        
        unit.ui.UpdateEquipmentButtons();
        unit.SwitchAnim(PlayerUnit.AnimState.Hammer);
        h.SetActive(false);
    }

// Initializes or closes functions for turn start/end
    public void StartEndTurn(bool start) {
        for (int i = 0; i <= units.Count - 1; i++) {
            units[i].EnableSelection(start);
        }
        
// Start Turn
        if (start) {
            StartCoroutine(pc.GridInput());
// Reset unit energy if not continued turn
            foreach(Unit u in units) {
                if (u is PlayerUnit) {
                    u.energyCurrent = u.energyMax;
                    u.moved = false;
                    u.elementCanvas.UpdateStatsDisplay();
                }
            }
            undoableMoves = new Dictionary<Unit, Vector2>();
            undoOrder = new List<Unit>();
            //ResolveConditions();
// End Turn
        } else {
            DeselectUnit();
            currentGrid.UpdateTargetCursor(false, Vector2.one * -32);
        }
    }

// Overriden functionality
    public override Unit SpawnUnit(Vector2 coord, Unit unit) {
        Unit u = base.SpawnUnit(coord, unit);
        u.transform.position += new Vector3(0, floorManager.floorOffset, 0);
        u.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = 0;
        
// Initialize equipment from prefab
        foreach(EquipmentData e in u.equipment) {
            e.EquipEquipment(u);
        }

        return u;
    }

    public IEnumerator DropNail() {
        yield return null;
        bool validCoord = false;
        Vector2 spawn = Vector2.zero;
// Drop on an enemy
        if (Random.Range(0,100) <= nail.collisionChance && scenario.currentEnemy.units.Count > 0) {
            spawn = scenario.currentEnemy.units[Random.Range(0,scenario.currentEnemy.units.Count - 1)].coord;    
// Drop on a neutral tile
        } else {
// Find a valid coord that a player is not in
            while (!validCoord) {
                validCoord = true;
                spawn = new Vector2(Random.Range(1,6), Random.Range(1,6));
                foreach(Unit u in units) {
                    if (u.coord == spawn) validCoord = false;
                }
                foreach(Unit u in scenario.currentEnemy.units) {
                    if (u.coord == spawn) validCoord = false;
                }
                if (currentGrid.sqrs.Find(sqr => sqr.coord == spawn).tileType != GridSquare.TileType.Bone) validCoord = false;
            }
        }

        nail.transform.position = currentGrid.PosFromCoord(spawn) + new Vector3(0, floorManager.floorOffset, 0);
        nail.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = 1;
        yield return StartCoroutine(UpdateNail(spawn));
        nail.collisionChance = 90;
    }

    public IEnumerator UpdateNail(Vector2 coord) {
        nail.transform.parent = unitParent.transform;
        GridElement subGE = currentGrid.gridElements.Find(ge => ge.coord == coord);
        
        if (subGE != null) {
            StartCoroutine(subGE.CollideFromBelow(nail));

        }
        yield return StartCoroutine(nail.nailDrop.MoveToCoord(nail, coord));
        
        if (!currentGrid.gridElements.Contains(nail))
            nail.StoreInGrid(currentGrid);

    }

// Get grid input from player controller, translate it to functionality
    public void GridInput(GridElement input) {
// Player clicks on unit
        if (input is Unit u) 
        {
// Player clicks on their own unit
            if (u.manager is PlayerManager) 
            {
                if (selectedUnit) {
                    if (u == selectedUnit && !selectedUnit.ValidCommand(u.coord)) 
                    {  
                        DeselectUnit();                 
                    }
                    else if (selectedUnit.ValidCommand(u.coord)) {
                        selectedUnit.ExecuteAction(u);
                    } else {
                        DeselectUnit();
                        SelectUnit(u);
                    }
                } else
                    SelectUnit(u);
            }
// Player clicks on enemy unit
            else if (u.manager is EnemyManager) 
            {
                if (selectedUnit) 
                {
// Unit is a target of valid action adjacency
                    if (selectedUnit.ValidCommand(u.coord)) {
                        selectedUnit.ExecuteAction(u);
                    } 
                }
            }
        }
// Player clicks on square
        else if (input is GridSquare sqr) 
        {
// Check if square is empty
            GridElement contents = null;
            foreach (GridElement ge in currentGrid.CoordContents(sqr.coord)) contents = ge;
// Square not empty, recurse this function with reference to square contents
            if (contents)
                GridInput(contents);
// Square empty
            else {
                if (selectedUnit) {
// Square is a target of valid action adjacency
                    if (selectedUnit.ValidCommand(sqr.coord)) {
                        currentGrid.DisableGridHighlight();
                        selectedUnit.ExecuteAction(sqr);
                    } else 
                        DeselectUnit();
                }
            }            
        }
        else {
            if (selectedUnit) {
                if (selectedUnit.ValidCommand(input.coord)) {
                    currentGrid.DisableGridHighlight();
                    selectedUnit.ExecuteAction(input);
                } else
                    DeselectUnit();
            }
        }
    }

    public void GridMouseOver(Vector2 pos, bool state) {
        currentGrid.UpdateTargetCursor(state, pos);
        bool update = false;
        if (prevCursorTarget) {
            if (pos != prevCursorTarget.coord) {
                prevCursorTarget.TargetElement(prevCursorTargetState);
                update = false;
                foreach (GridElement ge in currentGrid.CoordContents(pos)) {
                    if (!ge.targeted && ge is not GroundElement) {
                        prevCursorTargetState = ge.targeted;
                        ge.TargetElement(true);
                        prevCursorTarget = ge;
                        update = true;
                    }
                }
                if (!update) {
                    prevCursorTarget = null;
                    prevCursorTargetState = false;
                }
            }
        } else {
            update = false;
            foreach(GridElement ge in currentGrid.CoordContents(pos)) {
                if (!ge.targeted && ge is not GroundElement) {
                    prevCursorTargetState = ge.targeted;
                    ge.TargetElement(true);
                    prevCursorTarget = ge;
                    update = true;
                }
            }
        }
    }

    public override void SelectUnit(Unit u)
    {
        base.SelectUnit(u);
        if (u.energyCurrent > 0) u.ui.ToggleEquipmentButtons();
        if (!u.moved && u is PlayerUnit) {
            u.selectedEquipment = u.equipment[0];
            u.UpdateAction(u.selectedEquipment, u.moveMod);
        }
        prevCursorTargetState = true;
    }

    public override void DeselectUnit()
    {
        base.DeselectUnit();
        turnBlink.BlinkEndTurn();
        prevCursorTargetState = false;
    }

    protected override void RemoveUnit(GridElement ge)
    {
        base.RemoveUnit(ge);
        float pu = 0;
        foreach (Unit unit in units) {
            if (unit is PlayerUnit) pu++;
        }
        if (pu <= 0) {
            StartCoroutine(scenario.Lose());            
        }
    }

    public void UndoMove() {
        if (undoableMoves.Count > 0 && undoOrder.Count > 0) {
            Unit lastMoved = undoOrder[undoOrder.Count - 1];

            MoveData move = (MoveData)lastMoved.equipment[0];
            StartCoroutine(move.MoveToCoord(lastMoved, undoableMoves[lastMoved], true));
            lastMoved.moved = false;
            lastMoved.elementCanvas.UpdateStatsDisplay();

            undoOrder.Remove(lastMoved);
            undoableMoves.Remove(lastMoved);
            UIManager.instance.ToggleUndoButton(undoOrder.Count > 0);

        } else if ((undoableMoves.Count > 0 && undoOrder.Count == 0) ||(undoableMoves.Count == 0 && undoOrder.Count > 0)) {
            undoOrder = new List<Unit>();
            undoableMoves = new Dictionary<Unit, Vector2>();
        }
    }

    public void TriggerDescent() {
        floorManager.Descend();
    }

    public virtual void DescendGrids(Grid newGrid) {
        undoableMoves = new Dictionary<Unit, Vector2>();
        undoOrder = new List<Unit>();
        UIManager.instance.ToggleUndoButton(undoOrder.Count > 0);

        for (int i = units.Count - 1; i >= 0; i--) {
            currentGrid.RemoveElement(units[i]);
            units[i].StoreInGrid(newGrid);
            if (units[i] is not Nail)
                units[i].UpdateElement(units[i].coord);
        }
        currentGrid = newGrid;
        transform.parent = newGrid.transform;
        transform.SetSiblingIndex(3);
    }

    Dictionary<Unit, bool> prevTargetStates;
    public void DisplayAllHP(bool active) {
        if (active) prevTargetStates = new Dictionary<Unit, bool>();
        foreach (Unit u in units) {
            if (active) {
                prevTargetStates.Add(u, u.targeted);
                u.TargetElement(true);
            } else {
                u.targeted = prevTargetStates[u];
            }
        }
        foreach(Unit u in scenario.currentEnemy.units) {
            if (active) {
                prevTargetStates.Add(u, u.targeted);
                u.TargetElement(true);
            } else {
                u.targeted = prevTargetStates[u];
            }
        }
    }
}
