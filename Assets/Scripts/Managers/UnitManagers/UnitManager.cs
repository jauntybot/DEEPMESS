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
    public GameObject unitParent;
    public List<Unit> units = new();    
    public Unit selectedUnit;
    public bool unitActing = false;
    [SerializeField] protected GameObject unitDescentPreview;


// Called from scenario manager when game starts
    public virtual IEnumerator Initialize(Grid _currentGrid) {
// Grab global refs
        if (ScenarioManager.instance) scenario = ScenarioManager.instance;
        if (FloorManager.instance) floorManager = FloorManager.instance;
        currentGrid = _currentGrid;

        yield return null;
    }

// Create a new unit from prefab index, update its GridElement
    public virtual Unit SpawnUnit(Unit unit, Vector2 coord) {
        Unit u = Instantiate(unit.gameObject, unitParent.transform).GetComponent<Unit>();

        units.Add(u);
        SubscribeElement(u);
        u.manager = this;

        UIManager.instance.UpdatePortrait(u, false);
        if (unit is not Nail) {
            DescentPreview dp = Instantiate(unitDescentPreview, floorManager.previewManager.transform).GetComponent<DescentPreview>();
            dp.Initialize(u, floorManager.previewManager);
        }

        u.StoreInGrid(currentGrid);
        u.UpdateElement(coord);

        return u;
    }

    public virtual Unit SpawnUnit(Unit unit) {
        Unit u = Instantiate(unit.gameObject, unitParent.transform).GetComponent<Unit>();

        units.Add(u);
        SubscribeElement(u);
        u.manager = this;

        UIManager.instance.UpdatePortrait(u, false);
        if (unit is not Nail) {
            DescentPreview dp = Instantiate(unitDescentPreview, floorManager.previewManager.transform).GetComponent<DescentPreview>();
            dp.Initialize(u, floorManager.previewManager);
        }

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

        UIManager.instance.UpdatePortrait(u, true);

        currentGrid.UpdateSelectedCursor(true, u.coord);
    
        u.PlaySound(u.selectedSFX);
    }

    public virtual void DeselectUnit() {     
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
            if (u.conditions.Count > 0) {
                for (int i = u.conditions.Count - 1; i >= 0; i--) {
                    if (u.conditions[i] is Unit.Status.Weakened) u.RemoveCondition(u.conditions[i]);
                }
                
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

