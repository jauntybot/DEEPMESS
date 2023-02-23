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
    [SerializeField] public GameObject nailPrefab, hammerPrefab, hammerPickupPrefab;

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
        unit.canvas.UpdateEquipmentDisplay();
    }

// Initializes or closes functions for turn start/end
    public void StartEndTurn(bool start) {
        for (int i = 0; i <= units.Count - 1; i++) {
            units[i].EnableSelection(start);
        }
        

        if (start) {
            StartCoroutine(pc.GridInput());
// Reset unit energy
            foreach(Unit u in units) {
                u.energyCurrent = u.energyMax;
                u.elementCanvas.UpdateStatsDisplay();
            }
        } else {
            DeselectUnit();
        }
    }

// Overriden functionality
    public override Unit SpawnUnit(Vector2 coord, Unit unit) {
        Unit u = base.SpawnUnit(coord, unit);
        u.owner = Unit.Owner.Player;
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

        bool nonPlayerCoord = false;
        Vector2 spawn = Vector2.zero;
        while (!nonPlayerCoord) {
            nonPlayerCoord = true;
            spawn = new Vector2(Random.Range(1,6), Random.Range(1,6));
            foreach(Unit u in units) {
                if (u.coord == spawn) nonPlayerCoord = false;
            }
        }

        float xOffset = currentGrid.PosFromCoord(spawn).x;
        nail.transform.position = new Vector3(xOffset, nail.transform.position.y, 0);
        yield return StartCoroutine(UpdateNail(spawn));
    }

    public IEnumerator UpdateNail(Vector2 coord) {
        nail.transform.parent = unitParent.transform;
        GridElement subGE = currentGrid.gridElements.Find(ge => ge.coord == coord);
        
        if (subGE != null) {
            StartCoroutine(subGE.CollideFromBelow(nail));

        }
        yield return StartCoroutine(nail.nailDrop.MoveToCoord(nail, coord));
        
            
        nail.StoreInGrid(currentGrid);

    }

// Get grid input from player controller, translate it to functionality
    public void GridInput(GridElement input) {
// Player clicks on unit
        if (input is Unit u) 
        {
// Player clicks on their own unit
            if (u.owner == Unit.Owner.Player) 
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
            else if (u.owner == Unit.Owner.Enemy) 
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

    }

    protected override void RemoveUnit(GridElement ge)
    {
        base.RemoveUnit(ge);
        if (units.Count <= 0) {
            scenario.Lose();            
        }
    }

    public void TriggerDescent() {
        floorManager.Descend();
    }

    public virtual void DescendGrids(Grid newGrid) {
        foreach(Unit unit in units) {
            currentGrid.RemoveElement(unit);
            unit.StoreInGrid(newGrid);
        }
        currentGrid = newGrid;
        transform.parent = newGrid.transform;
        hammerCharge = 0;
        chargeDisplay.UpdateCharges(hammerCharge);
    }
}
