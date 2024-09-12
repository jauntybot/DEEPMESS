using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class HelpExplaination : MonoBehaviour {

    [SerializeField] GameObject gifContainer;
    [SerializeField] List<Animator> gifAnims;
    [SerializeField] Image image;
    [SerializeField] TMP_Text titleTMP, bodyTMP;
    [SerializeField] Color keyColor;

    public void UpdateExplaination(string title, Sprite _image) {
        titleTMP.text = title;
        bodyTMP.text =  GetExplaination(title);
        gifContainer.SetActive(false);
        if (_image != null) {
            image.gameObject.SetActive(true);
            image.sprite = _image;
        } else image.gameObject.SetActive(false);
    }

    public void UpdateExplaination(string title, List<RuntimeAnimatorController> anims) {
        titleTMP.text = title;
        bodyTMP.text = GetExplaination(title);
        image.gameObject.SetActive(false);
        gifContainer.SetActive(true);
        gifAnims[0].runtimeAnimatorController = anims[0];
        gifAnims[0].transform.parent.gameObject.SetActive(true);
        for (int i = 1; i <= gifAnims.Count - 1; i++) {
            if (anims.Count - 1 < i)
                gifAnims[i].transform.parent.gameObject.SetActive(false);
            else {
                gifAnims[i].runtimeAnimatorController = anims[i];
                gifAnims[i].transform.parent.gameObject.SetActive(true);
            }
        }
    }

    string GetExplaination(string title) {
        string str = "";
        switch (title) {
            default: break;
            case "OVERVIEW":
                str = 
                "DEEPMESS is a turn-based micro-tactics game. Control 3 Slags and use the Hammer to strike the Nail and descend through god's head. Each Slag has unique Gear, use it and the Peek button to strategize your descents. If all 3 Slags or the Nail lose all HP you lose. Power up at Beacons and with on Floor rewards. God's anatomy rebuilds itself after every attempt."
                + "<b>" +ColorToRichText("") + "</b>";
            break;
            case "DESCENDING":
                str = 
                "Descending is your goal, and is often more important than killing enemies. Trigger a descent by striking the Nail with the Hammer. During a descent, Slags and enemies fall directly onto the floor below. Units that land on an occupied tile take 1 damage, and anything beneath a descending unit is destroyed. Use the Peek button in the bottom right to line units up before descending.";
            break;

            case "PLAYER":
                str = 
                "Select a Slag on the board to display its actions in the bottom left. Selecting a Slag or its action displays its range on the floor. In the bottom right there are buttons to Peek at the next floor, end your turn, and undo your Slag's movement.";
            break;
            case "SLAGS":
                str = 
                "Slags can move and perform an action on their turn - either using Gear or the Hammer. If a Slag is reduced to 0 HP it can be revived to 1 HP by striking it with the Hammer, or any action that restores HP. Once all Slags have moved and acted, end your turn in the bottom right.";
            break;
            case "NAIL":
                str = 
                "The Nail is a unit that cannot move or take an action. Striking the Nail with the Hammer triggers a descent. The Nail lands on a random tile after descent and destroys anything it lands on. The Nail can only be struck by the Hammer when it is primed. It takes 1 turn after a descent for the Nail to prime itself. The Nail deals damage back to enemies that attack it.";

            break;
            case "HAMMER":
                str = 
                "The Hammer is your primary tool and weapon. It is shared between your Slags. To use the Hammer, throw it in a straight line at a selected target, then select a Slag for the Hammer to bounce to. The Hammer can bounce to any Slag, including the one that threw it. The Hammer can strike and damage enemies, destroy walls, and bounce off of Gear such as the Anvil.";

            break;
            case "GEAR":
                str = 
                "Gear are your Slags' unique actions. Select a Slag and then its Gear in the bottom left to display its range on the floor. Gear can be upgraded with Lotto Cards from the slime bodega, which can be accessed via Beacons.";
            break;
            case "ENEMIES":
                str = 
                "Enemies move and attack on their turn. Basic enemies take damage on descent. Hover over enemies to display their HP and movement range, and select them to learn about them. Display enemies' turn order by hovering in the top right.";
            break;
            case "FLOORS":
                str = 
                "Floors are grouped into Chunks with modifiers and rewards scattered throughout. Blood tiles prevent slags from acting when stopped in. Bile tiles destroy units that fall onto them. Bulb tiles contain Bulbs. Stop on bulb tiles to collect them. Bulbs are consumable items that can be used as free actions.";
            break;
            case "STATUS EFFECTS":
                str = 
                "Gear, Bulbs, and God Thoughts can cause status effects on both Slags and enemies. Units can be stunned, preventing them from moving or taking an action for a turn. Enemies can be weakened, causing them to take extra damage when they are standing on a blood tile.";

            break;
            case "GOD THOUGHTS":
                str = 
                "Elite enemies and Bloated Bulbs contain God Thoughts - powerful items that provide passive effects for the rest of the run. God Thoughts can scrapped for Slime Bux.";

            break;
            case "BEACONS":
                str = 
                "Use the Beacon's action to call up to the slime bodega. There, you can redeem completed tasks, use Lotto Cards to upgrade Gear and purchase membership rewards. Beacons are destroyed after use and can be crushed by descending units and Gear.";
            break;
            case "TASKS":
                str = 
                "Gino at the slime bodega gives tasks to complete. Check on your progress in the top right. You can redeem completed tasks for Slime Bux and re-roll uncompleted tasks at Beacons. Slime Bux can be used to purchase membership rewards.";
            break;
            case "LOTTO CARDS":
                str = 
                "Every Beacon grants 1 Lotto Card. Scratch off the rewards to reveal a unique upgrade for each of your Slags' Gear. Each Slag has 3 slots for upgrades. Select an upgrade from the Lotto Card, and click and hold over a slot to fill it or replace the previous upgrade.";
            break;
        }
        
        return str;
    }

    string ColorToRichText(string str) {
        return "<color=#" + ColorUtility.ToHtmlStringRGB(keyColor) + ">" + str + "</color>";
    }

}
