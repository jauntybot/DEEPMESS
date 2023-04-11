using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : GridElement {


    [Header("Unit")]
    public UnitManager manager;
    public bool selected;
    public EquipmentData selectedEquipment;
    public List<EquipmentData> equipment;
    public bool moved;

    public List<Vector2> validActionCoords;
    
    public enum Status { Normal, Immobilized, Restricted }
    [Header("Modifiers")]
    public List<Status> conditions;
    private int prevMod;
    public int moveMod;
    public int attackMod;

    [Header("UI/UX")]
    public UnitUI ui;
    public Sprite portrait;
    [SerializeField] float animDur = 1f;


// Functions that will change depending on the class they're inherited from
#region Inherited Functionality

    public virtual void UpdateAction(EquipmentData equipment = null, int mod = 0) {
// Clear data
        validActionCoords = null;
        grid.DisableGridHighlight();
// Assign new data if provided
        selectedEquipment = equipment;
        if (selectedEquipment) {
            validActionCoords = selectedEquipment.TargetEquipment(this, mod);
        }
    }

    public virtual void ExecuteAction(GridElement target = null) {
        if (selectedEquipment) StartCoroutine(selectedEquipment.UseEquipment(this, target));
    }

    public bool ValidCommand(Vector2 target) {
        if (selectedEquipment == null) return false;
        if (validActionCoords.Count == 0) return false;
        if (validActionCoords.Find(coord => coord == target) == default) return false;
        if (energyCurrent < selectedEquipment.energyCost && selectedEquipment is not MoveData) return false;
        else if (moved && selectedEquipment is MoveData) return false;

        return true;
    }

    public override void UpdateElement(Vector2 c) {
        base.UpdateElement(c);
        if (grid.sqrs.Find(sqr => sqr.coord == c) is BloodTile) {

        } else {

        }
    }


#endregion


#region Unit Functionality
    
    public virtual IEnumerator CollideFromAbove(GridElement subGE) {

        yield return StartCoroutine(TakeDamage(1));
    }

    public virtual void ApplyCondition(Status s) {
        if (!conditions.Contains(s)) {
            conditions.Add(s);
            switch(s) {
                default: return;
                case Status.Normal: return;
                case Status.Immobilized:
                    prevMod = moveMod;
                    moveMod -= 10;
                    ui.UpdateEquipmentButtonMods();
                break;
            }
        }
    }

    public virtual void RemoveCondition(Status s) {
        if (conditions.Contains(s)) {
            switch(s) {
                default: return;
                case Status.Normal: return;
                case Status.Immobilized:
                    moveMod = prevMod;
                    ui.UpdateEquipmentButtonMods();
                    foreach(GridElement ge in grid.CoordContents(coord)) {
                        if (ge is ImmobilizeGoo goo) {
                            print("goo found");
                            Destroy(goo.gameObject);
                        }
                    }
                break;
            }
        }
    }


#endregion

}