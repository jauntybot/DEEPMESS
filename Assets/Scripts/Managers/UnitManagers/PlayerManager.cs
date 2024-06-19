using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Class that controls the player's functionality, recieves input from PlayerController and ScenarioManager
// tbh I'm not commenting this one bc it's the most heavily edited and refactors are incoming

[RequireComponent(typeof(PlayerController))]
public class PlayerManager : UnitManager {
    

    [HideInInspector] public PlayerController pc;


    [Header("PLAYER MANAGER")]
    [SerializeField] List<Unit> unitPrefabs;
    public UpgradeManager upgradeManager;
    public Nail nail;
    public List<HammerData> hammerActions;
    [SerializeField] List<BulbEquipmentData> bulbData;
    [SerializeField] GearData cascadeMovement;
     public GearData overrideEquipment = null;
    [SerializeField] public GridContextuals contextuals;
    [HideInInspector] public Vector2 lastHoveredCoord;
    public int defeatedEnemies;

    public delegate void OnPlayerAction(PlayerManager player);
    public virtual event OnPlayerAction UndoClearCallback;

    public int collectedNuggets;

    [Header("PREFABS")]
    [SerializeField] public GameObject nailPrefab;
    [SerializeField] public GameObject hammerPrefab;
    [SerializeField] GameObject slimeArmAnim;

    [Header("UNDO")]
    public Dictionary<Unit, Vector2> undoableMoves = new();
    public Dictionary<Unit, GridElement> harvestedByMove = new();
    public List<Unit> undoOrder;

    [Header("GRID VIS")]
    [SerializeField] GameObject gridCursor;
    [SerializeField] Unit hoveredUnit = null;
    [SerializeField] GridElement prevCursorTarget = null;
    [SerializeField] bool prevCursorTargetState = false;
    PlayerController.CursorState targetCursorState;

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
    
    public virtual IEnumerator Initialize(RunData run = null) {
        if (ScenarioManager.instance) scenario = ScenarioManager.instance;
        if (FloorManager.instance) floorManager = FloorManager.instance;

        contextuals.Initialize(this);
        lastHoveredCoord = new Vector2(0,0);
        gridCursor.transform.localScale = Vector3.one * FloorManager.sqrSize;

        List<Unit> initU = new() {
            SpawnUnit(unitPrefabs[0]),
            SpawnUnit(unitPrefabs[1]),
            SpawnUnit(unitPrefabs[2])
        };

        yield return null;
        
        nail = (Nail)SpawnUnit(nailPrefab.GetComponent<Nail>());
        nail.gameObject.transform.parent = unitParent.transform;      

        pc = GetComponent<PlayerController>();
        pc.Init();

        reviveTo = 1;
        collectedNuggets = 0;

        if (run != null) LoadRunState(run);
        else SpawnHammer((PlayerUnit)units[0], hammerActions);
        
        upgradeManager.Init(initU, run);
    }

    void LoadRunState(RunData runData) {
        foreach (Unit u in units) {
            if (runData.hammerUnit == u.name) SpawnHammer((PlayerUnit)u, hammerActions);
            if (runData.bulbs.ContainsKey(u.name) && bulbData.Find(d => d.name == runData.bulbs[u.name])) {
                u.ui.UpdateLoadout(bulbData.Find(d => d.name == runData.bulbs[u.name]));
            }
            if (runData.unitHP.ContainsKey(u.name)) {
                u.hpCurrent = runData.unitHP[u.name][0];
                u.hpMax = runData.unitHP[u.name][1];
                u.elementCanvas.InstantiateMaxPips();
                if (u.hpCurrent == 0) u.StartCoroutine(u.DestroySequence());
            }
            if (u is PlayerUnit && runData.unitUpgrades.ContainsKey(u.equipment[1].name)) {
                SlagGearData gear = (SlagGearData)u.equipment[1];
                int slotIndex = 0;
                foreach(String upgrade in runData.unitUpgrades[gear.name]) {
                    if (upgrade != "Empty") {
                        gear.UpgradeGear(gear.upgrades.Find(up => up.name == upgrade), slotIndex);
                    }
                    slotIndex++;
                }
            }
        }
        collectedNuggets = runData.slimeBux;
    }

// Overriden functionality
    public override Unit SpawnUnit(Unit unit, Vector2 coord) {
        Unit u = Instantiate(unit.gameObject, unitParent.transform).GetComponent<Unit>();

        units.Add(u);
        SubscribeElement(u);
        u.manager = this;
        if (u is PlayerUnit pu)
            pu.pManager = this;

        if (unit is not Nail) {
            DescentPreview dp = Instantiate(unitDescentPreview, floorManager.previewManager.transform).GetComponent<DescentPreview>();
            dp.Initialize(u, floorManager.previewManager);
            
        }

        u.Init(currentGrid, coord);
        
        u.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = 0;

        return u;
    }

