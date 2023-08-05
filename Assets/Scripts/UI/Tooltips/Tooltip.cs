using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    public enum Alignment { ToCursor, CenterScreen, Fixed };
    public Alignment align;

    public TextMeshProUGUI headerField;
    public TextMeshProUGUI contentField;
    [SerializeField] GameObject gifContainer;
    [SerializeField] List<Animator> gifAnims;
    
    public int textWrapLimit;

    [SerializeField] RectTransform rectTransform;

    public virtual void SetText(string content = "", string header = "", bool clickToSkip = false, List<RuntimeAnimatorController> gif = null)
    {
        transform.GetChild(0).gameObject.SetActive(true);
        
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
            gifAnims[0].runtimeAnimatorController = gif[0];
            for (int i = 1; i <= gifAnims.Count - 1; i++) {
                if (gif.Count - 1 < i)
                    gifAnims[i].gameObject.SetActive(false);
                else {
                    gifAnims[i].runtimeAnimatorController = gif[i];
                    gifAnims[i].gameObject.SetActive(true);
                }
            }
        }

        contentField.text = content;

        //limit text width
        //int headerLength = headerField.text.Length;
        //int contentLength = contentField.text.Length;

        //layoutElement.enabled = (headerLength > textWrapLimit || contentLength > textWrapLimit) ? true : false;


    }

    protected virtual void Update()
    {
        switch (align) {
            case Alignment.CenterScreen:
                RectTransform anchor = transform.GetChild(0).GetComponent<RectTransform>();
                anchor.anchorMin = new Vector2(0.5f, 0.5f);
                anchor.anchorMax = new Vector2(0.5f, 0.5f);
                anchor.anchoredPosition = new Vector2(0, anchor.rect.height/2);

            break;
            case Alignment.ToCursor:
                Vector2 position = Input.mousePosition;
                transform.position = position;

                Vector2 localAnchor = Vector2.zero;
                localAnchor.x = position.x < Screen.width/2 ? 0 : 1;
                localAnchor.y = position.y < Screen.height/2 ? 0 : 1;

                rectTransform.anchorMax = localAnchor; rectTransform.anchorMin = localAnchor; 
                rectTransform.pivot = localAnchor;
                rectTransform.anchoredPosition = new Vector2(25 - localAnchor.x * 50, 25 - localAnchor.y * 50);
            break;
        }
        if (align == Alignment.ToCursor) {
        }
    }
}
