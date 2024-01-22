using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteInEditMode]
public class TooltipEquipmentTrigger : TooltipTrigger {

    [HideInInspector] public EquipmentData equip;
    [SerializeField] bool initSelf;
    [SerializeField] RuntimeAnimatorController hammerAnim, anvilAnim, bigGrabAnim, shieldAnim, bulbAnim;

    void Start() {
        if (initSelf) Initialize(GetComponent<EquipmentButton>().data);
    }

    public override void OnPointerEnter(PointerEventData eventData) {
        base.OnPointerEnter(eventData);
    }

    public void Initialize(EquipmentData data) {
        equip = data;
        string name = equip.name;
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
            case "STRIKE": Strike(); break;
            case "PIN COUSHIN": PinCoushin(); break;
            case "AUTO-BOMB": AutoBomb(); break;
            case "QUAKE": Quake(); break;
        }
    }

    void Anvil() {
        header = "ANVIL";
        content = "Drop an anvil and move away. Anvil attracts enemies and descends with units.";
        anim = anvilAnim;
    }

    void Immobilize() {
        header = "Immobilize";
        content = "Immobilize an enemy unit for the duration of the current floor.";
    }

    void BigWind() {
        header = "BIG WIND";
        content = "Push all enemies 1 tile in chosen cardinal direction.";
    }

    void Shield() {
        header = "SHIELD";
        content = " Shield any player unit, protecting it from damage once.";
        anim = shieldAnim;
    }

    void Swap() {
        header = "SWAP";
        content = "Switch positions with any other unit.";
    }

    void BigGrab() {
        header = "BIG GRAB";
        content = "Grab and throw an enemy, stunning it for 1 turn.";
        anim = bigGrabAnim;
    }

    void Hammer() {
        header = "HAMMER";
        content = "Throw in a straight line, then arc a ricochet to any Slag.";
        anim = hammerAnim;
    }

    void HealBulb() {
        header = "HEAL BULB";
        content = "Restores 2HP. Can be thrown.";
        anim = bulbAnim;
    }

    void WeakenBulb() {
        header = "WEAKEN BULB";
        content = "Throw for area of effect explosion. Causes anything in radius to take double damage for that turn.";
        anim = bulbAnim;
    }
    void SurgeBulb() {
        header = "SURGE BULB";
        content = "Refreshes action and move. Can be thrown.";
        anim = bulbAnim;
    }

// ENEMY EQUIPMENT
    
    void Strike() {
        header = "STRIKE";
        content = "Attacks adjacent target for 1 damage.";
        anim = null;
    }

    void AutoBomb() {
        header = "AUTO-BOMB";
        content = "After priming, self destructs on death. Explodes all surrounding tiles for 2 damage.";
        anim = null;
    }

    void PinCoushin() {
        header = "PIN COUSHIN";
        content = "Shoots pins in 4 directions for 1 damage. Cannot be grabbed.";
        anim = null;
    }

    void Quake() {
        header = "QUAKE";
        content = "Attacks all surrounding tiles for 1 damage causing a cascade.";
        anim = null;
    }

}
