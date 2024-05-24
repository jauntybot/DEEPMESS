using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PathCard : MonoBehaviour {

    PathManager manager;
    Button button;

    public FloorChunk floorPacket;

    public Transform rewardContainer, bonusObjContainer, hazardsContainer;
    [SerializeField] GameObject bonusObjPrefab, nuggetRewardPrefab, relicRewardPrefab, extremeTagPrefab, eliteTagPrefab;
    [SerializeField] TMP_Text floorCount;
    public Transform subcardContainer;
    bool selectable = true;


    public void Init(PathManager _manager, FloorChunk _packet) {
        manager = _manager;
        button = GetComponent<Button>();
        
        
        floorPacket = _packet;

        floorCount.text = floorPacket.packetLength.ToString();

        // for (int i = floorPacket.nuggets - 1; i >= 0; i--) 
        //     Instantiate(nuggetRewardPrefab, rewardContainer);
        // for (int i = floorPacket.relics - 1; i >= 0; i--) 
        //     Instantiate(relicRewardPrefab, rewardContainer);
        for (int i = floorPacket.packetMods.Count - 1; i >= 0; i--) {
            hazardsContainer.gameObject.SetActive(true);
            GameObject prefab;
            switch (floorPacket.packetMods[i]) {
                default:
                case FloorChunk.PacketMods.Hazard:
                    prefab = extremeTagPrefab;
                break;
                case FloorChunk.PacketMods.Elite:
                    prefab = eliteTagPrefab;
                break;
            }
            Instantiate(prefab, hazardsContainer);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(rewardContainer.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(hazardsContainer.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
        
        selectable = true;

        for (int i = 0; i < subcardContainer.childCount - 1; i++)
            subcardContainer.GetChild(i).gameObject.SetActive(false);
        subcardContainer.gameObject.GetComponent<VerticalLayoutGroup>().spacing = 0;
    }

    public void AssignObjectives(List<Objective> objs) {
        for (int i = 0; i <= objs.Count - 1; i++) {
            BonusObjectiveCard card = Instantiate(bonusObjPrefab, bonusObjContainer).GetComponent<BonusObjectiveCard>();
            card.Init(objs[i]);
        }
        //floorPacket.objectives = objs;

        LayoutRebuilder.ForceRebuildLayoutImmediate(bonusObjContainer.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
    }

    public void UnsubObjectives() {
        foreach (Transform child in bonusObjContainer) {
            BonusObjectiveCard card = child.GetComponent<BonusObjectiveCard>();
            if (card)
                card.Unsub();
        }
    }

    public void SelectCard() {
        if (selectable) {
            manager.SelectCard(this);
        }
    }

    public IEnumerator ExpandAnimation(bool state) {
        VerticalLayoutGroup layout = subcardContainer.gameObject.GetComponent<VerticalLayoutGroup>();
        if (state) {
            if (bonusObjContainer.childCount > 0) 
                bonusObjContainer.parent.gameObject.SetActive(true);
            subcardContainer.GetChild(0).gameObject.SetActive(true);
        }
        float from = state ? -150 : 0;
        float space = state ? 0 : -150;
        float t = 0;
        while (t < 0.15f) {
            layout.spacing = Mathf.Lerp(from, space, t/0.15f);
            LayoutRebuilder.ForceRebuildLayoutImmediate(subcardContainer.GetComponent<RectTransform>());
            Canvas.ForceUpdateCanvases();
            yield return null;
            t += Time.deltaTime; 
        }
        yield return null;
        if (!state) {
            for (int i = 0; i < subcardContainer.childCount - 1; i++)
                subcardContainer.GetChild(i).gameObject.SetActive(false);
        }
        layout.spacing = space;
        LayoutRebuilder.ForceRebuildLayoutImmediate(subcardContainer.GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
    }

    public void SelectPath() {
        manager.StartCoroutine(manager.SelectPath(this));
        selectable = false;
        Button b = subcardContainer.GetChild(0).GetComponent<Button>();
        b.enabled = false;
        foreach (Transform child in b.transform) 
            child.gameObject.SetActive(false);
    }

    
}
