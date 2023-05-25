using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Nail : Unit
{
    FloorManager floorManager;
    public MoveData nailDrop;

    public enum NailState { Primed, Buried }
    public NailState nailState;

    protected override void Start()
    {
        base.Start();
        if (FloorManager.instance)
            floorManager = FloorManager.instance;
            selectedEquipment = equipment[0];
        gfxAnim = gfx[0].GetComponent<Animator>();
        ToggleNailState(NailState.Primed);
    }


    public virtual void ToggleNailState(NailState toState) {
        switch (toState) {
            default: break;
            case NailState.Primed:
                gfxAnim.SetBool("Primed", true);
            break;
            case NailState.Buried:
                gfxAnim.SetBool("Primed", false);
            break;
        }
        nailState = toState;
        ui.overview.UpdateOverview(hpCurrent);
    }

     public override void UpdateElement(Vector2 c) {
        base.UpdateElement(c);
        if (manager.scenario.currentTurn != ScenarioManager.Turn.Cascade) {
            GridSquare targetSqr = grid.sqrs.Find(sqr => sqr.coord == c);
            if (targetSqr.tileType == GridSquare.TileType.Blood) {
                targetSqr.PlaySound(targetSqr.dmgdSFX);
            } else if (targetSqr.tileType == GridSquare.TileType.Bile) {
                targetSqr.PlaySound(targetSqr.dmgdSFX);
            } else {
                RemoveCondition(Status.Restricted);
            }
        }
    }
}
