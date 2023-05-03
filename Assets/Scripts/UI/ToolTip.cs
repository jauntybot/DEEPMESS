using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ToolTip : MonoBehaviour
{

    public TextMeshProUGUI headerField;
    public TextMeshProUGUI contentField;
    public LayoutElement layoutElement;
    
    public int textWrapLimit;

    [SerializeField] RectTransform rectTransform;

    public void SetText(string content, string header = "")
    {
        if (string.IsNullOrEmpty(header))
        {
            headerField.gameObject.SetActive(false);
        }
        else
        {
            headerField.gameObject.SetActive(true);
            headerField.text = header;
        }

        contentField.text = content;

        //limit text width
        int headerLength = headerField.text.Length;
        int contentLength = contentField.text.Length;

        layoutElement.enabled = (headerLength > textWrapLimit || contentLength > textWrapLimit) ? true : false;
    }

    private void Update()
    {
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
