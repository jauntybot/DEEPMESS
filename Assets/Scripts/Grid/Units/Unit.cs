using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : GridElement {

    public override event OnElementUpdate ElementDestroyed;

    [Header("Unit")]
    protected Animator gfxAnim;
    [SerializeField] DescentVFX descentVFX;
    public UnitManager manager;
    public bool selected;
    public EquipmentData selectedEquipment;
    public List<EquipmentData> equipment;
    public bool moved, usedEquip;

    public List<Vector2> validActionCoords;
    public List<Vector2> inRangeCoords;
    
    public enum Status { Normal, Immobilized, Restricted, Disabled, Weakened, Stunned }

    [Header("Modifiers")]
    public List<Status> conditions;
    public UnitConditionVFX conditionDisplay;
    private int prevMod;
    public int moveMod;
    public int attackMod;

    [Header("UNIT UI/UX")]
    public UnitUI ui;
    public DescentPreview descentPreview;
    public Sprite portrait;
    
    [SerializeField] float animDur = 1f;

    [Header("UNIT AUDIO")]
    public SFX landingSFX;

// Functions that will change depending on the class they're inherited from
#region Inherited Functionality

    protected override void Start() {
        base.Start();
// If first serialized GFX has an animator set Unit anim to it 
        if (gfx[0].GetComponent<Animator>()) {
            gfxAnim = gfx[0].GetComponent<Animator>();
            gfxAnim.keepAnimatorStateOnDisable = true;
        }
        if (conditionDisplay) conditionDisplay.Init(this);
    }

    public virtual void UpdateAction(EquipmentData equipment = null, int mod = 0) {
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
            yield return StartCoroutine(selectedEquipment.UseEquipment(this, target));
        }
        if (selectedEquipment && !selectedEquipment.multiselect) {
            selectedEquipment.UntargetEquipment(this);
            selectedEquipment = null;
        }
        if (grid.sqrs.Find(sqr => sqr.coord == coord) is TileBulb tb && this is PlayerUnit pu) {
                if (!tb.harvested && pu.ui.bulb == null)
                    tb.HarvestBulb(pu);
        }
    }

    public bool ValidCommand(Vector2 target, EquipmentData equip) {
        if (equip == null) return false;
        if (validActionCoords.Count == 0) return false;
        if (!validActionCoords.Contains(target)) return false;
        if (energyCurrent < equip.energyCost && equip is not MoveData) return false;
        else if (moved && equip is MoveData) return false;
        else if (usedEquip && (equip is PerFloorEquipmentData && equip is not HammerData)) return false;

        return true;
    }

    public override void UpdateElement(Vector2 c) {
        base.UpdateElement(c);
        if (manager.scenario.currentTurn != ScenarioManager.Turn.Cascade) {
            Tile targetSqr = grid.sqrs.Find(sqr => sqr.coord == c);
            if (targetSqr.tileType == Tile.TileType.Blood) {
                targetSqr.PlaySound(targetSqr.dmgdSFX);
                ApplyCondition(Status.Restricted);
            } else if (targetSqr.tileType == Tile.TileType.Bile && hpCurrent > 0) {
                targetSqr.PlaySound(targetSqr.dmgdSFX);
                RemoveShell();
                StartCoroutine(TakeDamage(hpMax, DamageType.Bile));
            } else if (targetSqr is TileBulb tb && this is PlayerUnit pu) {
                if (!tb.harvested && pu.ui.bulb == null)
                    tb.HarvestBulb(pu);
            } else {
                RemoveCondition(Status.Restricted);
            }
        }
    }

    public override IEnumerator TakeDamage(int dmg, DamageType dmgType = DamageType.Unspecified, GridElement source = null) {

        bool prevTargeted = targeted;
        TargetElement(true);

        int modifiedDmg = conditions.Contains(Status.Weakened) ? dmg * 2 : dmg;
        yield return base.TakeDamage(modifiedDmg, dmgType, source);

        TargetElement(targeted);
    }

    public override IEnumerator DestroyElement(DamageType dmgType = DamageType.Unspecified) {
        ElementDestroyed?.Invoke(this);
        
        PlaySound(destroyedSFX);
        yield return new WaitForSecondsRealtime(0.5f);

        //gfxAnim.SetBool("Destoyed", true);
        
        if (this.gameObject != null)
            Destroy(this.gameObject);
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

    public virtual IEnumerator CollideFromAbove(GridElement subGE) {
        if (subGE is not GroundElement)
            yield return StartCoroutine(TakeDamage(1, DamageType.Melee));
    }

    public virtual void ApplyCondition(Status s) {
        if (!conditions.Contains(s)) {
            conditions.Add(s);
            conditionDisplay.UpdateCondition(s);
            switch(s) {
                default: return;
                case Status.Normal: return;
                case Status.Immobilized:
                    moved = true;
                break;
                case Status.Disabled:
                    energyCurrent = 0;
                    moved = true;
                    ui.UpdateEquipmentButtons();
                    elementCanvas.UpdateStatsDisplay();
                break;
                case Status.Weakened:
                    //elementCanvas.UpdateStatsDisplay();
                break;
            }
        }
    }

    public virtual void RemoveCondition(Status s) {
        if (conditions.Contains(s)) {
            conditionDisplay.RemoveCondition(s);
            conditions.Remove(s);
            switch(s) {
                default: return;
                case Status.Normal: return;
                case Status.Immobilized:
                    moved = false;
                    foreach(GridElement ge in grid.CoordContents(coord)) {
                        if (ge is ImmobilizeGoo goo) {
                            Destroy(goo.gameObject);
                        }
                    }
                break;
                case Status.Restricted:
                    ui.ToggleEquipmentButtons();
                break;
                case Status.Disabled:
                    hpCurrent = 0;
                    StartCoroutine(TakeDamage(-1));
                    moved = false;
                    energyCurrent = 1;
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