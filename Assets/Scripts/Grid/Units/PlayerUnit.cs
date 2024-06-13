using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

// Inherited Unit; unit functionality dependent on player input

public class PlayerUnit : Unit {

    public PlayerManager pManager;
    public enum AnimState { Idle, Hammer, Disabled };
    public AnimState prevAnimState, animState;
    
    public override event OnElementUpdate ElementDestroyed;
    public virtual event OnElementUpdate ElementDisabled;

    public int hammerUses, equipUses, bulbPickups, bulbMod;

    protected override void Start() {
        base.Start();
        hammerUses = 0; equipUses = 0; bulbPickups = 0; bulbMod = 0;
    }


    // Called when an action is applied to a unit or to clear it's actions
    public override void UpdateAction(GearData equipment = null, int mod = 0) {
        if (pManager.overrideEquipment == null) {
            base.UpdateAction(equipment, mod);
            pManager.EquipmentSelected(equipment);
        }
        else {
            base.UpdateAction(pManager.overrideEquipment, mod);
            pManager.EquipmentSelected(pManager.overrideEquipment);
        }
    }

    public override bool ValidCommand(Vector2 target, GearData equip) {
        if (equip == null) return false;
        if (validActionCoords.Count == 0) return false;
        if (!validActionCoords.Contains(target)) return false;
        if (energyCurrent < equip.energyCost && equip is not MoveData) return false;
        else if (moved && equip is MoveData && !pManager.overrideEquipment) return false;

        return true;
    }

    public override IEnumerator ExecuteAction(GridElement target = null) {
        GearData equip = selectedEquipment;
        Coroutine co = null;
// Input parsing - what kind of equipment is being used 
// single target equipment, not movement
        if (equip) {
// Tally for end of run scoring
            if (equip is SlagGearData && equip is not HammerData) {
                if (equip is not BigGrabData || equip.firstTarget != null) 
                    equipUses++;
            }
            else if (equip is HammerData && equip.firstTarget != null) hammerUses++; 
// Base equipment, no multitarget
            if (!equip.multiselect) {
                co = StartCoroutine(base.ExecuteAction(target));
                if (equip is not MoveData || energyCurrent <= 0) {
                    pManager.DeselectUnit();
                    pManager.StartCoroutine(pManager.UnitIsActing());
                }
// movement equipment
                else if (energyCurrent > 0) {
                    pManager.StartCoroutine(pManager.UnitIsActing());
                    ui.ToggleEquipmentButtons();
                }
// multi-target equipment
            } else {
// execute full action
                if (equip.firstTarget != null) {
                    //pManager.DeselectUnit();
                    pManager.StartCoroutine(pManager.UnitIsActing());
                    co = StartCoroutine(base.ExecuteAction(target));
// first target selected, update grid contextuals  
                } else {
                    co = StartCoroutine(base.ExecuteAction(target));
                    Animator anim = equip.contextualAnimGO.GetComponentInChildren<Animator>();
                    pManager.contextuals.UpdateContext(equip, equip.gridColor, equip.multiContext, anim, equip is HammerData ? target : this);
                }
            }
        } else {
            pManager.DeselectUnit();
            pManager.StartCoroutine(pManager.UnitIsActing());
        }
// Run base coroutine
        UIManager.instance.LockHUDButtons(true);

        yield return co;

        if (ui.overview)
            ui.overview.UpdateOverview();

        UIManager.instance.LockHUDButtons(false);
        if (equip is MoveData && energyCurrent > 0) {
            grid.UpdateSelectedCursor(true, coord);
            ui.ToggleEquipmentButtons();
        } else {
            foreach(GridElement ge in pManager.currentGrid.gridElements) 
                ge.TargetElement(false);
        }

// Harvest bulb if standing on and used bulb
        if (equip is BulbEquipmentData && grid.tiles.Find(t => t.coord == coord) is TileBulb tb) {
            if (!tb.harvested && equipment.Find(e => e is BulbEquipmentData) == null)
                tb.HarvestBulb(this);
        }

        //manager.unitActing = false;
        //pManager.DeselectUnit();
    }

// Allow the player to click on this
    public override void EnableSelection(bool state)  {
        selectable = state;
        hitbox.enabled = state;
    }

    public override void TargetElement(bool state) {
        base.TargetElement(state);
        if (ui.overview)
            ui.overview.UpdateOverview();
        // if (state && manager.scenario.currentTurn == ScenarioManager.Turn.Player)
        //     ui.ToggleEquipmentPanel(state);
        // else
        //     ui.ToggleEquipmentPanel(state);
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
    public override IEnumerator DestroySequence(DamageType dmgType = DamageType.Unspecified, GridElement source = null, GearData sourceEquip = null) {
        if (!destroyed) 
            destroyed = true;

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
        ObjectiveEventManager.Broadcast(GenerateDestroyEvent(dmgType, source, sourceEquip));
        
        SwitchAnim(AnimState.Disabled);
        ApplyCondition(Status.Disabled);
    }

    public override IEnumerator TakeDamage(int dmg, DamageType dmgType = DamageType.Unspecified, GridElement source = null, GearData sourceEquip = null) {
        if (!destroyed || dmg < 0) {
            yield return base.TakeDamage(dmg, dmgType, source, sourceEquip);
            if (ui.overview)
                ui.overview.UpdateOverview();
            if (Mathf.Sign(dmg) == -1 && conditions.Contains(Status.Disabled)) Stabilize();
            else if (Mathf.Sign(dmg) == 1 && Random.Range(0,3) == 0) pManager.nail.barkBox.Bark(BarkBox.BarkType.SlagHurt);
        }
    }

    public override IEnumerator CollideFromBelow(GridElement above, GridElement source, GearData sourceEquip) {
        yield return StartCoroutine(TakeDamage(1, DamageType.Melee));
    }

    public override IEnumerator CollideFromBelow(GridElement above) {
        yield return StartCoroutine(TakeDamage(1, DamageType.Melee));
    }

    public override void ApplyCondition(Status s, bool undo = false) {
        base.ApplyCondition(s);
        //ui.ToggleEquipmentButtons();
        if (s == Status.Restricted && !manager.scenario.floorManager.tutorial.bloodEncountered && manager.scenario.floorManager.tutorial.isActiveAndEnabled && manager.scenario.floorManager.floorSequence.activePacket.packetType != FloorChunk.PacketType.Tutorial)
            manager.scenario.floorManager.tutorial.StartCoroutine(manager.scenario.floorManager.tutorial.BloodTiles());
        
    }

    public virtual void Stabilize() {
        SwitchAnim(AnimState.Idle);
        RemoveCondition(Status.Disabled);
    }

}
