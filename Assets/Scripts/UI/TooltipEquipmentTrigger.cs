using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteInEditMode]
public class TooltipEquipmentTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    private static LTDescr delay;
    //get the equipment data
    private Image image;
    private GameObject hammerButton;
    private string imageName;

    public string header;
    [Multiline()]
    public string content;

    private void Awake()
    {
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
    
        hammerButton = eventData.pointerEnter;
        Debug.Log(hammerButton.name);
        image = hammerButton.GetComponent<Image>();
        imageName = image.sprite.name;
        Debug.Log("image name is " + imageName);

        delay = LeanTween.delayedCall(1f, () =>
        {
            switch (imageName)
            {
                case "eqp - anvil":
                    Anvil();
                    break;
                case "eqp - immobilize":
                    Immobilize();
                    break;
                case "eqp - big wind":
                    BigWind();
                    break;
                case "eqp - shield":
                    Shield();
                    break;
                case "eqp - swap":
                    Swap();
                    break;
                case "eqp - big grab":
                    BigGrab();
                    break;
                case "hammer - active new":
                    Hammer();
                    break;
            }

            TooltipSystem.Show(content, header);
        });
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        LeanTween.cancel(delay.uniqueId);
        TooltipSystem.Hide();
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
