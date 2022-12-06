using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessagePanel : MonoBehaviour
{
 
    public GameObject panel;
    public Animator anim;
    public TMPro.TMP_Text messageText;


    public IEnumerator DisplayMessage(string message) {
        panel.SetActive(true);
        messageText.text = message;
        while (panel.activeSelf) {
            yield return null;
        }
    }

}
