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
    
    public enum Status { Normal, Immobilized }
    [Header("Modifiers")]
    public Status status;
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


#endregion


#region Unit Functionality
    
    public virtual IEnumerator CollideFromAbove(GridElement subGE) {

        yield return StartCoroutine(TakeDamage(1));

        // yield return new WaitForSecondsRealtime(1);
        // float timer = 0;
        // Vector3 bumpUp = transform.position + Vector3.up * 2;
        // while (timer<animDur) {
        //     yield return null;
        //     transform.position = Vector3.Lerp(transform.position, bumpUp, timer/animDur);
        //     timer += Time.deltaTime;
        // }
        // timer = 0;
        // StartCoroutine(TakeDamage(1));
        // while (timer<animDur) {
        //     yield return null;
        //     transform.position = Vector3.Lerp(transform.position, grid.PosFromCoord(moveTo), timer/animDur);

        //     timer += Time.deltaTime;
        // }   

        // UpdateElement(moveTo);
        

    }

    public virtual void ApplyCondition(Status s) {
        status = s;
        switch(status) {
            default: return;
            case Status.Normal: return;
            case Status.Immobilized:
                prevMod = moveMod;
                moveMod -= 10;
                ui.UpdateEquipmentButtonMods();
            break;
        }
    }

    public virtual void RemoveCondition() {
        switch(status) {
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


#endregion

}