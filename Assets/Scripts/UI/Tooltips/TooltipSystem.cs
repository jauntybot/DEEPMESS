using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipSystem : MonoBehaviour
{

    public static TooltipSystem instance;
    public Tooltip tooltip;
    public static TooltipTrigger activeTrigger;

    public void Awake()
    {
        instance = this;
    }

    public static void Show(TooltipTrigger trigger)
    {
        activeTrigger = trigger;
        instance.tooltip.SetText(trigger.content, trigger.header);
        instance.tooltip.transform.GetChild(0).gameObject.SetActive(true);
    }

    public static void Hide()
    {
        instance.tooltip.transform.GetChild(0).gameObject.SetActive(false);
        activeTrigger = null;
    }

}
