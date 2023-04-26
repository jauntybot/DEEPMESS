using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Nail : Unit
{
    FloorManager floorManager;
    public MoveData nailDrop;

    public enum NailState { Primed, Buried }
    public NailState nailState;
    [SerializeField] Animator gfxAnim;

    protected override void Start()
    {
        base.Start();
        if (FloorManager.instance)
            floorManager = FloorManager.instance;
            selectedEquipment = equipment[0];
        gfxAnim = gfx[0].GetComponent<Animator>();
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
    }

    
}
