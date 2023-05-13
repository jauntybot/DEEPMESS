using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteInEditMode]
public class TooltipEquipmentTrigger : TooltipTrigger, IPointerEnterHandler, IPointerExitHandler
{

    [SerializeField] bool initSelf;

    void Start() {
        if (initSelf) Initialize(GetComponent<EquipmentButton>().data.name);
    }

    public void Initialize(string name)
    {
        switch (name) {
            case "ANVIL": Anvil(); break;
            case "IMMOBILIZE": Immobilize(); break;
            case "BIG WIND": BigWind(); break;
            case "SHIELD": Shield(); break;
            case "SWAP": Swap(); break;
            case "BIG GRAB": BigGrab(); break;
            case "HAMMER": Hammer(); break;
        }
    }

    private void Anvil()
    {
        header = "Anvil";
        content = "Move up to 2 tiles away, leaving an anvil in previous position. Anvils can be targeted by enemies, and descend with your units.";
    }

    private void Immobilize()
    {
        header = "Immobilize";
        content = "Immobilize an enemy unit for the duration of the current floor.";
    }

    private void BigWind()
    {
        header = "Big Wind";
        content = "Push all enemies 1 tile in chosen cardinal direction.";
    }

    private void Shield()
    {
        header = "Shield";
        content = "Deploy a shield onto any friendly unit or nail. Defends for 1 damage and is then destroyed.";
    }

    private void Swap()
    {
        header = "Swap";
        content = "Switch positions with any other unit.";
    }

    private void BigGrab()
    {
        header = "Big Grab";
        content = "Grab an adjacent enemy and throw it up to 4 tiles away on an unoccupied tile.";
    }

    private void Hammer()
    {
        header = "Hammer";
        content = "Use to attack enemies, pass to friendlies, or strike the nail";
    }
}
