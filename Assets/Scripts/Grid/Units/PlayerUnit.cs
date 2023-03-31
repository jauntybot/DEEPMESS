using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Inherited Unit; unit functionality dependent on player input

public class PlayerUnit : Unit {

    public int consumableCount;
    
// Called when an action is applied to a unit or to clear it's actions
    public override void UpdateAction(EquipmentData equipment = null, int mod = 0) 
    {
        if (equipment is ConsumableEquipmentData && consumableCount > 0)
            base.UpdateAction(equipment, mod);
        else if (equipment is not ConsumableEquipmentData)
            base.UpdateAction(equipment, mod);
    }

// Allow the player to click on this
    public override void EnableSelection(bool state) 
    {
        selectable = state;
        hitbox.enabled = state;
    }

    public override void TargetElement(bool state)
    {
        base.TargetElement(state);
        ui.ToggleEquipmentPanel(state);
        if (energyCurrent == 0 || manager.selectedUnit != this) ui.ToggleEquipmentPanel(false);
    }

    public override IEnumerator DestroyElement() {

        bool droppedHammer = false;
        EquipmentPickup pickup = null;
        foreach (EquipmentData equip in equipment) {
            if (equip is HammerData hammer) {
                if (!droppedHammer) {
                    PlayerManager m = (PlayerManager)manager;
                    pickup = Instantiate(m.hammerPickupPrefab, transform.position, Quaternion.identity, grid.neutralGEContainer.transform).GetComponent<EquipmentPickup>();
                    pickup.StoreInGrid(grid);
                    pickup.UpdateElement(coord);
                    droppedHammer = true;
                }
                if (pickup != null) {
                    pickup.equipment = equip;
                }
            }
        }


        yield return base.DestroyElement();
    }
}
