using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PathCard : MonoBehaviour {

    PathManager manager;
    Button button;

    public FloorChunk floorPacket;

    public Transform hazardsContainer;
    [SerializeField] GameObject extremeTagPrefab, eliteTagPrefab;
    [SerializeField] TMP_Text floorCount;
    public Button selectButton;
    bool selectable = true;


    public void Init(PathManager _manager, FloorChunk _packet) {
        manager = _manager;
        button = GetComponent<Button>();
        
        
        floorPacket = _packet;

        floorCount.text = floorPacket.packetLength.ToString();

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

        LayoutRebuilder.ForceRebuildLayoutImmediate(hazardsContainer.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
        
        selectable = true;
    }

    public void SelectCard() {
        if (selectable) {
            manager.SelectCard(this);
        }
    }

    public void SelectPath() {
        manager.StartCoroutine(manager.SelectPath(this));
        selectable = false;
        selectButton.enabled = false;
        foreach (Transform child in selectButton.transform) 
            child.gameObject.SetActive(false);
    }

    
}
