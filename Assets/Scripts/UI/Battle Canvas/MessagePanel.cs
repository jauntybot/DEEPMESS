using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessagePanel : MonoBehaviour
{
 
    public enum Message { Slag, Antibody, Descent, FinalDescent, Cascade, Position, Win, Lose } ;

    public Animator anim;
    [Header("Old Anim")]
    public GameObject panel;
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

    public IEnumerator PlayMessage(Message message) {
        string trigger = "";
        switch(message) {
            default:
            case Message.Slag:
                trigger = "Slag";
            break;
            case Message.Antibody:
                trigger = "Antibody";
            break;
            case Message.Descent:
                trigger = "Descent";
            break;
            case Message.FinalDescent:
                trigger = "FinalDescent";
            break;
            case Message.Cascade:
                trigger = "Cascade";
            break;
            case Message.Position:
                trigger = "Position";
            break;
            case Message.Win:
                trigger = "Win";
            break;
            case Message.Lose:
                trigger = "Lose";
            break;
        }
        anim.SetTrigger(trigger);
        yield return null;
        yield return new WaitForSecondsRealtime(anim.GetCurrentAnimatorStateInfo(0).length);
    }

}
