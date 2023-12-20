using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class TurnOrderHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    public bool hoverable = false;
    bool hoverState = false;
    public delegate void TurnOrderEvent(bool state);
    public virtual event TurnOrderEvent OnHoverCallback;

    public void EnableHover(bool state) {
        hoverable = state;
    }

    public virtual void OnPointerEnter(PointerEventData eventData) {
        if (hoverable && !hoverState) {
            hoverState = true;
            OnHoverCallback?.Invoke(hoverState);
        }
    }

    public virtual void OnPointerExit(PointerEventData eventData) {
        if (hoverable && hoverState) {
            hoverState = false;
            OnHoverCallback?.Invoke(hoverState);
        }
    }

}
