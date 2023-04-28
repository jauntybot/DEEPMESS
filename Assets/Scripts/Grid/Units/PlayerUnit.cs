using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Inherited Unit; unit functionality dependent on player input

public class PlayerUnit : Unit {


    public enum AnimState { Idle, Hammer, Disabled };
    public AnimState prevAnimState, animState;
    protected Animator gfxAnim;
    public override event OnElementUpdate ElementDestroyed;
    
    protected override void Start() {
        base.Start();
        gfxAnim = gfx[0].GetComponent<Animator>();
    }

// Called when an action is applied to a unit or to clear it's actions
    public override void UpdateAction(EquipmentData equipment = null, int mod = 0) 
    {
        PlayerManager m = (PlayerManager)manager;
        if (m.overrideEquipment == null) {
            if (equipment is ConsumableEquipmentData && !usedEquip)
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
// Untarget every unit
        foreach(GridElement ge in m.currentGrid.gridElements) 
            ge.TargetElement(false);
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
        prevAnimState = animState;
        animState = toState;
        switch (toState) {
            default: gfxAnim.SetBool("Hammer", false); break;
            case AnimState.Idle: 
                if (prevAnimState == AnimState.Hammer)
                    gfxAnim.SetBool("Hammer", false); 
                else if (prevAnimState == AnimState.Disabled)
                    gfxAnim.SetBool("Disabled", false); 
                break;
            case AnimState.Hammer: gfxAnim.SetBool("Hammer", true); break;
            case AnimState.Disabled: gfxAnim.SetBool("Disabled", true); break;
        }
    }

// Override destroy so that player units are disabled instead
    public override IEnumerator DestroyElement() {
        
        AudioManager.PlaySound(destroyed, transform.position);

        bool droppedHammer = false;
        for (int i = equipment.Count - 1; i >= 0; i--) {
            if (equipment[i] is HammerData hammer) {
                if (!droppedHammer) {
                    List<Unit> possiblePasses = new List<Unit>();
                    foreach (Unit u in manager.units) {
                        if (u is not Nail && !u.conditions.Contains(Status.Disabled) && u != this)
                            possiblePasses.Add(u);
                    }
                    if (possiblePasses.Count > 0) {                
                        StartCoroutine(hammer.ThrowHammer(this, null, possiblePasses[Random.Range(0, possiblePasses.Count)]));   
                        droppedHammer = true;
                    }
                }
            }
        }
        yield return null;
        SwitchAnim(AnimState.Disabled);
        ApplyCondition(Status.Disabled);
    }

    public virtual void Stabilize() {
        
        SwitchAnim(AnimState.Idle);
        RemoveCondition(Status.Disabled);
        PlayerManager m = (PlayerManager)manager;
        StartCoroutine(m.nail.TakeDamage(1));
    }
}
