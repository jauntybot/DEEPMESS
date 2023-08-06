using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteInEditMode]
public class TooltipEquipmentTrigger : TooltipTrigger
{

    [SerializeField] bool initSelf;
    [SerializeField] RuntimeAnimatorController anvilAnim, bigGrabAnim, shieldAnim, bulbAnim;

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
            case "HEAL BULB": HealBulb(); break;
            case "WEAKEN BULB": WeakenBulb(); break;
            case "SURGE BULB": SurgeBulb(); break;
        }
    }

    private void Anvil()
    {
        header = "Anvil";
        content = "Drop an anvil and move away. Anvil attracts enemies.";
        anim = anvilAnim;
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
        content = "Erect a shield around any unit, protecting it from damage.";
        anim = shieldAnim;
    }

    private void Swap()
    {
        header = "Swap";
        content = "Switch positions with any other unit.";
    }

    private void BigGrab()
    {
        header = "Big Grab";
        content = "Grab and throw an enemy.";
        anim = bigGrabAnim;
    }

    private void Hammer()
    {
        header = "Hammer";
        content = "Use to attack enemies, pass to friendlies, or strike the nail";
    }

    private void HealBulb()
    {
        header = "Heal Bulb";
        content = "Restores 2HP. Can be thrown.";
        anim = bulbAnim;
    }

    private void WeakenBulb()
    {
        header = "Weaken Bulb";
        content = "Throw for area of effect explosion. Causes anything in radius to take double damage for that turn.";
        anim = bulbAnim;
    }
    private void SurgeBulb()
    {
        header = "Surge Bulb";
        content = "Refreshes action and move. Can be thrown.";
        anim = bulbAnim;
    }
}
