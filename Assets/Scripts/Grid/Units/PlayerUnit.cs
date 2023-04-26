using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Inherited Unit; unit functionality dependent on player input

public class PlayerUnit : Unit {

    public int consumableCount;

    public enum AnimState { Idle, Hammer };
    public AnimState animState;
    protected Animator gfxAnim;
    
    protected override void Start() {
        base.Start();
        gfxAnim = gfx[0].GetComponent<Animator>();
    }

// Called when an action is applied to a unit or to clear it's actions
    public override void UpdateAction(EquipmentData equipment = null, int mod = 0) 
    {
        PlayerManager m = (PlayerManager)manager;
        if (m.overrideEquipment == null) {
            if (equipment is ConsumableEquipmentData && consumableCount > 0)
                base.UpdateAction(equipment, mod);
            else if (equipment is not ConsumableEquipmentData)
                base.UpdateAction(equipment, mod);
        }
        else {
            base.UpdateAction(m.overrideEquipment, mod);
        }
    }

    public override IEnumerator ExecuteAction(GridElement target = null) {
        PlayerManager m = (PlayerManager)manager;
        m.unitActing = true;
        
        Coroutine co = StartCoroutine(base.ExecuteAction(target));

        if (selectedEquipment) {
            if (!selectedEquipment.multiselect)
                manager.DeselectUnit();
            else if (selectedEquipment.firstTarget == null)
                manager.DeselectUnit();
        } else
            manager.DeselectUnit();
        
        yield return co;

        UIManager.instance.ToggleUndoButton(m.undoOrder.Count > 0);
        m.unitActing = false;
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
        //if (energyCurrent == 0 || manager.selectedUnit != this) ui.ToggleEquipmentPanel(false);
    }

    public virtual void SwitchAnim(AnimState toState) {
        animState = toState;
        switch (toState) {
            default: gfxAnim.SetBool("Hammer", false); break;
            case AnimState.Idle: gfxAnim.SetBool("Hammer", false); break;
            case AnimState.Hammer: gfxAnim.SetBool("Hammer", true); break;
        }
    }

// Override destroy to account for dropping the hammer
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
                    pickup.equipment.Add(equip);
                }
            }
        }


        yield return base.DestroyElement();
    }
}
