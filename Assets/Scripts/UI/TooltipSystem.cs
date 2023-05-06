using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipSystem : MonoBehaviour
{

    public static TooltipSystem instance;
    public ToolTip tooltip;

    public void Awake()
    {
        if (instance) {
            Debug.Log("Warning! More than one instance of TooltipSystem found!");
            DestroyImmediate(instance.gameObject);
        }
        instance = this;
    }

    public static void Show(string content, string header = "")
    {
        instance.tooltip.SetText(content, header);
        instance.tooltip.transform.GetChild(0).gameObject.SetActive(true);
    }

    public static void Hide()
    {
        instance.tooltip.transform.GetChild(0).gameObject.SetActive(false);
    }

}
