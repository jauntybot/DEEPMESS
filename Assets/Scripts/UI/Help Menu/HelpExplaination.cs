using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class HelpExplaination : MonoBehaviour {

    [SerializeField] GameObject gifContainer;
    [SerializeField] List<Animator> gifAnims;
    [SerializeField] Image image;
    [SerializeField] TMP_Text titleTMP, bodyTMP;
    [SerializeField] Color keyColor;


    public void UpdateExplaination(string title, string body, Sprite _image) {
        titleTMP.text = title;
        bodyTMP.text =  GetExplaination(title);
        gifContainer.SetActive(false);
        if (_image != null) {
            image.gameObject.SetActive(true);
            image.sprite = _image;
        } else image.gameObject.SetActive(false);
    }

    public void UpdateExplaination(string title, string body, List<RuntimeAnimatorController> anims) {
        titleTMP.text = title;
        bodyTMP.text = GetExplaination(title);
        image.gameObject.SetActive(false);
        gifContainer.SetActive(true);
        gifAnims[0].runtimeAnimatorController = anims[0];
        gifAnims[0].transform.parent.gameObject.SetActive(true);
        for (int i = 1; i <= gifAnims.Count - 1; i++) {
            if (anims.Count - 1 < i)
                gifAnims[i].transform.parent.gameObject.SetActive(false);
            else {
                gifAnims[i].runtimeAnimatorController = anims[i];
                gifAnims[i].transform.parent.gameObject.SetActive(true);
            }
        }
    }

    string GetExplaination(string title) {
        string str = "";
        switch (title) {
            default: break;
            case "OVERVIEW":
                bodyTMP.text = 
                "<b>" +ColorToRichText("") + "</b>";
            break;
            case "DESCENDING":
                bodyTMP.text = 
                "<b>" +ColorToRichText("") + "</b>";
            break;
            case "ENEMIES":
                bodyTMP.text = 
                "<b>" +ColorToRichText("") + "</b>";
            break;
            case "PLAYER":
                bodyTMP.text = 
                "<b>" +ColorToRichText("") + "</b>";
            break;
            case "SLAGS":
                bodyTMP.text = 
                "<b>" +ColorToRichText("") + "</b>";
            break;
        }
        
        return str;
    }

    string ColorToRichText(string str) {
        return "<color=#" + ColorUtility.ToHtmlStringRGB(keyColor) + ">" + str + "</color>";
    }

}
