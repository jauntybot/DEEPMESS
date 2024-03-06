using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PathCard : MonoBehaviour {

    PathManager manager;
    Button button;

    public FloorPacket floorPacket;

    public Transform bonusObjContainer, rewardContainer;
    [SerializeField] GameObject bonusObjPrefab, nuggetRewardPrefab;
    [SerializeField] TMP_Text floorCount;


    public void Init(PathManager _manager, FloorPacket _packet) {
        manager = _manager;
        button = GetComponent<Button>();
        
        
        floorPacket = _packet;

        floorCount.text = floorPacket.packetLength.ToString();

        for (int i = floorPacket.nuggets - 1; i >= 0; i--) {
            Instantiate(nuggetRewardPrefab, rewardContainer);
        }
        for (int i = floorPacket.relics - 1; i >= 0; i--) {
            
        }
    }

    public void AssignObjectives(List<Objective> objs) {
        for (int i = 0; i <= objs.Count - 1; i++) {
            BonusObjectiveCard card = Instantiate(bonusObjPrefab, bonusObjContainer).GetComponent<BonusObjectiveCard>();
            card.Init(objs[i]);
        }
        floorPacket.objectives = objs;

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

    public void SelectPath() {
        manager.SelectPath(this);

    }

    
}
