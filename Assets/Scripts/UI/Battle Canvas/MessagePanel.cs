using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessagePanel : MonoBehaviour
{
 
    public GameObject panel;
    public Animator anim;
    [SerializeField] Image panelImage;
    public TMPro.TMP_Text messageText;
    [SerializeField] List<Color> messageColors;

    public IEnumerator DisplayMessage(string message, int colorIndex) {
        panel.SetActive(true);
        panelImage.color = messageColors[colorIndex];
        messageText.text = message;
        while (panel.activeSelf) {
            yield return null; 
        }
    }

}
