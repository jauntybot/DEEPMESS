using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour {
    public enum Alignment { ToCursor, CenterScreen, Fixed };
    public Alignment align;

    public TextMeshProUGUI headerField;
    public TextMeshProUGUI contentField;
    public GameObject gifContainer;
    [SerializeField] List<Animator> gifAnims;
    
    public int textWrapLimit;

    [SerializeField] protected RectTransform rectTransform;

    public virtual void SetText(string content = "", string header = "", List<RuntimeAnimatorController> gif = null) {
        if (string.IsNullOrEmpty(header)) {
            headerField.transform.parent.gameObject.SetActive(false);
            transform.GetChild(0).GetComponent<VerticalLayoutGroup>().padding.top = 5;
        } else {
            headerField.transform.parent.gameObject.SetActive(true);
            headerField.text = header;
            transform.GetChild(0).GetComponent<VerticalLayoutGroup>().padding.top = 20;
        }

        if (gif == null) 
            gifContainer.SetActive(false);
        else {
            gifContainer.SetActive(true);
            gifAnims[0].runtimeAnimatorController = gif[0];
            gifAnims[0].transform.parent.gameObject.SetActive(true);
            for (int i = 1; i <= gifAnims.Count - 1; i++) {
                if (gif.Count - 1 < i)
                    gifAnims[i].transform.parent.gameObject.SetActive(false);
                else {
                    gifAnims[i].runtimeAnimatorController = gif[i];
                    gifAnims[i].transform.parent.gameObject.SetActive(true);
                }
            }
        }

        if (string.IsNullOrEmpty(content))
            contentField.transform.parent.gameObject.SetActive(false);
        else {
            contentField.transform.parent.gameObject.SetActive(true);
            contentField.text = content;
        }
        
        transform.GetChild(0).gameObject.SetActive(true);

        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
        StartCoroutine(Rebuild());

        //limit text width
        //int headerLength = headerField.text.Length;
        //int contentLength = contentField.text.Length;

        //layoutElement.enabled = (headerLength > textWrapLimit || contentLength > textWrapLimit) ? true : false;
    }

    protected virtual IEnumerator Rebuild() {
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
    }

    protected virtual void Update() {
        if (transform.GetChild(0).gameObject.activeSelf) {
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
                    Vector2 sign;
                    if (position.x < Screen.width * 4/5) {
                        localAnchor.x = 0;
                        sign.x = 1;
                    } else {
                        localAnchor.x = 1;
                        sign.x = -1;
                    }
                    
                    if (position.y < Screen.height * 4/5) {
                        localAnchor.y = 0;
                        sign.y = 1;
                    } else {
                        localAnchor.y = 1;
                        sign.y = -1;
                    }

                    rectTransform.anchorMax = localAnchor; rectTransform.anchorMin = localAnchor; rectTransform.pivot = localAnchor;
                    
                    Vector3 pos = new Vector2(position.x + 20 * sign.x, position.y);
                    if (sign.y == -1) {
                        if (sign.x == -1) pos.x += 20; 
                        else pos.x += 40;
                        pos.y -= 20;
                    }
                    rectTransform.position = pos;
                break;
            }
        }
    }
}
