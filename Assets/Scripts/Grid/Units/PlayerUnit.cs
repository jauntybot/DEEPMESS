using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Inherited Unit; unit functionality dependent on player input

public class PlayerUnit : Unit {


    public PlayerManager pManager;
    public enum AnimState { Idle, Hammer, Disabled };
    public AnimState prevAnimState, animState;
    
    public override event OnElementUpdate ElementDestroyed;
    public virtual event OnElementUpdate ElementDisabled;
    
    protected override void Start() {
        base.Start();
    }

    public override void UpdateElement(Vector2 coord) {
        base.UpdateElement(coord);
        if (pManager.overrideEquipment)
            transform.position += new Vector3(0, FloorManager.instance.floorOffset, 0);

    }

// Called when an action is applied to a unit or to clear it's actions
    public override void UpdateAction(EquipmentData equipment = null, int mod = 0) 
    {
        if (pManager.overrideEquipment == null) {
            if (equipment is ConsumableEquipmentData && !usedEquip)
                base.UpdateAction(equipment, mod);
            else if (equipment is not ConsumableEquipmentData)
                base.UpdateAction(equipment, mod);
            pManager.EquipmentSelected(equipment);
        }
        else {
            base.UpdateAction(pManager.overrideEquipment, mod);
            pManager.EquipmentSelected(pManager.overrideEquipment);
        }
    }

    public override IEnumerator ExecuteAction(GridElement target = null) {
        
        Coroutine co = StartCoroutine(base.ExecuteAction(target));

        if (selectedEquipment) {
            if (!selectedEquipment.multiselect && selectedEquipment is not MoveData) {
                pManager.DeselectUnit();
                pManager.StartCoroutine(pManager.UnitIsActing());
            }
            else if (selectedEquipment is MoveData && energyCurrent > 0) {
                pManager.StartCoroutine(pManager.UnitIsActing());
            }
            else if (selectedEquipment.firstTarget != null) {
                GridElement anim = selectedEquipment.contextualAnimGO ? selectedEquipment.contextualAnimGO.GetComponent<GridElement>() : null;
                pManager.contextuals.UpdateContext(selectedEquipment.multiContext, selectedEquipment.gridColor, anim, target);
            } else {
                pManager.DeselectUnit();
                pManager.StartCoroutine(pManager.UnitIsActing());
            }
        } else {
            pManager.DeselectUnit();
            pManager.StartCoroutine(pManager.UnitIsActing());
        }
        FloorManager.instance.upButton.GetComponent<Button>().enabled = false; FloorManager.instance.downButton.GetComponent<Button>().enabled = false;
        yield return co;
        FloorManager.instance.upButton.GetComponent<Button>().enabled = true; FloorManager.instance.downButton.GetComponent<Button>().enabled = true;
        if (selectedEquipment is MoveData && energyCurrent > 0)
            grid.UpdateSelectedCursor(true, coord);
        else {
            foreach(GridElement ge in pManager.currentGrid.gridElements) 
                ge.TargetElement(false);
        }

        UIManager.instance.ToggleUndoButton(pManager.undoOrder.Count > 0);
        pManager.unitActing = false;
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
        //ui.ToggleEquipmentPanel(state);
        //if (energyCurrent == 0 || pManager.selectedUnit != this) ui.ToggleEquipmentPanel(false);
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
    public override IEnumerator DestroyElement(DamageType dmgType) {
        
        if (destroyedSFX)
            PlaySound(destroyedSFX.Get());

        bool droppedHammer = false;
        for (int i = equipment.Count - 1; i >= 0; i--) {
            if (equipment[i] is HammerData hammer) {
                if (!droppedHammer) {
                    List<Unit> possiblePasses = new List<Unit>();
                    foreach (Unit u in pManager.units) {
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
        ElementDisabled?.Invoke(this);
        SwitchAnim(AnimState.Disabled);
        ApplyCondition(Status.Disabled);
    }

    public virtual void Stabilize() {
        
        SwitchAnim(AnimState.Idle);
        RemoveCondition(Status.Disabled);
        PlayerManager m = (PlayerManager)pManager;
        StartCoroutine(m.nail.TakeDamage(1));
    }
}
