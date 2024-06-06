using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeTooltip : Tooltip {

    [SerializeField] Transform infoContainer;
    [SerializeField] Image gearIcon;
    [SerializeField] TMP_Text upgradeLvl;


    public virtual void SetText(string content = "", string header = "", Sprite icon = null, int lvl = 1) {
        if (string.IsNullOrEmpty(header)) {
            headerField.transform.parent.gameObject.SetActive(false);
        } else {
            headerField.transform.parent.gameObject.SetActive(true);
            headerField.text = header;
        }
        
        if (string.IsNullOrEmpty(content))
            contentField.transform.parent.gameObject.SetActive(false);
        else {
            contentField.transform.parent.gameObject.SetActive(true);
            contentField.text = content;
        }
        
        transform.GetChild(0).gameObject.SetActive(true);
        
        infoContainer.gameObject.SetActive(icon != null);
        if (icon != null) {
            headerField.GetComponent<RectTransform>().anchorMin = new Vector2(0.6f, 1);
            headerField.GetComponent<RectTransform>().anchorMax = new Vector2(0.6f, 1);
        } else {
            headerField.transform.parent.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 1);
            headerField.transform.parent.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1);
        }
        
        gearIcon.sprite = icon;
        upgradeLvl.text = "LVL " + lvl;
        

        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
        StartCoroutine(Rebuild());
    }

}
