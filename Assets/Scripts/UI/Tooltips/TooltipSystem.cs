using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipSystem : MonoBehaviour
{

    public static TooltipSystem instance;
    public Tooltip tooltipBL;
    public Tooltip tooltipTR;
    public static TooltipTrigger activeTrigger;

    public void Awake()
    {
        instance = this;
    }

    public static void ShowTR(TooltipTrigger trigger)
    {
        RuntimeAnimatorController anim = trigger.anim;
        activeTrigger = trigger;
        instance.tooltipTR.SetText(trigger.content, trigger.header, false, new List<RuntimeAnimatorController>{ anim });
        instance.tooltipTR.transform.GetChild(0).gameObject.SetActive(true);
    }

    public static void ShowBL(TooltipTrigger trigger)
    {
        RuntimeAnimatorController anim = trigger.anim;
        activeTrigger = trigger;
        instance.tooltipBL.SetText(trigger.content, trigger.header, false, new List<RuntimeAnimatorController>{ anim });
        instance.tooltipBL.transform.GetChild(0).gameObject.SetActive(true);
    }

    public static void Hide()
    {
        instance.tooltipBL.transform.GetChild(0).gameObject.SetActive(false);
        instance.tooltipTR.transform.GetChild(0).gameObject.SetActive(false);
        activeTrigger = null;
    }

}
