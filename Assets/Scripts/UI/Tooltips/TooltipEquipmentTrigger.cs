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
    [SerializeField] RuntimeAnimatorController thornsAnim, strikeAnim, autoBombAnim, pinCoushinAnim, quakeAnim;

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
            case "THORNS": Thorns(); break;
            case "HEAL BULB": HealBulb(); break;
            case "STUN BULB": StunBulb(); break;
            case "SURGE BULB": SurgeBulb(); break;
            case "STRIKE+": StrikePlus(); break;
            case "STRIKE": Strike(); break;
            case "PIN COUSHIN": PinCoushin(); break;
            case "AUTO-BOMB": AutoBomb(); break;
            case "QUAKE": Quake(); break;
        }
    }

    public void Initialize(string name) {
        switch (name) {
            case "ANVIL": Anvil(); break;
            case "IMMOBILIZE": Immobilize(); break;
            case "BIG WIND": BigWind(); break;
            case "SHIELD": Shield(); break;
            case "SWAP": Swap(); break;
            case "BIG GRAB": BigGrab(); break;
            case "HAMMER": Hammer(); break;
            case "THORNS": Thorns(); break;
            case "HEAL BULB": HealBulb(); break;
            case "STUN BULB": StunBulb(); break;
            case "SURGE BULB": SurgeBulb(); break;
            case "STRIKE+": StrikePlus(); break;
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

    void Thorns() {
        header = "THORNS";
        content = "Deals 1 damage to attacking enemies.";
        anim = thornsAnim;
    }

    void HealBulb() {
        header = "HEAL BULB";
        content = "Restores 2HP.";
        anim = bulbAnim;
    }

    void StunBulb() {
        header = "STUN BULB";
        content = "Explodes in area of effect, stunning anything inside.";
        anim = bulbAnim;
    }
    void SurgeBulb() {
        header = "SURGE BULB";
        content = "Refreshes Slag action and move.";
        anim = bulbAnim;
    }

// ENEMY EQUIPMENT
    
    void Strike() {
        header = "STRIKE";
        content = "Attacks adjacent target for 1 damage.";
        anim = strikeAnim;
    }

    void StrikePlus() {
        header = "STRIKE+";
        content = "Attacks adjacent target for 2 damage.";
        anim = strikeAnim;
    }

    void AutoBomb() {
        header = "AUTO-BOMB";
        content = "Takes one turn to prime. Does not move once primed, explodes on next turn or death. Explodes all surrounding tiles for 2 damage.";
        anim = autoBombAnim;
    }

    void PinCoushin() {
        header = "PIN COUSHIN";
        content = "Shoots pins in 4 directions for 1 damage. Cannot be moved.";
        anim = pinCoushinAnim;
    }

    void Quake() {
        header = "QUAKE";
        content = "Attacks all surrounding tiles for 1 damage causing a cascade.";
        anim = quakeAnim;
    }

}
