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
    [HideInInspector] public List<Vector2> nailSpawnOverrides = new List<Vector2>();
    public List<HammerData> hammerActions;
    [SerializeField] EquipmentData cascadeMovement;
     public EquipmentData overrideEquipment = null;
    [SerializeField] public GridContextuals contextuals;

    [Header("PREFABS")]
    [SerializeField] public GameObject nailPrefab;
    [SerializeField] public GameObject hammerPrefab, hammerPickupPrefab;

    [Header("UNDO")]
    public bool unitActing = false;
    public Dictionary<Unit, Vector2> undoableMoves = new Dictionary<Unit, Vector2>();
    public List<Unit> undoOrder;
    private GridElement prevCursorTarget = null;
    private bool prevCursorTargetState = false;

    [Header("MISC.")]
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

    public override IEnumerator Initialize(bool tut = false)
    {
        yield return base.Initialize();

        contextuals.Initialize(this);

        List<Vector2> spawnCoords =new List<Vector2>{
            new Vector2(3,4),
            new Vector2(4,4),
            new Vector2(3,3)
        };
        if (tut) {
            spawnCoords = new List<Vector2>{
                new Vector2(4,1),
                new Vector2(1,2),
                new Vector2(3,3)
            };
        }

        List<Unit> initU = new List<Unit>() {
            SpawnUnit(spawnCoords[0], loadout.unitPrefabs[0]),
            SpawnUnit(spawnCoords[1], loadout.unitPrefabs[1]),
            SpawnUnit(spawnCoords[2], loadout.unitPrefabs[2])
        };

        yield return StartCoroutine(loadout.Initialize(initU));
        //yield return ScenarioManager.instance.StartCoroutine(ScenarioManager.instance.SwitchTurns(ScenarioManager.Turn.Descent));

        SpawnHammer((PlayerUnit)units[0], hammerActions);
        
        nail = (Nail)SpawnUnit(new Vector3(3, 3), nailPrefab.GetComponent<Nail>());
        nail.gameObject.transform.parent = unitParent.transform;      

        pc = GetComponent<PlayerController>();
        if (FloorManager.instance) floorManager = FloorManager.instance;

// NEEDS IF STATEMENT FOR BOOL USED IN LOADOUT INITIALIZATION
        ScenarioManager.instance.InitialDescent();
// END HERE
    }

