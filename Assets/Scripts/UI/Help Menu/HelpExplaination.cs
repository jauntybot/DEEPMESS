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
                "DEEPMESS is a turn-based micro-tactics game."
                + '\n' + "Each Slag has unique GEAR, use it and the PEEK button to strategize your descents."
                + '\n' + "Control 3 Slags and use the Hammer to strike the Nail and descend. If all 3 Slags or the Nail lose all HP you lose."
                + '\n' + "God's anatomy will rebuild itself, forcing you to carve a new path through varied terrain."
                + "<b>" +ColorToRichText("") + "</b>";
            break;
            case "DESCENDING":
                str = 
                "Descending is your goal, and is often more important than killing enemies. Trigger a descent by striking the Nail with the Hammer."
                + '\n' + "During a descent, Slags and enemies fall directly onto the floor below, while the Nail lands in a random tile. Units that land on an occupied tile take 1 damage, and anything beneath a descending unit is destroyed."
                + '\n' + "Use the Peek button in the bottom right to line units up before descending."
                + "<b>" +ColorToRichText("") + "</b>";
            break;

            case "PLAYER":
                str = 
                "Select a Slag on the board to display its actions in the bottom left. Selecting a Slag or its action will show its range on the floor."
                + '\n' + "In the bottom right there are buttons to Peek at the next floor, end your turn, and undo your Slags' movement."
                + "<b>" +ColorToRichText("") + "</b>";
            break;
            case "SLAGS":
                str = 
                "Slags can move and perform an action on their turn—either using Gear or the Hammer."
                + '\n' + "If a Slag is reduced to 0 HP it can be revived to 1 HP by striking it with the Hammer, or any action that grants HP."
                + '\n' + "Once all Slags have moved and acted, end your turn in the bottom right."
                + "<b>" +ColorToRichText("") + "</b>";
            break;
            case "NAIL":
                str = 
                "The Nail is a unit that cannot move or take an action. Striking the Nail with the Hammer will trigger a descent."
                + '\n' + "The Nail will land on a random tile on descent and will destroy anything it lands on."
                + '\n' + "The Nail can only be struck by the Hammer when it is primed. It takes 1 turn after a descent for the Nail to prime itself."
                + '\n' + "The Nail deals damage back to enemies that attack it.";

            break;
            case "HAMMER":
                str = 
                "The Hammer is your main tool and weapon. It is shared between your Slags."
                + '\n' + "To use the Hammer, throw it in a straight line at a selected target, then select a Slag for the Hammer to bounce to. The Hammer can bounce to any active Slag, including the one that threw it."
                + '\n' + "The Hammer can strike and damage enemies, destroy walls, and bounce off of Gear such as the Anvil.";

            break;
            case "GEAR":
                str = 
                "Gear are your Slags' unique actions. Select a Slag and then its Gear in the bottom left to display its range on the floor."
                + '\n' + "Gear can be upgraded with Lotto Cards from the slime bodega, which can be accessed via Beacons.";
            break;
            case "ENEMIES":
                str = 
                "Enemies will move and attack on their turn. After a descent, they will scatter but not attack."
                + '\n' + "Hover over enemies to display their HP and movement range, and select them to learn about their actions. Display enemies' turn order by hovering in the top right."
                + "<b>" +ColorToRichText("") + "</b>";
            break;
            case "FLOORS":
                str = 
                "Floors are grouped into Chunks with modifiers and rewards scattered throughout."
                + '\n' + "Blood tiles prevent slags from acting when stopped in."
                + '\n' + "Bile tiles destroy units that fall onto them."
                + '\n' + "Bulb tiles contain Bulbs. Stop on bulb tiles to collect them. Bulbs are consumable items that can be used as free actions.";
            break;
            case "STATUS EFFECTS":
                str = 
                "Gear, Bulbs, and God Thoughts can cause status effects on both Slags and enemies."
                + '\n' + "Units can be stunned, preventing them from moving or taking an action for a turn."
                + '\n' + "Enemies can be weakened, causing them to take extra damage.";

            break;
            case "GOD THOUGHTS":
                str = 
                "Elite enemies and Bloated Bulbs contain God Thoughts—powerful items that will provide passive effects for the rest of the run."
                + '\n' + "God Thoughts can scrapped for Slime Bux.";

            break;
            case "BEACONS":
                str = 
                "Use the Beacon's action to call up to the slime bodega. There, you can redeem completed tasks, use Lotto Cards to upgrade Gear and purchase membership rewards."
                + '\n' + "Beacons are destroyed after use and can be crushed by descending units and Gear.";

            break;
            case "TASKS":
                str = 
                "Gino at the slime bodega will give tasks to complete. Check on your progress in the top right. You can redeem completed tasks for Slime Bux and re-roll uncompleted tasks at Beacons."
                + '\n' + "Slime Bux can be used to purchase membership rewards.";
            break;
            case "LOTTO CARDS":
                str = 
                "Every Beacon grants 1 Lotto Card. Scratch off the rewards to reveal a unique upgrade for each of your Slags' Gear."
                + '\n' + "Each Slag has 3 slots for upgrades. Select an upgrade from the Lotto Card, and click and hold over a slot to fill it or replace the previous upgrade.";

            break;
        }
        
        return str;
    }

    string ColorToRichText(string str) {
        return "<color=#" + ColorUtility.ToHtmlStringRGB(keyColor) + ">" + str + "</color>";
    }

}
