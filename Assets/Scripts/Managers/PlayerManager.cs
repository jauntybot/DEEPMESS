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
// Turn vars
    public TMPro.TMP_Text energyText;
    public GameObject energyWarning;
    public int currentEnergy, maxEnergy;

    public enum Action { None, Move, Attack }
    public Action currentAction;



    protected override void Start() {
        base.Start();

    }

    public override IEnumerator Initialize()
    {
        yield return base.Initialize();
        pc = GetComponent<PlayerController>();
        if (FloorManager.instance) floorManager = FloorManager.instance;
    }

    public void StartEndTurn(bool start) {
        for (int i = 0; i <= units.Count - 1; i++) 
            units[i].EnableSelection(start);
        

        if (start) {

            currentEnergy = maxEnergy;
            UpdateEnergyDisplay();

            StartCoroutine(pc.GridInput());

// Defense decay
            foreach(Unit u in units) {
                if (u.defense > 0)
                    StartCoroutine(u.TakeDamage(1));
            }
        } else {
            DeselectUnit(true);
            print ("unit deselected from StartEndTurn");
        }
    }

// Overriden functionality
    public override Unit SpawnUnit(Vector2 coord, Unit unit) {
        Unit u = base.SpawnUnit(coord, unit);
        u.owner = Unit.Owner.Player;
        return u;
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
                        print ("unit deselected from GridInput repeat click");
                        DeselectUnit(true);                 
                    } else 
                        SelectUnit(u);
                } else
                    SelectUnit(u);
            }
// Player clicks on enemy unit
            else if (u.owner == Unit.Owner.Enemy) 
            {
                if (selectedUnit && currentAction == Action.Attack) 
                {
                    if (selectedUnit.validAttackCoords.Find(coord => coord == u.coord) != null) {
                        StartCoroutine(AttackWithUnit(u.coord));
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
                    switch (currentAction) {
                        case Action.None:
                        print ("unit deselected from SelectUnit empty sqr");
                            DeselectUnit(true);
                        break;
                        case Action.Move:
                            if (selectedUnit.validMoveCoords.Find(coord => coord == sqr.coord) != null)
                                StartCoroutine(MoveUnit(sqr.coord));
                        break;
                        case Action.Attack:

                        break;
                    }
                }
            }            
        }
    }

    public void ChangeAction(int index) {
        currentAction = (Action)index;
        if (selectedUnit) {
            selectedUnit.UpdateAction((int)currentAction);
        }
    }

    public override void SelectUnit(Unit u) {
        base.SelectUnit(u); 
// Untarget every unit that isn't this one
        foreach(GridElement ge in currentGrid.gridElements) {
            ge.TargetElement(ge == u);
        }
        if (currentAction != Action.None) {
            selectedUnit.UpdateAction((int)currentAction);
        }
    }

    public override void DeselectUnit(bool untarget) {
// Untarget every unit that isn't this one
        foreach(GridElement ge in currentGrid.gridElements) {
            ge.TargetElement(ge == selectedUnit);
        }
        base.DeselectUnit(untarget);
    }
    
    public override IEnumerator MoveUnit(Vector2 moveTo) 
    {
        Coroutine co = StartCoroutine(base.MoveUnit(moveTo));

        yield return co;
        
    }

    public override IEnumerator AttackWithUnit(Vector2 attackAt) 
    {
        Coroutine co = StartCoroutine(base.AttackWithUnit(attackAt));
        yield return co;      
    }

    public override IEnumerator DefendUnit(int value)
    {
        Coroutine co = StartCoroutine(base.DefendUnit(value));

        yield return co;            
    }


    public void UpdateEnergyDisplay() {
        energyText.text = currentEnergy.ToString();
        if (currentEnergy <= 0)
            StartCoroutine(EnergyWarning());
        else
            energyWarning.SetActive(false);
    }

    protected virtual IEnumerator EnergyWarning() {
        for (int i = 0; i < 3; i++) {
            energyWarning.SetActive(false);
            yield return new WaitForSecondsRealtime(.2f);
            energyWarning.SetActive(true);
            yield return new WaitForSecondsRealtime(.2f);
        }
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
