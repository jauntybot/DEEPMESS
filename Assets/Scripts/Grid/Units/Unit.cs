using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : GridElement {


    public enum Owner { Player, Enemy }

    [Header("Unit")]
    public UnitManager manager;
    public Owner owner; // Depreciate in favor of manager ^
    public bool selected;
    public List<EquipmentData> equipment;
    public EquipmentData selectedEquipment;

    public List<Vector2> validActionCoords;

    [SerializeField] float animDur = 1f;

// Functions that will change depending on the class they're inherited from
#region Inherited Functionality

    public virtual void UpdateAction(EquipmentData equipment = null) {
// Clear data
        validActionCoords = null;
        grid.DisableGridHighlight();
// Assign new data if provided
        selectedEquipment = equipment;
        if (selectedEquipment) {
            validActionCoords = selectedEquipment.TargetEquipment(this);
        }
    }

    public virtual void ExecuteAction(GridElement target = null) {
        if (selectedEquipment) StartCoroutine(selectedEquipment.UseEquipment(this, target));
    }

    public bool ValidCommand(Vector2 target) {
        if (selectedEquipment == null) return false;
        if (validActionCoords.Count == 0) return false;
        if (validActionCoords.Find(coord => coord == target) == default) return false;
        if (energyCurrent < selectedEquipment.energyCost) return false;

        return true;
    }


#endregion

// The basics of being a unit; movement, HP, attacking
#region Unit Functionality
    
    public virtual IEnumerator CollideOnDescent(Vector2 moveTo) {
        yield return new WaitForSecondsRealtime(1);
        float timer = 0;
        Vector3 bumpUp = transform.position + Vector3.up * 2;
        while (timer<animDur) {
            yield return null;
            transform.position = Vector3.Lerp(transform.position, bumpUp, timer/animDur);
            timer += Time.deltaTime;
        }
        timer = 0;
        StartCoroutine(TakeDamage(1));
        while (timer<animDur) {
            yield return null;
            transform.position = Vector3.Lerp(transform.position, grid.PosFromCoord(moveTo), timer/animDur);

            timer += Time.deltaTime;
        }   

        UpdateElement(moveTo);
    }


#endregion

}