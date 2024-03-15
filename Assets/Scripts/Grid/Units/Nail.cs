using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Nail : Unit {
    public override event OnElementUpdate ElementUpdated;

    public MoveData nailDrop;

    public enum NailState { Falling, Primed, Buried, Hiding }
    public NailState nailState;

    [SerializeField] GameObject primedVFX;
    [SerializeField] SFX primedSFX;

    public override event OnElementUpdate ElementDestroyed;
    public virtual event OnElementUpdate ElementDisabled;

    public BarkBox barkBox;

    protected override void Start() {
        base.Start();
        selectedEquipment = equipment[0];
        gfxAnim = gfx[0].GetComponent<Animator>();
        ToggleNailState(NailState.Falling);
    }


    public virtual void ToggleNailState(NailState toState) {
        switch (toState) {
            default: break;
            case NailState.Falling:
                gfxAnim.SetBool("Falling", true);
                gfxAnim.SetBool("Primed", false);
            break;
            case NailState.Primed:
                gfxAnim.SetBool("Falling", false);
                gfxAnim.SetBool("Primed", true);
                primedVFX.SetActive(true);
                PlaySound(primedSFX);
                if (Random.Range(0, 4) == 0) barkBox.Bark(BarkBox.BarkType.NailPrime);
            break;
            case NailState.Hiding:
            case NailState.Buried:
                gfxAnim.SetBool("Falling", false);
                gfxAnim.SetBool("Primed", false);
            break;
        }
        nailState = toState;
        ui.overview.UpdateOverview();
    }

    public override void UpdateElement(Vector2 c) {
        ElementUpdated?.Invoke(this);
        transform.position = grid.PosFromCoord(c);
        UpdateSortOrder(c);
        coord=c;
        foreach (GridElement ge in grid.CoordContents(c)) {
            if (ge != this)
                ge.OnSharedSpace(this);
        }
        if (manager.scenario.currentTurn != ScenarioManager.Turn.Cascade) {
            Tile targetSqr = grid.tiles.Find(sqr => sqr.coord == c);
            if (targetSqr.tileType == Tile.TileType.Blood) {
                targetSqr.PlaySound(targetSqr.dmgdSFX);
            } else if (targetSqr.tileType == Tile.TileType.Bile) {
                targetSqr.PlaySound(targetSqr.dmgdSFX);
            } else {
                RemoveCondition(Status.Restricted);
            }
        }
    }

    public override IEnumerator TakeDamage(int dmg, DamageType dmgType = DamageType.Unspecified, GridElement source = null, EquipmentData sourceEquip = null) {
        if (!destroyed) {
            yield return base.TakeDamage(dmg, dmgType, source, sourceEquip);
            if (ui.overview)
                ui.overview.UpdateOverview();
                
            if (source is EnemyUnit u && (u.destroyed || u.hpCurrent <= 0)) {
                if (Random.Range(0, 2) == 0) barkBox.Bark(BarkBox.BarkType.NailKill);
            }
            if (hpCurrent <= 2) barkBox.Bark(BarkBox.BarkType.LowHP);
        }
    }

    public override IEnumerator DestroySequence(DamageType dmgType = DamageType.Unspecified, GridElement source = null, EquipmentData sourceEquip = null) {
        if (!destroyed) {
            PlaySound(destroyedSFX);
            
            yield return null;
            
            ElementDisabled?.Invoke(this);
            ObjectiveEventManager.Broadcast(GenerateDestroyEvent(dmgType, source, sourceEquip));

            ApplyCondition(Status.Disabled);
            //gfxAnim.SetBool("Destoyed", true);
        }
    }

    public override IEnumerator CollideFromAbove(GridElement subGE, int hardLand = 0) {
        yield return null;
        if (subGE is EnemyUnit) {
            if (Random.Range(0, 2) == 0)
                barkBox.Bark(BarkBox.BarkType.NailCrush);
        }
    }

    public override IEnumerator CollideFromAbove(GridElement subGE, int hardLand = 0, GridElement source = null, EquipmentData sourceEquip = null) {
        yield return null;
        if (subGE is EnemyUnit) {
            if (Random.Range(0, 2) == 0)
                barkBox.Bark(BarkBox.BarkType.NailCrush);
        }
    }
}
