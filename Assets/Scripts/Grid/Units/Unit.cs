using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : GridElement {

    public override event OnElementUpdate ElementDestroyed;

    public UnitManager manager;

    [Header("Unit")]
    [SerializeField] DescentVFX descentVFX;
    public GameObject airTraillVFX;
    public bool selected;
    public GearData selectedEquipment;
    public List<GearData> equipment;
    public bool moved;

    public List<Vector2> validActionCoords;
    public List<Vector2> inRangeCoords;
    
    public enum Status { Normal, Immobilized, Restricted, Disabled, Weakened, Stunned }

    [Header("Modifiers")]
    public List<Status> conditions;
    public UnitConditionVFX conditionDisplay;
    public int moveMod;
    public int attackMod;

    [Header("UNIT UI/UX")]
    public UnitGameUI ui;
    public Sprite portrait;

    [Header("UNIT AUDIO")]
    public SFX landingSFX;

// Functions that will change depending on the class they're inherited from
#region Inherited Functionality

    protected virtual void Awake() {
        if (conditionDisplay) conditionDisplay.Init(this);
    }

    public virtual void Init() {
        audioSource = GetComponent<AudioSource>();
        hitbox = GetComponent<PolygonCollider2D>();
        hitbox.enabled = false;

        hpCurrent = hpMax;
        energyCurrent = energyMax;

// If first serialized GFX has an animator set Unit anim to it 
        if (gfx[0].GetComponent<Animator>()) {
            gfxAnim = gfx[0].GetComponent<Animator>();
            gfxAnim.keepAnimatorStateOnDisable = true;
        }

        elementCanvas = GetComponentInChildren<ElementCanvas>();
        if (elementCanvas) elementCanvas.Initialize(this);

        UnitInit();
        transform.localScale = Vector3.one * FloorManager.sqrSize;
    }

    public override void Init(Grid g, Vector2 c) {
        base.Init(g, c);
        UnitInit();
    }

    void UnitInit() {
// Create UnitGameUI through UIManager before element canvas init
        UIManager.instance.UpdatePortrait(this, false);
        elementCanvas = GetComponentInChildren<ElementCanvas>();
        if (elementCanvas) elementCanvas.Initialize(this);
// Initialize equipment from prefab
        foreach(GearData e in equipment) {
            e.EquipGear(this);
        }
    }

    public virtual void UpdateAction(GearData equipment = null, int mod = 0) {
// Clear data
        validActionCoords = null;
        grid.DisableGridHighlight();
        if (selectedEquipment)
            selectedEquipment.UntargetEquipment(this);
// Assign new data if provided
        selectedEquipment = equipment;
        if (selectedEquipment) {
            validActionCoords = selectedEquipment.TargetEquipment(this, mod);
        }
    }

    public virtual IEnumerator ExecuteAction(GridElement target = null) {
        if (selectedEquipment) {
            yield return StartCoroutine(selectedEquipment.UseGear(this, target));
        }
        if (selectedEquipment && !selectedEquipment.multiselect) {
            selectedEquipment.UntargetEquipment(this);
            selectedEquipment = null;
        }
        manager.unitActing = false;
    }

    public virtual bool ValidCommand(Vector2 target, GearData equip) {
        if (equip == null) return false;
        if (validActionCoords.Count == 0) return false;
        if (!validActionCoords.Contains(target)) return false;
        if (energyCurrent < equip.energyCost && equip is not MoveData) return false;
        else if (moved && equip is MoveData) return false;

        return true;
    }

    public override void UpdateElement(Vector2 c) {
        Vector2 prevCoord = coord;
        base.UpdateElement(c);
        if (manager.scenario.currentTurn != ScenarioManager.Turn.Cascade) {
            Tile targetSqr = grid.tiles.Find(sqr => sqr.coord == c);
            if (targetSqr.tileType == Tile.TileType.Blood) {
                if (prevCoord != new Vector2(-32, -32) && prevCoord != c)
                    targetSqr.PlaySound(targetSqr.dmgdSFX);
// SHIELD UNIT TIER II -- Blood bouyancy
                if (!conditions.Contains(Status.Disabled) && !(shield && shield.buoyant))
                    ApplyCondition(Status.Restricted);
            } else if (targetSqr.tileType == Tile.TileType.Bile && hpCurrent > 0) {
// SHIELD UNIT TIER II -- Bile bouyancy
                targetSqr.PlaySound(targetSqr.dmgdSFX);
                if (!destroyed && hpCurrent > 0 && this is not BossUnit) {
                    RemoveShield();
                    StartCoroutine(TakeDamage(hpMax, DamageType.Bile));
                }
            } else if (targetSqr is TileBulb) {
// Harvest Tile Bulb
                if (this is PlayerUnit pu && grid.tiles.Find(sqr => sqr.coord == coord) is TileBulb tb && pu.pManager.overrideEquipment == null) {
                    if (!tb.harvested && equipment.Find(e => e is BulbEquipmentData) == null)
                        tb.HarvestBulb(pu);
                }
            } 
            if (targetSqr.tileType != Tile.TileType.Blood && conditions.Contains(Status.Restricted)) {
                RemoveCondition(Status.Restricted);
            }
        }
    }

// For when a Slag is acting on a Unit to move it, such as BigGrab or any push mechanics
    public virtual void UpdateElement(Vector2 c, GridElement source, GearData sourceEquip = null) {
        Vector2 prevCoord = coord;
        base.UpdateElement(c);
        if (manager.scenario.currentTurn != ScenarioManager.Turn.Cascade) {
            Tile targetSqr = grid.tiles.Find(sqr => sqr.coord == c);
            if (targetSqr.tileType == Tile.TileType.Blood) {
                if (prevCoord != new Vector2(-32, -32) && prevCoord != c)
                    targetSqr.PlaySound(targetSqr.dmgdSFX);
// SHIELD UNIT TIER II -- Blood bouyancy
                if (!conditions.Contains(Status.Disabled) && !(shield && shield.buoyant))
                    ApplyCondition(Status.Restricted, sourceEquip && sourceEquip is CascadeMoveData);
            } else if (targetSqr.tileType == Tile.TileType.Bile && hpCurrent > 0) {
// SHIELD UNIT TIER II -- Bile bouyancy
                targetSqr.PlaySound(targetSqr.dmgdSFX);
                if (!(shield && shield.buoyant)) {
                    RemoveShield();
                    StartCoroutine(TakeDamage(hpMax, DamageType.Bile, source, sourceEquip));
                }
            } else if (targetSqr is TileBulb) {
// Harvest Tile Bulb
                if (this is PlayerUnit pu && grid.tiles.Find(sqr => sqr.coord == coord) is TileBulb tb && pu.pManager.overrideEquipment == null) {
                    if (!tb.harvested && equipment.Find(e => e is BulbEquipmentData) == null)
                        tb.HarvestBulb(pu);
                }
            } 
            if (targetSqr.tileType != Tile.TileType.Blood && conditions.Contains(Status.Restricted)) {
                RemoveCondition(Status.Restricted, sourceEquip && sourceEquip is CascadeMoveData);
            }
        }
    }

    public override IEnumerator TakeDamage(int dmg, DamageType dmgType = DamageType.Unspecified, GridElement source = null, GearData sourceEquip = null) {
        if (!destroyed) {
            TargetElement(true);

            int modifiedDmg = conditions.Contains(Status.Weakened) ? dmg + 1 : dmg;
            yield return base.TakeDamage(modifiedDmg, dmgType, source, sourceEquip);

            TargetElement(targeted);
        }
    }

    public override IEnumerator DestroySequence(DamageType dmgType = DamageType.Unspecified, GridElement source = null, GearData sourceEquip = null) {
        if (!destroyed) 
            destroyed = true;

        ElementDestroyed?.Invoke(this);
        ObjectiveEventManager.Broadcast(GenerateDestroyEvent(dmgType, source, sourceEquip));        
        
        PlaySound(destroyedSFX);
        float timer = 0f;
        while (timer < 0.5f) {
            yield return null;
            timer += Time.deltaTime;
        }

        if (manager.selectedUnit == this) manager.DeselectUnit();

        if (gameObject != null)
            Destroy(gameObject);
    }

#endregion


#region Unit Functionality
    
    public void DescentVFX(Tile sqr, GridElement subGE = null) {
        descentVFX.gameObject.SetActive(true);

        if (subGE) {
            if (subGE is Wall)
                descentVFX.SetColor(4);
            else if (subGE is EnemyUnit)
                descentVFX.SetColor(3);
            else
                descentVFX.SetColor(0);
        } else {
            if (sqr.tileType == Tile.TileType.Blood)
                descentVFX.SetColor(2);
            else if (sqr.tileType == Tile.TileType.Bile)
                descentVFX.SetColor(1);
            else
                descentVFX.SetColor(0);
        }

    }

    public virtual IEnumerator CollideFromAbove(GridElement subGE, int hardLand = 0) {
// Tutorial tooltip popup
        if (manager.scenario.floorManager.tutorial.isActiveAndEnabled && !manager.scenario.floorManager.tutorial.collisionEncountered && manager.scenario.floorManager.floorSequence.activePacket.packetType != FloorChunk.PacketType.Tutorial)
            manager.scenario.floorManager.tutorial.StartCoroutine(manager.scenario.floorManager.tutorial.DescentDamage());
        
        if (subGE is PlayerUnit)
            yield return StartCoroutine(DestroySequence());
        else
            yield return StartCoroutine(TakeDamage(1 + hardLand, DamageType.Fall, subGE));
    }

// For when a Slag is acting on a Unit to move it, such as BigGrab or any push mechanics
    public virtual IEnumerator CollideFromAbove(GridElement subGE, int hardLand = 0, GridElement source = null, GearData sourceEquip = null) {
// Tutorial tooltip popup
        if (manager.scenario.floorManager.tutorial.isActiveAndEnabled && !manager.scenario.floorManager.tutorial.collisionEncountered && manager.scenario.floorManager.floorSequence.activePacket.packetType != FloorChunk.PacketType.Tutorial)
            manager.scenario.floorManager.tutorial.StartCoroutine(manager.scenario.floorManager.tutorial.DescentDamage());
        
        if (subGE is PlayerUnit)
            yield return StartCoroutine(DestroySequence());
        else
            yield return StartCoroutine(TakeDamage(1 + hardLand, DamageType.Fall, source, sourceEquip));
    }

    
    public override void RemoveShield() {
        if (shield) {
            Shield temp = shield;
            shield = null;
            if (temp.aerodynamics)
                moveMod--;
// SHIELD UNIT TIER I - Remove buoyancy
            if (temp.buoyant)
                UpdateElement(coord);
// SHIELD UNIT TIER II - Heal unit on breaking
            if (temp.healing) {
                StartCoroutine(TakeDamage(-1));
            }
            temp.DestroySelf();
        }
    }


    public virtual void ApplyCondition(Status s, bool undo = false) {
        UnitConditionEvent evt = ObjectiveEvents.UnitConditionEvent;
        evt.undo = false;
        evt.apply = true;
        evt.condition = s;
        evt.target = this;
        ObjectiveEventManager.Broadcast(evt);

        if (!conditions.Contains(s)) {
            conditions.Add(s);
            conditionDisplay.UpdateCondition(s);


            switch(s) {
                default: return;
                case Status.Normal: return;
                case Status.Restricted:
                    if (this is PlayerUnit)
                        ui.UpdateEquipmentButtons();
                break;
                case Status.Immobilized:
                    moved = true;
                break;
                case Status.Disabled:
                    energyCurrent = 0;
                    moved = true;
                    if (this is PlayerUnit)
                        ui.UpdateEquipmentButtons();
                    elementCanvas.UpdateStatsDisplay();
                    if (conditions.Contains(Status.Stunned)) 
                        RemoveCondition(Status.Stunned);
                break;
                case Status.Weakened:
                    //elementCanvas.UpdateStatsDisplay();
                break;
                case Status.Stunned:
                    moved = true; energyCurrent = 0;
                    if (this is PlayerUnit)
                        ui.UpdateEquipmentButtons();
                break;
            }
        }
    }


    
    public virtual void RemoveCondition(Status s, bool undo = false) {
        UnitConditionEvent evt = ObjectiveEvents.UnitConditionEvent;
        evt.undo = undo;
        evt.condition = s;
        evt.apply = false;
        evt.target = this;
        ObjectiveEventManager.Broadcast(evt);

        if (conditions.Contains(s)) {
            if (conditionDisplay) conditionDisplay.RemoveCondition(s);
            conditions.Remove(s);
            switch(s) {
                default: return;
                case Status.Normal: return;
                case Status.Restricted:
                    if (this is PlayerUnit)
                        ui.UpdateEquipmentButtons();
                break;
                case Status.Disabled:
                    destroyed = false;
                    hpCurrent = 0;
                    StartCoroutine(TakeDamage(-manager.reviveTo));
                    if (manager.reviveTo == 1) {
                        moved = false;
                        energyCurrent = 1;
                    } else {
                        ApplyCondition(Status.Stunned);
                    }
                break;
                case Status.Weakened:
                break;
            }
        }
    }

    public void SelectUnitButton() {
        manager.SelectUnit(this);
    }

#endregion

}