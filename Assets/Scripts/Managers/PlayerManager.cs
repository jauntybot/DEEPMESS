using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Class that controls the player's functionality, recieves input from PlayerController and ScenarioManager
// tbh I'm not commenting this one bc it's the most heavily edited and refactors are incoming

[RequireComponent(typeof(Deck))]
[RequireComponent(typeof(PlayerController))]
public class PlayerManager : UnitManager {
    

    [HideInInspector] public PlayerController pc;

    [Header("PLAYER MANAGER")]
// Turn vars
    public TMPro.TMP_Text energyText;
    public GameObject energyWarning;
    public int currentEnergy, maxEnergy;



    protected override void Start() {
        base.Start();
        pc = GetComponent<PlayerController>();
    }

    public override IEnumerator Initialize()
    {
        yield return base.Initialize();
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
        }
    }

// Overriden functionality
    public override Unit SpawnUnit(Vector2 coord, Unit unit) {
        Unit u = base.SpawnUnit(coord, unit);
        u.owner = Unit.Owner.Player;
        return u;
    }

    public override void SelectUnit(Unit u) {
        base.SelectUnit(u); 
        
    }
    
    public override IEnumerator MoveUnit(Vector2 moveTo) 
    {
        if (PlayCard()) {
            Coroutine co = StartCoroutine(base.MoveUnit(moveTo));

            yield return co;
        }
    }

    public override IEnumerator AttackWithUnit(Vector2 attackAt) 
    {
        if (PlayCard()) {
            Coroutine co = StartCoroutine(base.AttackWithUnit(attackAt));
            yield return co;      
        }
    }

    public override IEnumerator DefendUnit(int value)
    {
        if (PlayCard()) {
            Coroutine co = StartCoroutine(base.DefendUnit(value));

            yield return co;            
        }
    }

    public bool PlayCard() {



            StartCoroutine(EnergyWarning());
            return false;
        
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

// Get grid input from player controller, translate it to functionality
    public void GridInput(GridElement input) {
        if (input is Unit u) 
        {
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
            else if (u.owner == Unit.Owner.Enemy) 
            {
                if (selectedUnit) 
                {
                                    
                }
            }
        }
        else if (input is GridSquare sqr) 
        {
// Recurse this function with reference to square contents if found
            GridElement contents = grid.CoordContents(sqr.coord);
            if (contents)
                GridInput(contents);
            else {
                if (selectedUnit) {
                    // switch () {
                    //     case CardData.Action.Move:
                    //         if (selectedUnit.validMoveCoords.Find(coord => coord == sqr.coord) != null)
                    //             StartCoroutine(MoveUnit(sqr.coord));
                    //     break;
                    //     case CardData.Action.Attack:

                    //     break;
                    // }
                }
            }            
        }


    }

    protected override void RemoveUnit(GridElement ge)
    {
        base.RemoveUnit(ge);
        if (units.Count <= 0) {
            scenario.Lose();            
        }
    }
}
