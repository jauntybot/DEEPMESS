using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    protected static LTDescr delay;

    public string header;

    [Multiline()]
    public string content;

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        TooltipSystem.Show(this);
    }

    protected virtual void OnDisable() {
        if (TooltipSystem.activeTrigger == this)
            TooltipSystem.Hide();
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        TooltipSystem.Hide();
    }
}
