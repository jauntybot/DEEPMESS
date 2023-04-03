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
    public int hammerCharge, descentChargeReq;
    [SerializeField] HammerChargeDisplay chargeDisplay;
    public EquipmentData hammerAction;

    private GridElement prevCursorTarget = null;
    private bool prevCursorTargetState = false;
    [SerializeField] public GameObject nailPrefab, hammerPrefab, hammerPickupPrefab;

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
        hammerCharge = 0;
        chargeDisplay.UpdateCharges(hammerCharge);
        
        yield return base.Initialize();

        List<Unit> initU = new List<Unit>() {
            SpawnUnit(new Vector2(3,4), loadout.unitPrefabs[0]),
            SpawnUnit(new Vector2(4,4), loadout.unitPrefabs[1]),
            SpawnUnit(new Vector2(3,3), loadout.unitPrefabs[2])
        };
        yield return StartCoroutine(loadout.Initialize(initU));

        SpawnHammer((PlayerUnit)units[0], (HammerData)hammerAction);
        foreach (Unit u in initU) {
            StartCoroutine(floorManager.DropUnit(u, u.transform.position, currentGrid.PosFromCoord(u.coord)));
        }
        
        nail = (Nail)SpawnUnit(new Vector3(0, 30), nailPrefab.GetComponent<Nail>());
        nail.gameObject.transform.parent = unitParent.transform;
        yield return StartCoroutine(DropNail());


        pc = GetComponent<PlayerController>();
        if (FloorManager.instance) floorManager = FloorManager.instance;
    }

// Spawn a new instance of a hammer and update hammer actions
    public virtual void SpawnHammer(PlayerUnit unit, HammerData equip) {
        GameObject h = Instantiate(hammerPrefab, unit.transform.position, Quaternion.identity, unit.transform);
        h.GetComponentInChildren<SpriteRenderer>().sortingOrder = unit.gfx[0].sortingOrder;
        unit.gfx.Add(h.GetComponentInChildren<SpriteRenderer>());
        unit.equipment.Insert(3, equip);
        equip.EquipEquipment(unit);
        equip.AssignHammer(h, nail);
        
        unit.ui.UpdateEquipmentButtons();
    }

// Initializes or closes functions for turn start/end
    public void StartEndTurn(bool start, bool newFloor = false) {
        for (int i = 0; i <= units.Count - 1; i++) {
            units[i].EnableSelection(start);
        }
        

        if (start) {
            StartCoroutine(pc.GridInput());
            if (!newFloor) {
// Reset unit energy if not continued turn
                foreach(Unit u in units) {
                    if (u is PlayerUnit) {
                        u.energyCurrent = u.energyMax;
                        u.elementCanvas.UpdateStatsDisplay();
                    }
                }
                ResolveConditions();
            } else {

            }
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

    public void ChargeHammer(int charge) {
        if (hammerCharge < descentChargeReq) {
            hammerCharge += charge;
            if (hammerCharge > descentChargeReq) hammerCharge = descentChargeReq;
            chargeDisplay.UpdateCharges(hammerCharge);
        }
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
            }
        }

        nail.transform.position = currentGrid.PosFromCoord(spawn) + new Vector3(0, floorManager.floorOffset, 0);
        nail.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = 1;
        yield return StartCoroutine(UpdateNail(spawn));
        nail.collisionChance = 90;
        UIManager.instance.UpdateDropChance(nail.collisionChance);
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
                    if (u == selectedUnit) 
                    {  
                        DeselectUnit();                 
                    }
                    else if (selectedUnit.ValidCommand(u.coord)) {
                        selectedUnit.ExecuteAction(u);
                        DeselectUnit();
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
                        DeselectUnit();
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
                        DeselectUnit();
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
                    DeselectUnit();
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

    public override void SelectUnit(Unit t)
    {
        base.SelectUnit(t);
        if (t.energyCurrent > 0) t.ui.ToggleEquipmentPanel(true);
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

    public void TriggerDescent() {
        floorManager.Descend();
    }

    public virtual void DescendGrids(Grid newGrid) {
        foreach(Unit unit in units) {
            currentGrid.RemoveElement(unit);
            unit.StoreInGrid(newGrid);
            if (unit is not Nail)
                unit.UpdateElement(unit.coord);
        }
        currentGrid = newGrid;
        transform.parent = newGrid.transform;
        transform.SetSiblingIndex(3);
        hammerCharge = 0;
        chargeDisplay.UpdateCharges(hammerCharge);
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
