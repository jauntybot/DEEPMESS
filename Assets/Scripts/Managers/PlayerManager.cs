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
    
    public Drill drill;
    public List<EquipmentData> hammerActions;
    [SerializeField] public GameObject drillPrefab, hammerPrefab;

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

        PlayerUnit u = (PlayerUnit)units[0];
        foreach(EquipmentData action in hammerActions) {
            u.equipment.Add(action);
            action.EquipEquipment(u);
        }
        u.canvas.UpdateEquipmentDisplay();

        pc = GetComponent<PlayerController>();
        if (FloorManager.instance) floorManager = FloorManager.instance;

        drill = (Drill)SpawnUnit(Vector2.zero, drillPrefab.GetComponent<Drill>());
        drill.gameObject.transform.position = new Vector3 (0,20,0);
        drill.gameObject.transform.parent = unitParent.transform;
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

    public IEnumerator UpdateDrill(Vector2 coord) {
        drill.transform.parent = unitParent.transform;
        yield return StartCoroutine(drill.drillDrop.MoveToCoord(drill, coord));
        drill.StoreInGrid(currentGrid);

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
                    } else 
                        SelectUnit(u);
                } else
                    SelectUnit(u);
            }
// Player clicks on enemy unit
            else if (u.owner == Unit.Owner.Enemy) 
            {
                if (selectedUnit && selectedUnit.selectedEquipment) 
                {
// Unit is a target of valid action adjacency
                    if (selectedUnit.validActionCoords.Find(coord => coord == u.coord) != null
                        && selectedUnit.energyCurrent > 0) {
                        StartCoroutine(selectedUnit.selectedEquipment.UseEquipment(selectedUnit, u));
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
                    if (selectedUnit.selectedEquipment &&
                    selectedUnit.validActionCoords.Find(coord => coord == sqr.coord) != null &&
                    selectedUnit.energyCurrent > 0) {
                        currentGrid.DisableGridHighlight();
                        StartCoroutine(selectedUnit.selectedEquipment.UseEquipment(selectedUnit, sqr));
                        DeselectUnit();
                    } else {
                        DeselectUnit();
                    }
                }
            }            
        }
    }


    public override void SelectUnit(Unit u) {
        base.SelectUnit(u); 
        PlayerUnit pu = (PlayerUnit)u;
// Disable equipment display if out of energy
        if (pu.energyCurrent == 0) pu.canvas.ToggleEquipmentDisplay(false);
// Untarget every unit that isn't this one
        foreach(GridElement ge in currentGrid.gridElements) 
            ge.TargetElement(ge == u);
        
    }

    
    public override IEnumerator MoveUnit(Unit unit, Vector2 moveTo, int cost = 0) 
    {
            unit.energyCurrent -= cost;
            Coroutine co = StartCoroutine(base.MoveUnit(unit, moveTo));
            yield return co;
    }

    public override IEnumerator AttackWithUnit(Unit unit, Vector2 attackAt, int cost = 0) 
    {
            selectedUnit.energyCurrent -= cost;
            Coroutine co = StartCoroutine(base.AttackWithUnit(unit, attackAt));
            yield return co;   
    }

    protected override void RemoveUnit(GridElement ge)
    {
        base.RemoveUnit(ge);
        if (units.Count <= 0) {
            scenario.Lose();            
        }
    }

    public virtual void DescendGrids(Grid newGrid) {
        foreach(Unit unit in units) {
            currentGrid.RemoveElement(unit);
            unit.StoreInGrid(newGrid);
        }
        currentGrid = newGrid;
        transform.parent = newGrid.transform;
    }
}
