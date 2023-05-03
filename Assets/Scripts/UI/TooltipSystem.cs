using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipSystem : MonoBehaviour
{

    private static TooltipSystem current;
    public ToolTip tooltip;

    public void Awake()
    {
        current = this;
    }

    public static void Show(string content, string header = "")
    {
        current.tooltip.SetText(content, header);
        current.tooltip.transform.GetChild(0).gameObject.SetActive(true);
    }

    public static void Hide()
    {
        current.tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

}