// Spawn a new instance of a hammer and update hammer actions
    public virtual void SpawnHammer(PlayerUnit unit, List<HammerData> hammerData) {
        GameObject h = Instantiate(hammerPrefab, unit.transform.position, Quaternion.identity, unit.transform);
        h.GetComponentInChildren<SpriteRenderer>().sortingOrder = unit.gfx[0].sortingOrder;
        unit.gfx.Add(h.GetComponentInChildren<SpriteRenderer>());
        h.transform.GetChild(0).transform.localPosition = new Vector3(0.5f, 0, 0);
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
    public void StartEndTurn(bool start, bool cascade = false) {
        ToggleUnitSelectability(start);
// Start Turn
        if (start) {
            StartCoroutine(pc.GridInput());
// Reset unit energy if not continued turn
            foreach(Unit u in units) {
                if (u is PlayerUnit) {
                    if (!u.conditions.Contains(Unit.Status.Disabled)) {
                        u.energyCurrent = u.energyMax;
                        if (!u.conditions.Contains(Unit.Status.Immobilized))
                            u.moved = false;
                    }
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
            if (scenario.prevTurn != ScenarioManager.Turn.Descent && scenario.currentTurn != ScenarioManager.Turn.Descent) {
                if (nail.nailState == Nail.NailState.Buried)
                    nail.ToggleNailState(Nail.NailState.Primed);
            }
        }
        if (cascade) {
            overrideEquipment = cascadeMovement;
            foreach (Unit u in units) {
                u.grid = currentGrid;
            }
            contextuals.grid = currentGrid;
            //contextuals.DisplayGridContextuals(selectedUnit, null, GridContextuals.ContextDisplay.IconOnly, 0);
        } else
            overrideEquipment = null;
    }

    public void ToggleUnitSelectability(bool state) {
        for (int i = 0; i <= units.Count - 1; i++) {
            units[i].EnableSelection(state);
        }
    }

// Overriden functionality
    public override Unit SpawnUnit(Vector2 coord, Unit unit) {
        Unit u = base.SpawnUnit(coord, unit);
        //u.transform.position += new Vector3(0, floorManager.floorOffset, 0);
        u.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = 0.5f;
        if (u is Nail)
            u.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = 0;
            
                
// Initialize equipment from prefab
        foreach(EquipmentData e in u.equipment) {
            e.EquipEquipment(u);
        }
        //u.grid.RemoveElement(u);

        return u;
    }

    public IEnumerator DropNail() {
        if (nail.nailState == Nail.NailState.Buried)
            nail.ToggleNailState(Nail.NailState.Primed);
            
        yield return null;

        bool validCoord = false;
        Vector2 spawn = Vector2.zero;
// Find a valid coord that a player is not in
        while (!validCoord) {
            validCoord = true;
            if (nailSpawnOverrides.Count > 0) 
                spawn = nailSpawnOverrides[Random.Range(0,nailSpawnOverrides.Count)];
            else
                spawn = new Vector2(Random.Range(1,6), Random.Range(1,6));
                
            foreach(Unit u in units) {
                if (u.coord == spawn) validCoord = false;
            }

            if (currentGrid.sqrs.Find(sqr => sqr.coord == spawn).tileType == GridSquare.TileType.Bile) validCoord = false;
        }
        
        nail.transform.position = currentGrid.PosFromCoord(spawn) + new Vector3(0, floorManager.floorOffset, 0);
        nail.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = 1;
        
        yield return StartCoroutine(UpdateNail(spawn));
    }

    public IEnumerator UpdateNail(Vector2 coord) {
        nail.transform.parent = unitParent.transform;

        GridElement subGE = currentGrid.gridElements.Find(ge => ge.coord == coord);
        if (subGE != null) {
            StartCoroutine(subGE.CollideFromBelow(nail));
        }
        
        yield return StartCoroutine(nail.nailDrop.MoveToCoord(nail, coord));

        if (nail.landingSFX)
            nail.PlaySound(nail.landingSFX.Get());

        nail.ToggleNailState(Nail.NailState.Buried);

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
                    if (u == selectedUnit && !selectedUnit.ValidCommand(u.coord, selectedUnit.selectedEquipment)) 
                    {  
                        DeselectUnit();                 
                    }
                    else if (selectedUnit.ValidCommand(u.coord, selectedUnit.selectedEquipment)) {
                        StartCoroutine(selectedUnit.ExecuteAction(u));
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
                    if (selectedUnit.ValidCommand(u.coord, selectedUnit.selectedEquipment)) {
                        StartCoroutine(selectedUnit.ExecuteAction(u));
                    } 
                }
            }
        }
        else if (input is GroundElement) {
            GridInput(currentGrid.sqrs.Find(sqr => sqr.coord == input.coord));
        }
// Player clicks on square
        else if (input is GridSquare sqr) 
        {
// Check if square is empty
            GridElement contents = null;
            foreach (GridElement ge in currentGrid.CoordContents(sqr.coord)) {
                if (ge is not GroundElement)
                   contents = ge;
            }
// Square not empty, recurse this function with reference to square contents
            if (contents)
                GridInput(contents);
// Square empty
            else {
                if (selectedUnit) {
// Square is a target of valid action adjacency
                    if (selectedUnit.ValidCommand(sqr.coord, selectedUnit.selectedEquipment)) {
                        currentGrid.DisableGridHighlight();
                        StartCoroutine(selectedUnit.ExecuteAction(sqr));
                    } else 
                        DeselectUnit();
                }
            }            
        }
        else {
            if (selectedUnit) {
                if (selectedUnit.ValidCommand(input.coord, selectedUnit.selectedEquipment)) {
                    currentGrid.DisableGridHighlight();
                    StartCoroutine(selectedUnit.ExecuteAction(input));
                } else
                    DeselectUnit();
            }
        }
    }

    public void GridMouseOver(Vector2 pos, bool state) {
        currentGrid.UpdateTargetCursor(state, pos);
        if (selectedUnit != null) {
            if (contextuals.displaying) {
                if (selectedUnit.validActionCoords.Count > 0) {
                    if (selectedUnit.validActionCoords.Contains(pos)) 
                        contextuals.UpdateCursor(selectedUnit, pos);
                    else
                        contextuals.ToggleValid(false);
                }
            }
        }
        bool update = false;
// if cursor is on grid
        if (prevCursorTarget) {
// player has moved the cursor to another coord
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
// first grid mouseOver if cursor not on grid
        } else {
            update = false;
            foreach(GridElement ge in currentGrid.CoordContents(pos)) {
                if (!ge.targeted && ge is not GroundElement) {
                    prevCursorTargetState = ge.targeted;
                    ge.TargetElement(true);
                    prevCursorTarget = ge;
                }
            }
        }
    }

    public override void SelectUnit(Unit u)
    {
        if (u.selectable) {
            base.SelectUnit(u);
            if (u.energyCurrent > 0) u.ui.ToggleEquipmentButtons();
            if (!u.moved && u is PlayerUnit) {
                u.selectedEquipment = u.equipment[0];
                u.UpdateAction(u.selectedEquipment, u.moveMod);
            }
            prevCursorTargetState = true;
        }
    }

    public void EquipmentSelected(EquipmentData equip = null) {
        if (equip) {
            if (!equip.multiselect || equip.firstTarget == null) {
                contextuals.StartUpdateCoroutine();
            }
            if (equip.contextualAnimGO != null) {
                contextuals.DisplayGridContextuals(selectedUnit, equip.contextualAnimGO, equip.contextDisplay, equip.gridColor);
            } else {
                contextuals.DisplayGridContextuals(selectedUnit, selectedUnit.gameObject, equip.contextDisplay, equip.gridColor);
            }
        }
    }

    public override void DeselectUnit()
    {
        base.DeselectUnit();
        turnBlink.BlinkEndTurn();
        prevCursorTargetState = false;
        contextuals.displaying = false;
    }

    public virtual IEnumerator UnitIsActing() {
        unitActing = true;
        scenario.uiManager.LockHUDButtons(true);
        while (unitActing) {
            yield return null;

        }
        scenario.uiManager.LockHUDButtons(false);
    }

    public void UndoMove() {
        if (selectedUnit)
            DeselectUnit();
        foreach (Unit u in units) 
            u.UpdateAction();

        if (undoableMoves.Count > 0 && undoOrder.Count > 0) {
            Unit lastMoved = undoOrder[undoOrder.Count - 1];

            MoveData move = (MoveData)cascadeMovement;
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

    public void TriggerDescent(bool tut = false) {
        floorManager.Descend(false, tut);
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
            if (units[i].conditions.Contains(Unit.Status.Immobilized))
                units[i].RemoveCondition(Unit.Status.Immobilized);
        }
        currentGrid = newGrid;
        contextuals.grid = newGrid;
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
