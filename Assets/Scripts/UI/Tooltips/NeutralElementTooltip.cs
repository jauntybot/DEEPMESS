using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NeutralElementTooltip : Tooltip
{

    public Image tilePreview;
    public Animator tilePreviewAnim;


    public TMP_Text titleText;
    public TMP_Text contentText;

    [SerializeField] Sprite wallSprite, boneSprite;
    [SerializeField] RuntimeAnimatorController bloodAnim, bileAnim, bulbAnim;

    public override void SetText(string content = "", string header = "", bool clickToSkip = false, List<RuntimeAnimatorController> gif = null) {
         transform.GetChild(0).gameObject.SetActive(true);
        
        if (string.IsNullOrEmpty(header)) 
            headerField.gameObject.SetActive(false);
        else {
            headerField.gameObject.SetActive(true);
            headerField.text = header;
        }

        if (gif != null)
            tilePreviewAnim.runtimeAnimatorController = gif[0];
        else {
            tilePreviewAnim.runtimeAnimatorController = null;
            if (header == "WALL") tilePreview.sprite = wallSprite;
            else tilePreview.sprite = boneSprite;
        }

        contentField.text = content;

        StartCoroutine(Rebuild());
    }

    public void HoverOver(GridElement ge = null) {
        if (ge == null) {
            rectTransform.gameObject.SetActive(false);
        } else {
            rectTransform.gameObject.SetActive(true);
            if (ge is Wall) {
                SetText("Blocks movement, damages if landed on.", "WALL");
            } else if (ge is Tile t) {
                if (ge is TileBulb) {
                    SetText("Contains a bulb.", "BULB", false, new List<RuntimeAnimatorController>{ bulbAnim });
                } else {
                    switch(t.tileType) {
                        case Tile.TileType.Bone:
                            SetText("No special effect.", "BONE");
                        break;
                        case Tile.TileType.Bile:
                            SetText("Destroys anything that lands in it.", "BILE", false, new List<RuntimeAnimatorController>{ bileAnim });
                        break;
                        case Tile.TileType.Blood:
                            SetText("Prevents Slags from acting.", "BLOOD", false, new List<RuntimeAnimatorController>{ bloodAnim });
                        break;
                    }
                }
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            Canvas.ForceUpdateCanvases();
        }
    }
}