    public override Unit SpawnUnit(Unit unit) {
        Unit u = Instantiate(unit.gameObject, unitParent.transform).GetComponent<Unit>();

        units.Add(u);
        SubscribeElement(u);
        u.manager = this;
        if (u is PlayerUnit pu)
            pu.pManager = this;

        if (unit is not Nail) {
            DescentPreview dp = Instantiate(unitDescentPreview, floorManager.previewManager.transform).GetComponent<DescentPreview>();
            dp.Initialize(u, floorManager.previewManager);
        }

        u.Init();

        u.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = 0;

        return u;
    }

// Spawn a new instance of a hammer and update hammer actions
    public virtual void SpawnHammer(PlayerUnit unit, List<HammerData> hammerData) {
        GameObject h = Instantiate(hammerPrefab, unit.transform.position, Quaternion.identity, unit.transform);
        h.GetComponentInChildren<SpriteRenderer>().sortingOrder = unit.gfx[0].sortingOrder;
        unit.gfx.Add(h.GetComponentInChildren<SpriteRenderer>());
        h.transform.GetChild(0).transform.localPosition = new Vector3(0.5f, 0, 0);
        foreach(HammerData equip in hammerData) {
            unit.equipment.Insert(unit.equipment.Count, equip);
            equip.EquipGear(unit, true);
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
// Reset unit energy if not continued turn
            foreach(Unit u in units) {
                if (u is PlayerUnit) {
                    if (!u.conditions.Contains(Unit.Status.Disabled) && !u.conditions.Contains(Unit.Status.Stunned)) {
                        u.energyCurrent = u.energyMax;
                        if (!u.conditions.Contains(Unit.Status.Immobilized))
                            u.moved = false;
                    } else {
                        u.energyCurrent = 0;
                        u.moved = true;
                    }
                    u.elementCanvas.UpdateStatsDisplay();
                }
                u.TargetElement(false);
            }
            undoableMoves = new Dictionary<Unit, Vector2>();
            undoOrder = new List<Unit>();
            harvestedByMove = new Dictionary<Unit, GridElement>();
            UndoClearCallback?.Invoke(this);
// End Turn
        } else {
            DeselectUnit();
            if (scenario.prevTurn != ScenarioManager.Turn.Descent && scenario.prevTurn != ScenarioManager.Turn.Cascade) {
                ResolveConditions();
            }
            contextuals.UpdateGridCursor(false);
            if (scenario.prevTurn != ScenarioManager.Turn.Descent && !floorManager.currentFloor.gridElements.Find(ge => ge is BossUnit)) {
                if (nail.nailState == Nail.NailState.Buried)
                    nail.ToggleNailState(Nail.NailState.Primed);
                else if (scenario.currentTurn is ScenarioManager.Turn.Enemy)
                    nail.barkBox.Bark(BarkBox.BarkType.Delay);
            }
        }
        if (cascade) {
            overrideEquipment = cascadeMovement;
            foreach (Unit u in units) {
                u.grid = currentGrid;
            }
            //contextuals.DisplayGridContextuals(selectedUnit, null, GridContextuals.ContextDisplay.IconOnly, 0);
        } else
            overrideEquipment = null;
    }

