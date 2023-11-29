using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : GridElement {

    public override event OnElementUpdate ElementDestroyed;

    [Header("Unit")]
    [HideInInspector] public UnitManager manager;
    protected Animator gfxAnim;
    [SerializeField] DescentVFX descentVFX;
    public GameObject airTraillVFX;
    public bool selected;
    public EquipmentData selectedEquipment;
    public List<EquipmentData> equipment;
    public bool moved;

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
    public GameUnitUI ui;
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

        if (ui.overview)
            ui.overview.UpdateOverview(hpCurrent);

// Initialize equipment from prefab
        foreach(EquipmentData e in equipment) {
            e.EquipEquipment(this);
        }
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
        manager.unitActing = false;
    }

    public virtual bool ValidCommand(Vector2 target, EquipmentData equip) {
        if (equip == null) return false;
        if (validActionCoords.Count == 0) return false;
        if (!validActionCoords.Contains(target)) return false;
        if (energyCurrent < equip.energyCost && equip is not MoveData) return false;
        else if (moved && equip is MoveData) return false;

        return true;
    }

    public override void UpdateElement(Vector2 c) {
        base.UpdateElement(c);
        if (manager.scenario.currentTurn != ScenarioManager.Turn.Cascade) {
            Tile targetSqr = grid.tiles.Find(sqr => sqr.coord == c);
            if (targetSqr.tileType == Tile.TileType.Blood) {
                targetSqr.PlaySound(targetSqr.dmgdSFX);
                ApplyCondition(Status.Restricted);
            } else if (targetSqr.tileType == Tile.TileType.Bile && hpCurrent > 0) {
                targetSqr.PlaySound(targetSqr.dmgdSFX);
                RemoveShield();
                StartCoroutine(TakeDamage(hpMax, DamageType.Bile));
            } else if (targetSqr is TileBulb tb && this is PlayerUnit pu) {
                if (!tb.harvested && pu.equipment.Find(e => e is BulbEquipmentData) == null)
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

    public override IEnumerator DestroySequence(DamageType dmgType = DamageType.Unspecified) {
        ElementDestroyed?.Invoke(this);
        
        PlaySound(destroyedSFX);
        yield return new WaitForSecondsRealtime(0.5f);

        //gfxAnim.SetBool("Destoyed", true);

        if (manager.selectedUnit == this) manager.DeselectUnit();

        enabled = false;
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

    public virtual IEnumerator CollideFromAbove(GridElement subGE) {
        if (manager.scenario.tutorial.isActiveAndEnabled && !manager.scenario.tutorial.collisionEncountered && manager.scenario.floorManager.floorSequence.activePacket.packetType != FloorPacket.PacketType.Tutorial)
            manager.scenario.tutorial.StartCoroutine(manager.scenario.tutorial.DescentDamage());
        
        if (subGE is PlayerUnit)
            yield return StartCoroutine(DestroySequence());
        else
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