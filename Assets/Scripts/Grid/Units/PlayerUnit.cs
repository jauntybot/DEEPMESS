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

    public int hammerUses, equipUses, bulbPickups;


    public override void UpdateElement(Vector2 coord) {
        base.UpdateElement(coord);
        if (pManager.overrideEquipment)
            transform.position += new Vector3(0, FloorManager.instance.floorOffset, 0);

    }

// Called when an action is applied to a unit or to clear it's actions
    public override void UpdateAction(EquipmentData equipment = null, int mod = 0) 
    {
        if (pManager.overrideEquipment == null) {
            base.UpdateAction(equipment, mod);
            pManager.EquipmentSelected(equipment);
        }
        else {
            base.UpdateAction(pManager.overrideEquipment, mod);
            pManager.EquipmentSelected(pManager.overrideEquipment);
        }
    }

    public override IEnumerator ExecuteAction(GridElement target = null) {
        
        EquipmentData equip = selectedEquipment;
        Coroutine co = StartCoroutine(base.ExecuteAction(target));

// Input parsing - what kind of equipment is being used 
// single target equipment, not movement
        if (equip) {
// Tally for end of run scoring
            if (equip is SlagEquipmentData && equip is not HammerData) equipUses++;
            else if (equip is HammerData) hammerUses++; 

            if (!equip.multiselect && equip is not MoveData) {
                pManager.DeselectUnit();
                pManager.StartCoroutine(pManager.UnitIsActing());
            }
// movement equipment
            else if (equip is MoveData && energyCurrent > 0) {
                pManager.StartCoroutine(pManager.UnitIsActing());
                ui.ToggleEquipmentButtons();
            }
// multi-target equipment - first target selected, update grid contextuals
            else if (equip.firstTarget != null) {
// Riccochet hammer check
                if ((equip is not HammerData d) || d.upgrades[SlagEquipmentData.UpgradePath.Power] == 0 || (d.upgrades[SlagEquipmentData.UpgradePath.Power] == 1 && d.secondTarget != null)) {
                    GridElement anim = equip.contextualAnimGO ? equip.contextualAnimGO.GetComponent<GridElement>() : null;
                    pManager.contextuals.UpdateContext(equip, equip.gridColor, equip.multiContext, anim, target);
                } else if (d.upgrades[SlagEquipmentData.UpgradePath.Power] == 1 && d.secondTarget == null) {
                    GridElement anim = equip.contextualAnimGO.GetComponent<GridElement>();
                    pManager.contextuals.UpdateContext(equip, equip.gridColor, equip.contextDisplay, anim, target);
                }
// multi-target equipment - execute full action
            } else if ((equip is not HammerData d) || (d.upgrades[SlagEquipmentData.UpgradePath.Power] == 1 && d.secondTarget != null)) {
                pManager.DeselectUnit();
                pManager.StartCoroutine(pManager.UnitIsActing());
            }
        } else {
            pManager.DeselectUnit();
            pManager.StartCoroutine(pManager.UnitIsActing());
        }
        UIManager.instance.peekButton.GetComponent<Button>().interactable = false;
// Run base coroutine
        yield return co;
// Harvest Tile Bulb
        if (grid.tiles.Find(sqr => sqr.coord == coord) is TileBulb tb && this is PlayerUnit pu) {
            if (!tb.harvested && pu.equipment.Find(e => e is BulbEquipmentData) == null)
                tb.HarvestBulb(pu);
        }

        UIManager.instance.peekButton.GetComponent<Button>().interactable = true;
        if (equip is MoveData && energyCurrent > 0) {
            grid.UpdateSelectedCursor(true, coord);
        }
        else {
            foreach(GridElement ge in pManager.currentGrid.gridElements) 
                ge.TargetElement(false);
        }

        //manager.unitActing = false;
    }

// Allow the player to click on this
    public override void EnableSelection(bool state)  {
        selectable = state;
        hitbox.enabled = state;
    }

    public override void TargetElement(bool state) {
        base.TargetElement(state);
        if (state && manager.scenario.currentTurn == ScenarioManager.Turn.Player)
            ui.ToggleEquipmentPanel(state);
        else
            ui.ToggleEquipmentPanel(state);
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
    public override IEnumerator DestroySequence(DamageType dmgType) {
        
        
        PlaySound(destroyedSFX);

        bool droppedHammer = false;
        for (int i = equipment.Count - 1; i >= 0; i--) {
            if (equipment[i] is HammerData hammer) {
                if (!droppedHammer) {
                    List<Unit> possiblePasses = new();
                    foreach (Unit u in pManager.units) {
                        if (u is PlayerUnit && !u.conditions.Contains(Status.Disabled) && u != this)
                            possiblePasses.Add(u);
                    }
                    if (possiblePasses.Count > 0) {                
                        StartCoroutine(hammer.LaunchHammer(this, null, possiblePasses[Random.Range(0, possiblePasses.Count)]));   
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

    public override IEnumerator CollideFromBelow(GridElement above) {
        yield return StartCoroutine(TakeDamage(1, DamageType.Melee));
    }

    public override void ApplyCondition(Status s) {
        base.ApplyCondition(s);
        ui.ToggleEquipmentButtons();
        if (manager.scenario.tutorial.isActiveAndEnabled && !manager.scenario.tutorial.bloodEncountered && manager.scenario.floorManager.floorSequence.activePacket.packetType != FloorPacket.PacketType.Tutorial)
            manager.scenario.tutorial.StartCoroutine(manager.scenario.tutorial.BloodTiles());
        
    }

    public virtual void Stabilize() {
        
        SwitchAnim(AnimState.Idle);
        RemoveCondition(Status.Disabled);
        PlayerManager m = (PlayerManager)pManager;
        StartCoroutine(m.nail.TakeDamage(1));
    }

}
