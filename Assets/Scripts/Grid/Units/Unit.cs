using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : GridElement {

    public enum Owner { Player, Enemy }

    [Header("Unit")]
    public Owner owner;
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

    public virtual void ExecuteAction() {
        if (selectedEquipment) selectedEquipment.UseEquipment(this);
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