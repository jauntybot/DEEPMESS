using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public enum Location { BottomLeft, TopRight, ToCursor };
    public Location location;

    protected static LTDescr delay;

    public string header;

    [Multiline()]
    public string content;

    public RuntimeAnimatorController anim;

    public virtual void OnPointerEnter(PointerEventData eventData) {
        if (location == Location.BottomLeft)
            TooltipSystem.ShowBL(this);
        else if (location == Location.TopRight)
            TooltipSystem.ShowTR(this);
        else
            TooltipSystem.ShowHover(this);
    }

    protected virtual void OnDisable() {
        if (TooltipSystem.activeTrigger == this)
            TooltipSystem.Hide();
    }

    public virtual void OnPointerExit(PointerEventData eventData) {
        TooltipSystem.Hide();
    }
}
