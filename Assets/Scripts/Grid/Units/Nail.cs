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

    protected override void Start()
    {
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
            break;
            case NailState.Hiding:
            case NailState.Buried:
                gfxAnim.SetBool("Falling", false);
                gfxAnim.SetBool("Primed", false);
            break;
        }
        nailState = toState;
        ui.overview.UpdateOverview(hpCurrent);
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
}
