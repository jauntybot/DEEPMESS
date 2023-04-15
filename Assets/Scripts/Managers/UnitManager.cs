using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base class for enemy and player managers

public class UnitManager : MonoBehaviour {

// Global refs
    [SerializeField]
    public Grid currentGrid;
    protected FloorManager floorManager;
    [HideInInspector] public ScenarioManager scenario;

    [Header("UNIT MANAGER")]
    [SerializeField] protected GameObject unitParent;
    public List<Unit> units = new List<Unit>();    
    public Unit selectedUnit;


// Called from scenario manager when game starts
    public virtual IEnumerator Initialize() 
    {
// Grab global refs
        if (ScenarioManager.instance) scenario = ScenarioManager.instance;
        if (FloorManager.instance) floorManager = FloorManager.instance;
        currentGrid = floorManager.currentFloor;

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
        u.manager = this;

        UIManager.instance.UpdatePortrait(u, false);

        return u;
    }

// Inherited functionality dependent on inherited classes
    public virtual void SelectUnit(Unit u) {
        if (selectedUnit) {
            DeselectUnit();
        }

        selectedUnit = u;
        u.TargetElement(true);
        u.selected = true;

        UIManager.instance.UpdatePortrait(u);

        currentGrid.UpdateSelectedCursor(true, u.coord);
        AudioManager.PlaySound(AudioAtlas.Sound.selectionUnit, u.gameObject.transform.position);
    }
    public virtual void DeselectUnit() {
// Untarget every unit
        foreach(GridElement ge in currentGrid.gridElements) 
            ge.TargetElement(false);
        

        if (selectedUnit) {

            UIManager.instance.UpdatePortrait(selectedUnit, false);
// Clear action data
            selectedUnit.TargetElement(false);
            if (selectedUnit.selectedEquipment)
                selectedUnit.selectedEquipment.UntargetEquipment(selectedUnit);
            
            selectedUnit.selectedEquipment = null;
            selectedUnit.validActionCoords = null;
            selectedUnit.selected = false;
            selectedUnit = null;

            currentGrid.UpdateSelectedCursor(false, Vector2.one * -32);
            currentGrid.DisableGridHighlight();
        }            
        
    }

    public virtual void ResolveConditions() {
        foreach(Unit u in units) {
            if (u.conditions != null) {
                foreach (Unit.Status s in u.conditions) 
                u.RemoveCondition(s);
            }
        }
    }

    public virtual void SubscribeElement(GridElement ge) {
        ge.ElementDestroyed += RemoveUnit;
    }

    protected virtual void RemoveUnit(GridElement ge) {
        units.Remove(ge as Unit);
    }
}

