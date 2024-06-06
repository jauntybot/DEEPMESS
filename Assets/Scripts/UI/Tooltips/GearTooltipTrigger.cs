using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteInEditMode]
public class GearTooltipTrigger : TooltipTrigger {

    [HideInInspector] public GearData equip;
    [SerializeField] bool initSelf;
    [SerializeField] RuntimeAnimatorController hammerAnim, anvilAnim, bigGrabAnim, shieldAnim, bulbAnim;
    [SerializeField] RuntimeAnimatorController thornsAnim, strikeAnim, autoBombAnim, pinCoushinAnim, quakeAnim;

    void Start() {
        if (initSelf) Initialize(GetComponent<EquipmentButton>().data);
    }

    public override void OnPointerEnter(PointerEventData eventData) {
        base.OnPointerEnter(eventData);
    }

    public void Initialize(GearData data) {
        equip = data;
        string name = equip.name;
        switch (name) {
            case "Anvil": Anvil(); break;
            case "Shield": Shield(); break;
            case "Big Grab": BigGrab(); break;
            case "Hammer": Hammer(); break;
            case "Thorns": Thorns(); break;
            case "Heal Bulb": HealBulb(); break;
            case "Stun Bulb": StunBulb(); break;
            case "Surge Bulb": SurgeBulb(); break;
            case "Strike+": StrikePlus(); break;
            case "Strike": Strike(); break;
            case "Pin Coushin": PinCoushin(); break;
            case "Auto-Bomb": AutoBomb(); break;
            case "Quake": Quake(); break;
            case "Beacon Signal": Beacon(); break;
        }
    }

    public void Initialize(string name) {
        switch (name) {
            case "Anvil": Anvil(); break;
            case "Shield": Shield(); break;
            case "Big Grab": BigGrab(); break;
            case "Hammer": Hammer(); break;
            case "Thorns": Thorns(); break;
            case "Heal Bulb": HealBulb(); break;
            case "Stun Bulb": StunBulb(); break;
            case "Surge Bulb": SurgeBulb(); break;
            case "Strike+": StrikePlus(); break;
            case "Strike": Strike(); break;
            case "Pin Coushin": PinCoushin(); break;
            case "Auto-Bomb": AutoBomb(); break;
            case "Quake": Quake(); break;
        }
    }



    void Anvil() {
        header = "Anvil";
        content = "Drop an anvil and move away. Anvil attracts enemies and descends with units.";
        anim = anvilAnim;
    }


    void Shield() {
        header = "Shield";
        content = "Shield a Slag, protects from 1 attack.";
        anim = shieldAnim;
    }


    void BigGrab() {
        header = "Big Grab";
        content = "Grab and throw an enemy, stunning it for 1 turn.";
        anim = bigGrabAnim;
    }

    void Hammer() {
        header = "Hammer";
        content = "Throw in a straight line, then arc a ricochet to any Slag.";
        anim = hammerAnim;
    }

    void Thorns() {
        header = "Thorns";
        content = "Deals 1 damage to attacking enemies.";
        anim = thornsAnim;
    }

    void HealBulb() {
        header = "Heal Bulb";
        content = "Restores 2HP.";
        anim = bulbAnim;
    }

    void StunBulb() {
        header = "Stun Bulb";
        content = "Explodes in area of effect, stunning anything inside.";
        anim = bulbAnim;
    }
    void SurgeBulb() {
        header = "Surge Bulb";
        content = "Refreshes Slag action and move.";
        anim = bulbAnim;
    }

// ENEMY EQUIPMENT
    
    void Strike() {
        header = "Strike";
        content = "Attacks adjacent target for 1 damage.";
        anim = strikeAnim;
    }

    void StrikePlus() {
        header = "Strike+";
        content = "Attacks adjacent target for 2 damage.";
        anim = strikeAnim;
    }

    void AutoBomb() {
        header = "Auto-Bomb";
        content = "Takes 1 turn to prime. Explodes next turn or on death when primed. All surrounding tiles take 1 damage.";
        anim = autoBombAnim;
    }

    void PinCoushin() {
        header = "Pin Coushin";
        content = "Shoots pins in 4 directions for 1 damage. Cannot be moved.";
        anim = pinCoushinAnim;
    }

    void Quake() {
        header = "Quake";
        content = "Attacks all surrounding tiles for 1 damage causing a cascade.";
        anim = quakeAnim;
    }

    void Beacon() {
        header = "Call Home";
        content = "One time use. Call home to complete objectives and claim rewards. Beacon destroys self after.";
    }
}
