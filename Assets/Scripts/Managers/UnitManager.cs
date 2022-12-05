using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base class for enemy and player managers

public class UnitManager : MonoBehaviour {

// Global refs
    protected Grid grid;
    [HideInInspector] public ScenarioManager scenario;

// Unit vars
    [SerializeField] GameObject[] unitPrefabs;
    public List<Unit> units = new List<Unit>();    
    public Unit selectedUnit;
    public List<Vector2> startingCoords;



    protected virtual void Start() 
    {
// Grab global refs
        if (ScenarioManager.instance) scenario = ScenarioManager.instance;
        if (Grid.instance) grid = Grid.instance;

    }

// Called from scenario manager when game starts
    public virtual IEnumerator Initialize() 
    {
        for (int i = 0; i <= startingCoords.Count - 1; i++) {
            SpawnUnit(startingCoords[i], i);
            yield return new WaitForSeconds(Util.initD);
        }
    }

// Create a new unit from prefab index, update its GridElement
    public virtual Unit SpawnUnit(Vector2 coord, int index) 
    {
        Unit unit = Instantiate(unitPrefabs[index], this.transform).GetComponent<Unit>();
        unit.UpdateElement(coord);

        units.Add(unit);
        unit.ElementDestroyed += RemoveUnit;
        return unit;
    }

// Inherited functionality dependent on inherited classes
    public virtual void SelectUnit(Unit t) {
        if (selectedUnit)
            DeselectUnit();

        t.hpDisplay.ToggleHPDisplay(true);
        selectedUnit = t;

        grid.DisplayGridCursor(true, t.coord);
    }
    public virtual void DeselectUnit() {
        selectedUnit.hpDisplay.ToggleHPDisplay(false);
        selectedUnit = null;

        grid.DisplayGridCursor(true, Vector2.one * -32);
        grid.DisableGridHighlight();
    }
    public virtual IEnumerator MoveUnit(Vector2 moveTo) {
        yield return new WaitForSecondsRealtime(1/Util.fps);
        Unit unit = selectedUnit; 
        yield return StartCoroutine(unit.JumpToCoord(moveTo));
        yield return new WaitForSecondsRealtime(.2f);
        DeselectUnit();
    }
    public virtual IEnumerator AttackWithUnit(Vector2 attackAt) {
        yield return new WaitForSecondsRealtime(1/Util.fps);
        Unit unit = selectedUnit;
       
        Unit recipient = grid.CoordContents(attackAt) as Unit;
        yield return StartCoroutine(unit.AttackUnit(recipient));
        yield return new WaitForSecondsRealtime(.2f);
        recipient.hpDisplay.ToggleHPDisplay(false);
        DeselectUnit();
    }

    public virtual IEnumerator DefendUnit(int value) {
        yield return new WaitForSecondsRealtime(1/Util.fps);
        Unit unit = selectedUnit;

        yield return StartCoroutine(unit.Defend(value));
        yield return new WaitForSecondsRealtime(.2f);
        DeselectUnit();
    }

    protected virtual void RemoveUnit(GridElement ge) {
        units.Remove(ge as Unit);
    }
}

