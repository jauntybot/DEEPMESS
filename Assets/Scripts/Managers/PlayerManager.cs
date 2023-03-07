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
    
    public Nail nail;
    public int hammerCharge, descentChargeReq;
    [SerializeField] HammerChargeDisplay chargeDisplay;
    public List<EquipmentData> hammerActions;
    [SerializeField] PlacementData sharedMines;

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

        sharedMines.count = 15;
        
        yield return base.Initialize();

        nail = (Nail)SpawnUnit(Vector2.zero, nailPrefab.GetComponent<Nail>());
        nail.gameObject.transform.position = new Vector3 (0,20,0);
        nail.gameObject.transform.parent = unitParent.transform;

        SpawnHammer((PlayerUnit)units[0], hammerActions);

        pc = GetComponent<PlayerController>();
        if (FloorManager.instance) floorManager = FloorManager.instance;
    }

// Spawn a new instance of a hammer and update hammer actions
    public virtual void SpawnHammer(PlayerUnit unit, List<EquipmentData> actions) {
        GameObject h = Instantiate(hammerPrefab, unit.transform.position, Quaternion.identity, unit.transform);
        h.GetComponentInChildren<SpriteRenderer>().sortingOrder = unit.gfx[0].sortingOrder;
        unit.gfx.Add(h.GetComponentInChildren<SpriteRenderer>());
        foreach(HammerData action in actions) {
            unit.equipment.Add(action);
            action.EquipEquipment(unit);
            action.AssignHammer(h, nail);
        }
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
                        u.ui.UpdateEnergy();
                    }
                }
            } else {
                nail.collisionChance = 90;
                UIManager.instance.UpdateDropChance(nail.collisionChance);
            }
        } else {
            DeselectUnit();
        }
    }

// Overriden functionality
    public override Unit SpawnUnit(Vector2 coord, Unit unit) {
        Unit u = base.SpawnUnit(coord, unit);

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
            }
        }
        float xOffset = currentGrid.PosFromCoord(spawn).x;
        nail.transform.position = new Vector3(xOffset, nail.transform.position.y, 0);
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
            GridElement contents = currentGrid.CoordContents(sqr.coord);
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

    public override void SelectUnit(Unit t)
    {
        base.SelectUnit(t);
        if (t.energyCurrent > 0) t.ui.ToggleEquipmentPanel(true);
    }

    public override void DeselectUnit()
    {
        base.DeselectUnit();
        turnBlink.BlinkEndTurn();
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
        hammerCharge = 0;
        chargeDisplay.UpdateCharges(hammerCharge);
    }

    public void DisplayAllHP(bool active) {
        foreach (Unit u in units) 
            u.TargetElement(active);
        foreach(Unit u in scenario.currentEnemy.units)
            u.TargetElement(active);
    }
}