    public void ToggleUnitSelectability(bool state) {
        for (int i = 0; i <= units.Count - 1; i++) {
            units[i].EnableSelection(state);
        }
    }

#region Player Controller interface
// Get grid input from player controller, translate it to functionality
    public void GridInput(GridElement input) {
        if (input is Unit u) {
// Player clicks on unit
// Player clicks on their own unit
            if (u.manager is PlayerManager) {
                if (selectedUnit) {
                    if (u == selectedUnit && !selectedUnit.ValidCommand(u.coord, selectedUnit.selectedEquipment)) {  
                        //DeselectUnit();                 
                        return;
                    }
                    else if (selectedUnit.ValidCommand(u.coord, selectedUnit.selectedEquipment)) {
                        StartCoroutine(selectedUnit.ExecuteAction(u));
                    } else if (u is Nail && selectedUnit.selectedEquipment is HammerData) {
                        nail.barkBox.Bark(BarkBox.BarkType.NotPrimed);
                    } 
                    else {
                        DeselectUnit();
                        SelectUnit(u);
                    }
                } else {
                    SelectUnit(u);
                }
            }
// Player clicks on enemy unit
            else if (u.manager is EnemyManager) {
                if (overrideEquipment && selectedUnit == null) {
                    foreach(Unit _u in units) {
                        if (u.coord == _u.coord) {
                            SelectUnit(_u);
                        }
                    }
                } else if (selectedUnit && selectedUnit is PlayerUnit) {
// Unit is a target of valid action adjacency
                    if (selectedUnit.ValidCommand(u.coord, selectedUnit.selectedEquipment)) {
                        StartCoroutine(selectedUnit.ExecuteAction(u));
                    } else if (selectedUnit.selectedEquipment == null || selectedUnit.selectedEquipment is MoveData)
                        SelectUnit(u); 
                } else if (!overrideEquipment)
                    SelectUnit(u);
                
            }
        } else if (input is Beacon b && b.selectable) {
            if (selectedUnit && selectedUnit.ValidCommand(b.coord, selectedUnit.selectedEquipment)) 
                StartCoroutine(selectedUnit.ExecuteAction(b));
            else {
                if (selectedUnit) DeselectUnit();
                //scenario.SwitchTurns()
                SelectUnit(b);
            }
        } 
// Player clicks on square
        else if (input is Tile tile) {
// Check if square is empty
            GridElement contents = null;
            foreach (GridElement ge in currentGrid.CoordContents(tile.coord)) {
                contents = ge;
                if (ge is PlayerUnit && overrideEquipment) break;
            }
            if (overrideEquipment && !selectedUnit) {
                foreach(Unit _u in units) {
                    if (tile.coord == _u.coord) {
                        SelectUnit(_u);
                    }
                }
            }
// Square not empty, recurse this function with reference to square contents
            if (contents)
                GridInput(contents);
// Square empty
            else {
                if (overrideEquipment && !selectedUnit) {
                    foreach(Unit _u in units) {
                        if (tile.coord == _u.coord) {
                            SelectUnit(_u);
                        }
                    }
                } else if (selectedUnit && selectedUnit is PlayerUnit) {
// Square is a target of valid action adjacency
                    if (selectedUnit.ValidCommand(tile.coord, selectedUnit.selectedEquipment)) {
                        currentGrid.DisableGridHighlight();
                        StartCoroutine(selectedUnit.ExecuteAction(tile));
                    }
                }
            }            
        }
        else {
            if (selectedUnit && selectedUnit is PlayerUnit) {
                if (selectedUnit.ValidCommand(input.coord, selectedUnit.selectedEquipment)) {
                    currentGrid.DisableGridHighlight();
                    StartCoroutine(selectedUnit.ExecuteAction(input));
                } 
            } else {
                List<GridElement> list = currentGrid.CoordContents(input.coord);
                if (list.Count > 1) {
                    foreach(GridElement ge in list)
                        if (ge is PlayerUnit && overrideEquipment) GridInput(ge);
                }
            }
        }
    }


