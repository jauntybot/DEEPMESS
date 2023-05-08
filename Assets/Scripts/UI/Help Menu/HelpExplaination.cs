using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class HelpExplaination : MonoBehaviour
{

    [SerializeField] RawImage image;
    [SerializeField] UnityEngine.Video.VideoPlayer vp;
    [SerializeField] TMP_Text titleTMP, bodyTMP;


    public void UpdateExplaination(string title, string body) {
        titleTMP.text = title;
        bodyTMP.text = body;
    }

}
