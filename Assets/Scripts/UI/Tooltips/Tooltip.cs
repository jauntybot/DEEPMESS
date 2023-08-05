using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    public enum Alignment { ToCursor, Fixed };
    public Alignment align;

    public TextMeshProUGUI headerField;
    public TextMeshProUGUI contentField;
    public LayoutElement layoutElement;
    [SerializeField] GameObject gifContainer;
    [SerializeField] Animator anim;
    
    public int textWrapLimit;

    [SerializeField] RectTransform rectTransform;

    public virtual void SetText(string content, string header = "", bool clickToSkip = false, Animator gif = null)
    {
        if (string.IsNullOrEmpty(header)) 
            headerField.gameObject.SetActive(false);
        else {
            headerField.gameObject.SetActive(true);
            headerField.text = header;
        }

        if (gif == null)
            gifContainer.SetActive(false);
        else {
            gifContainer.SetActive(true);
            anim.runtimeAnimatorController = gif.runtimeAnimatorController;
        }

        contentField.text = content;

        //limit text width
        int headerLength = headerField.text.Length;
        int contentLength = contentField.text.Length;

        layoutElement.enabled = (headerLength > textWrapLimit || contentLength > textWrapLimit) ? true : false;

        transform.GetChild(0).gameObject.SetActive(true);
    }

    public virtual void SetText(Vector2 pos, string content, string header = "",  bool clickToSkip = false,  RuntimeAnimatorController gif = null) {
        if (string.IsNullOrEmpty(header)) 
            headerField.gameObject.SetActive(false);
        else {
            headerField.gameObject.SetActive(true);
            headerField.text = header;
        }

        if (gif == null)
            gifContainer.SetActive(false);
        else {
            gifContainer.SetActive(true);
            anim.runtimeAnimatorController = gif;
        }
        contentField.text = content;

        //limit text width
        int headerLength = headerField.text.Length;
        int contentLength = contentField.text.Length;

        layoutElement.enabled = (headerLength > textWrapLimit || contentLength > textWrapLimit) ? true : false;

        GetComponent<RectTransform>().anchoredPosition = pos;
        transform.GetChild(0).gameObject.SetActive(true);
    }

    private void Update()
    {
        if (align == Alignment.ToCursor) {
            Vector2 position = Input.mousePosition;
            transform.position = position;

            Vector2 anchor = Vector2.zero;
            anchor.x = position.x < Screen.width/2 ? 0 : 1;
            anchor.y = position.y < Screen.height/2 ? 0 : 1;

            rectTransform.anchorMax = anchor; rectTransform.anchorMin = anchor; 
            rectTransform.pivot = anchor;
            rectTransform.anchoredPosition = new Vector2(25 - anchor.x * 50, 25 - anchor.y * 50);
        }
    }
}