    public void GridMouseOver(Vector2 pos, bool state) {
        // if (pos == new Vector2(-32,-32)) pos = lastHoveredCoord;
        // else lastHoveredCoord = pos;

        contextuals.UpdateGridCursor(state, pos, selectedUnit && selectedUnit.selectedEquipment);
        if (Mathf.Sign(pos.x) >= 0) pc.UpdateCursor(targetCursorState);
        else pc.UpdateCursor(PlayerController.CursorState.Default);
// Unit is selected - Grid contextuals
        if (selectedUnit != null) {
            if (contextuals.displaying) {
                if (selectedUnit.inRangeCoords.Count > 0) {
                    if (selectedUnit.inRangeCoords.Contains(pos)) {
                        contextuals.UpdateCursor(selectedUnit, pos);
                        if (selectedUnit.validActionCoords.Contains(pos)) {
                            contextuals.ChangeLineColor(selectedUnit.selectedEquipment.gridColor);
                            pc.ToggleCursorValid(true);
                        }
                        else {
                            contextuals.ChangeLineColor(3); // 3 is invalid color index
                            pc.ToggleCursorValid(false);
                            contextuals.UpdateGridCursor(state, pos, true, false);
                        }
                    } else {
                        contextuals.ToggleValid(false);
                        pc.ToggleCursorValid(false);
                        contextuals.UpdateGridCursor(state, pos, true, false);
                    }
                }
            }
// No unit selected
        } else if (scenario.currentTurn == ScenarioManager.Turn.Player || scenario.currentTurn == ScenarioManager.Turn.Cascade) {
            bool hovering = false;
            foreach (GridElement ge in floorManager.currentFloor.CoordContents(pos)) {
                if (ge is Unit u) {
                    hovering = true;
                    if (u.selectable)
                        pc.ToggleCursorValid(true);
                    if (u == prevCursorTarget) break;
                    
                    hoveredUnit = u;
                    if (!prevCursorTarget) prevCursorTarget = u;

                    ge.TargetElement(true);
                    UIManager.instance.UpdatePortrait(u, true);
                    if ((u is PlayerUnit || u is EnemyUnit) && FloorManager.instance.currentFloor == currentGrid) {
                        if (u is PlayerUnit pu && (!pu.selectable || pu.moved)) break;
                        if (u.selectedEquipment != u.equipment[0])
                            u.selectedEquipment = u.equipment[0];
                        u.grid.DisableGridHighlight();
                        u.UpdateAction(u.selectedEquipment, u.moveMod);
                        u.grid.DisplayValidCoords(u.validActionCoords, u is EnemyUnit ? 4 : 3, false, false);
                    }
                }
            }
            if (!hovering) {
                UIManager.instance.UpdatePortrait();
                if (hoveredUnit) {
                    selectedUnit = hoveredUnit;
                    DeselectUnit();
                    hoveredUnit = null;
                }
                
                pc.ToggleCursorValid(false);
            }
        }

        bool update = false;
        
// Resolve target state of previously hovered unit
        if (prevCursorTarget) {
            if (pos != prevCursorTarget.coord) {
                prevCursorTarget.TargetElement(prevCursorTargetState);
                foreach (GridElement ge in floorManager.currentFloor.CoordContents(pos)) {
                    if (!ge.targeted) {
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
        } else if (!hoveredUnit) {
            foreach(GridElement ge in floorManager.currentFloor.CoordContents(pos)) {
                if (!ge.targeted) {
                    prevCursorTargetState = ge.targeted;
                    ge.TargetElement(true);
                    prevCursorTarget = ge;
                }

            }
        }
    }

#endregion

    public override void SelectUnit(Unit u) {
        if (u.selectable) {
            base.SelectUnit(u);
            if (u is PlayerUnit) {
                u.ui.ToggleEquipmentButtons();
                if (!u.moved || overrideEquipment) {
                    u.selectedEquipment = u.equipment[0];
                    u.UpdateAction(u.selectedEquipment, u.moveMod);
                }
            } else if (u is EnemyUnit) {
                
            } else if (u is Beacon b) {
                
            }
            prevCursorTargetState = true;
        }
    }

    public void EquipmentSelected(GearData equip = null) {
        if (equip && selectedUnit) {
            if (!equip.multiselect || equip.firstTarget == null) {
                contextuals.StartUpdateCoroutine();
            }
            if (equip.contextualAnimGO != null) { // gear
                contextuals.DisplayGridContextuals(selectedUnit, equip, equip.gridColor);
            } else { // movement
                contextuals.DisplayGridContextuals(selectedUnit, equip, equip.gridColor, selectedUnit.gameObject);
            }
            targetCursorState = equip is MoveData ? PlayerController.CursorState.Move : PlayerController.CursorState.Target;
        } else targetCursorState = PlayerController.CursorState.Default;
    }

    public override void DeselectUnit() {      
        if (selectedUnit is PlayerUnit pu && selectedUnit.selectedEquipment) {
            if (selectedUnit.selectedEquipment is SlagGearData && selectedUnit.selectedEquipment is not HammerData) {
                EquipmentButton butt = selectedUnit.ui.equipButtons.Find(e => e.data is SlagGearData);
                if (butt)
                    butt.DeselectEquipment();
            } else if (selectedUnit.selectedEquipment is HammerData) {
                EquipmentButton butt = selectedUnit.ui.equipButtons.Find(e => e.data is HammerData);
                if (butt)
                    butt.DeselectEquipment();
            } else if (selectedUnit.selectedEquipment is BulbEquipmentData) {
                EquipmentButton butt = selectedUnit.ui.equipButtons.Find(e => e.data is BulbEquipmentData);
                if (butt)
                    butt.DeselectEquipment();
            }

        }
        base.DeselectUnit();
        prevCursorTargetState = false;
        contextuals.displaying = false;
        targetCursorState = PlayerController.CursorState.Default;
    }

    public virtual IEnumerator UnitIsActing() {
        unitActing = true;
        scenario.uiManager.LockHUDButtons(true);
        targetCursorState = PlayerController.CursorState.Default;
        GridMouseOver(new Vector2(-32, -32), false);
        while (unitActing) {
            yield return null;
        }

        turnBlink.BlinkEndTurn();

        scenario.uiManager.LockHUDButtons(false);
        if (overrideEquipment)
            scenario.uiManager.LockPeekButton(true);

        if (selectedUnit) selectedUnit.ui.ToggleEquipmentButtons();
    }

    public void UndoMove() {
        if (undoableMoves.Count > 0 && undoOrder.Count > 0) {
            if (scenario.floorManager.tutorial.isActiveAndEnabled && !scenario.floorManager.tutorial.undoEncountered) {
                scenario.floorManager.tutorial.StartCoroutine(scenario.floorManager.tutorial.UndoTutorial());
                return;
            }

            if (selectedUnit)
                DeselectUnit();
            foreach (Unit u in units) 
                u.UpdateAction();
            PlayerUnit lastMoved = (PlayerUnit)undoOrder[undoOrder.Count - 1];
// Undo bulb harvest
            if (harvestedByMove.ContainsKey(lastMoved)) {
                if (harvestedByMove[lastMoved] is TileBulb harvested) {
                    harvested.UndoHarvest();
                    lastMoved.equipment.Find(e => e is BulbEquipmentData).UnequipEquipment(lastMoved);
                    harvestedByMove.Remove(lastMoved);
                } else if (harvestedByMove[lastMoved] is GodParticleGE particle) {
                    particle.UndoHarvest();
                    //collectedParticles.Remove(particle.type);
                    harvestedByMove.Remove(lastMoved);
                }
            }
// Undo objective scoring
            Tile targetSqr = currentGrid.tiles.Find(tile => tile.coord == lastMoved.coord);
            if (targetSqr.tileType == Tile.TileType.Blood) {
                UnitConditionEvent evt = ObjectiveEvents.UnitConditionEvent;
                evt.condition = Unit.Status.Restricted;
                evt.target = lastMoved;
                evt.undo = true;
                ObjectiveEventManager.Broadcast(evt);
            }
// Snap unit to undo position
            MoveData move = (MoveData)cascadeMovement;
            StartCoroutine(move.MoveToCoord(lastMoved, undoableMoves[lastMoved]));
            lastMoved.moved = false;
            lastMoved.elementCanvas.UpdateStatsDisplay();
// Lazy override for stand in blood count
            targetSqr = currentGrid.tiles.Find(tile => tile.coord == undoableMoves[lastMoved]);
            if (targetSqr.tileType == Tile.TileType.Blood) {
                UnitConditionEvent evt = ObjectiveEvents.UnitConditionEvent;
                evt.condition = Unit.Status.Restricted;
                evt.target = lastMoved;
                evt.undo = true;
                ObjectiveEventManager.Broadcast(evt);
            }

            undoOrder.Remove(lastMoved);
            undoableMoves.Remove(lastMoved);
            UIManager.instance.ToggleUndoButton(undoOrder.Count > 0);

        } else {
            undoOrder = new List<Unit>();
            undoableMoves = new Dictionary<Unit, Vector2>();
            harvestedByMove = new Dictionary<Unit, GridElement>();
        }
    }

    public void TriggerDescent() {
        floorManager.Descend(false, true, nail.coord);
    }

    public virtual void DescendGrids(Grid newGrid) {
        undoableMoves = new Dictionary<Unit, Vector2>();
        undoOrder = new List<Unit>();
        UIManager.instance.ToggleUndoButton(undoOrder.Count > 0);

        for (int i = units.Count - 1; i >= 0; i--) {
            if (currentGrid && currentGrid.gridElements.Contains(units[i]))
                currentGrid.RemoveElement(units[i]);
            
            if (!(floorManager.floorSequence.activePacket.packetType == FloorChunk.PacketType.BOSS && units[i] is Nail))
                units[i].StoreInGrid(newGrid);
                
            if (units[i].conditions.Contains(Unit.Status.Immobilized))
                units[i].RemoveCondition(Unit.Status.Immobilized);
        }
        currentGrid = newGrid;
        transform.parent = newGrid.transform;
        transform.SetSiblingIndex(3);
    }

    public IEnumerator RetrieveNailAnimation() {
        GameObject arm = Instantiate(slimeArmAnim, floorManager.transitionParent);
        nail.transform.parent = floorManager.transitionParent;
        Animator anim = arm.GetComponentInChildren<Animator>();
        nail.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = 1;

        arm.transform.position = nail.transform.position;
        SpriteRenderer sr = anim.GetComponent<SpriteRenderer>();
        sr.sortingOrder = nail.gfx[0].sortingOrder-1;

        yield return new WaitForSecondsRealtime(1.5f);

        nail.ToggleNailState(Nail.NailState.Falling);
        StartCoroutine(floorManager.EndSequenceAnimation(arm));
        yield return new WaitForSecondsRealtime(1.5f);
    }
}
