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
    [SerializeField] public GameObject drillPrefab;
    public GameObject instancedDrill;
    public Drill drill;

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
        pc = GetComponent<PlayerController>();
        if (FloorManager.instance) floorManager = FloorManager.instance;
        instancedDrill = SpawnUnit(Vector2.zero, drillPrefab.GetComponent<Drill>()).gameObject;
        drill = instancedDrill.GetComponent<Drill>();
        drill.gameObject.transform.position = new Vector3 (0,20,0);
        drill.gameObject.transform.parent = unitParent.transform;
    }

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
            DeselectUnit(true);
        }
    }

// Overriden functionality
    public override Unit SpawnUnit(Vector2 coord, Unit unit) {
        Unit u = base.SpawnUnit(coord, unit);
        u.owner = Unit.Owner.Player;

        return u;
    }

    public IEnumerator UpdateDrill(Vector2 coord) {
        drill.transform.parent = unitParent.transform;
        yield return StartCoroutine(MoveUnit(drill, coord));
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
                        DeselectUnit(true);                 
                    } else 
                        SelectUnit(u);
                } else
                    SelectUnit(u);
            }
// Player clicks on enemy unit
            else if (u.owner == Unit.Owner.Enemy) 
            {
                if (selectedUnit && selectedUnit.selectedEquipment.action == EquipmentData.Action.Attack) 
                {
                    if (selectedUnit.validActionCoords.Find(coord => coord == u.coord) != null
                        && selectedUnit.energyCurrent > 0) {
                        selectedUnit.energyCurrent -= 1;
                        StartCoroutine(AttackWithUnit(selectedUnit, u.coord));
                    } 
                }
            }
        }
// Player clicks on square
        else if (input is GridSquare sqr) 
        {
// Check if square is empty
            GridElement contents = floorManager.currentFloor.CoordContents(sqr.coord);
// Square not empty, recurse this function with reference to square contents
            if (contents)
                GridInput(contents);
// Square empty
            else {
                if (selectedUnit) {
                    switch (selectedUnit.selectedEquipment.action) {
                        case EquipmentData.Action.None:
                            DeselectUnit(true);
                        break;
                        case EquipmentData.Action.Move:
                            if (selectedUnit.validActionCoords.Find(coord => coord == sqr.coord) != null
                                && selectedUnit.energyCurrent > 0)
                                selectedUnit.energyCurrent -= 1;
                                StartCoroutine(MoveUnit(selectedUnit, sqr.coord));
                        break;
                        case EquipmentData.Action.Attack:

                        break;
                    }
                }
            }            
        }
    }


    public override void SelectUnit(Unit u) {
        base.SelectUnit(u); 

        foreach(GridElement ge in currentGrid.gridElements) {
// Untarget every unit that isn't this one
            ge.TargetElement(ge == u);
        }
    }

    
    public override IEnumerator MoveUnit(Unit unit, Vector2 moveTo, int cost = 0) 
    {
            unit.energyCurrent -= cost;
            Coroutine co = StartCoroutine(base.MoveUnit(unit, moveTo));
            yield return co;
    }

    public override IEnumerator AttackWithUnit(Unit unit, Vector2 attackAt) 
    {
            selectedUnit.energyCurrent -= 1;
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
