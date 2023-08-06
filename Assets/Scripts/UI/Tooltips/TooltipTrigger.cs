using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public enum Location { BottomLeft, TopRight };
    public Location location;

    protected static LTDescr delay;

    public string header;

    [Multiline()]
    public string content;

    public RuntimeAnimatorController anim;

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (location == Location.BottomLeft)
            TooltipSystem.ShowBL(this);
        else
            TooltipSystem.ShowTR(this);
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
