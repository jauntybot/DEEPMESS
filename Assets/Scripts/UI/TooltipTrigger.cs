using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    private static LTDescr delay;

    public string header;

    [Multiline()]
    public string content;

    public void OnPointerEnter(PointerEventData eventData)
    {
        delay = LeanTween.delayedCall(1f, () =>
        {
            TooltipSystem.Show(content, header);
        });
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        LeanTween.cancel(delay.uniqueId);
        TooltipSystem.Hide();
    }
}
