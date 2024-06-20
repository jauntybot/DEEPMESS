using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NeutralElementTooltip : Tooltip {

    public Image tilePreview;
    public Animator tilePreviewAnim;


    public TMP_Text titleText;
    public TMP_Text contentText;

    [SerializeField] Sprite wallSprite, beaconSprite, boneSprite;
    [SerializeField] RuntimeAnimatorController bloodAnim, bileAnim, healBulbAnim, surgeBulbAnim, stunBulbAnim, bloatedBulbAnim;

    public override void SetText(string content = "", string header = "", List<RuntimeAnimatorController> gif = null) {
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
            else if (header == "BEACON") tilePreview.sprite = beaconSprite;
            else tilePreview.sprite = boneSprite;
        }

        contentField.text = content;

        StartCoroutine(Rebuild());
    }

    protected override IEnumerator Rebuild() {
        yield return base.Rebuild();
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform.GetChild(0).GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent.GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
    }

    public void HoverOver(GridElement ge = null) {
        if (ge == null) {
            rectTransform.gameObject.SetActive(false);
        } else {
            rectTransform.gameObject.SetActive(true);
            if (ge is Wall) {
                SetText("Blocks movement, damages if landed on. Can be destroyed.", "WALL");
            } else if (ge is Beacon) {
                SetText("Direct line to Gino. Can be destroyed.", "BEACON");
            } else if (ge is BloatedBulb) {
                SetText("Releases a god thought when destroyed.", "BLOATED BULB", new List<RuntimeAnimatorController>{ bloatedBulbAnim });
            } else if (ge is Tile t) {
                if (ge is TileBulb tb) {
                    if (tb.bulb is SupportBulbData s) {
                        if (s.supportType == SupportBulbData.SupportType.Heal) 
                            SetText("Contains a Heal Bulb. Restores 2 HP.", "HEAL BULB", new List<RuntimeAnimatorController>{ healBulbAnim });
                        else 
                            SetText("Contains a Surge Bulb. Refresh Slag action and move.", "SURGE BULB", new List<RuntimeAnimatorController>{ surgeBulbAnim });
                    } else if (tb.bulb is DebuffBulbData) 
                        SetText("Contains a Stun Bulb. Stuns in area of effect.", "BULB", new List<RuntimeAnimatorController>{ stunBulbAnim });
                } else {
                    switch(t.tileType) {
                        case Tile.TileType.Bone:
                            SetText("No special effect.", "BONE");
                        break;
                        case Tile.TileType.Bile:
                            SetText("Destroys anything that lands in it.", "BILE", new List<RuntimeAnimatorController>{ bileAnim });
                        break;
                        case Tile.TileType.Blood:
                            SetText("Prevents Slag action while stopped in.", "BLOOD", new List<RuntimeAnimatorController>{ bloodAnim });
                        break;
                    }
                }
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            Canvas.ForceUpdateCanvases();
        }
    }
}
