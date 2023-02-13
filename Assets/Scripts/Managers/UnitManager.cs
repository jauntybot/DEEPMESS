using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base class for enemy and player managers

public class UnitManager : MonoBehaviour {

// Global refs
    [SerializeField]
    protected Grid currentGrid;
    protected FloorManager floorManager;
    [HideInInspector] public ScenarioManager scenario;

    [Header("UNIT MANAGER")]
// Unit vars
    [SerializeField] GameObject[] unitPrefabs;
    [SerializeField] protected GameObject unitParent;
    public List<Unit> units = new List<Unit>();    
    public Unit selectedUnit;
    public List<Vector2> startingCoords;


// Called from scenario manager when game starts
    public virtual IEnumerator Initialize() 
    {
// Grab global refs
        if (ScenarioManager.instance) scenario = ScenarioManager.instance;
        if (FloorManager.instance) floorManager = FloorManager.instance;
        currentGrid = floorManager.currentFloor;

        for (int i = 0; i <= startingCoords.Count - 1; i++) {
            SpawnUnit(startingCoords[i], unitPrefabs[i].GetComponent<Unit>());
        }
        yield return null;
    }

// Create a new unit from prefab index, update its GridElement
    public virtual Unit SpawnUnit(Vector2 coord, Unit unit) 
    {
        Unit u = Instantiate(unit.gameObject, unitParent.transform).GetComponent<Unit>();
        u.StoreInGrid(currentGrid);
        u.UpdateElement(coord);

        units.Add(u);
        SubscribeElement(u);
        return u;
    }

// Inherited functionality dependent on inherited classes
    public virtual void SelectUnit(Unit t) {
        if (selectedUnit) {
            DeselectUnit();
        }

        t.TargetElement(true);
        selectedUnit = t;
        t.selected = true;

        currentGrid.DisplayGridCursor(true, t.coord);
    }
    public virtual void DeselectUnit() {
        // Untarget every unit that isn't this one
        foreach(GridElement ge in currentGrid.gridElements) 
            ge.TargetElement(false);
            
        if (selectedUnit) {
// Clear action data
            
            selectedUnit.TargetElement(false);

            selectedUnit.selectedEquipment = null;
            selectedUnit.validActionCoords = null;
            selectedUnit.selected = false;
            selectedUnit = null;

            currentGrid.DisplayGridCursor(true, Vector2.one * -32);
            currentGrid.DisableGridHighlight();
        }            
        
    }
    public virtual IEnumerator MoveUnit(Unit unit, Vector2 moveTo, int cost = 0) {

        DeselectUnit();

        //yield return StartCoroutine(unit.JumpToCoord(moveTo));
        unit.UpdateAction();

        yield return new WaitForSecondsRealtime(.5f);
        unit.TargetElement(false);
    }

    public virtual IEnumerator AttackWithUnit(Unit unit, Vector2 attackAt, int cost = 0) {
         
        Unit recipient = currentGrid.CoordContents(attackAt) as Unit;
        foreach(Vector2 coord in selectedUnit.validActionCoords) {
            if (currentGrid.CoordContents(coord) is Unit u) {
                u.TargetElement(u == recipient);
            }
        }

        DeselectUnit();    
        //yield return StartCoroutine(unit.AttackUnit(recipient));

        unit.UpdateAction();
        yield return new WaitForSecondsRealtime(.5f);
        unit.TargetElement(false);
    }

    public virtual void SubscribeElement(GridElement ge) {
        ge.ElementDestroyed += RemoveUnit;
    }

    protected virtual void RemoveUnit(GridElement ge) {
        units.Remove(ge as Unit);
    }
}

