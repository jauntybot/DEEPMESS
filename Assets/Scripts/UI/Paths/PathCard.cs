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
    [SerializeField] GameObject vanillaTagPrefab, hazardTagPrefab, extremeTagPrefab, eliteTagPrefab, bossTagPrefab;
    [SerializeField] TMP_Text floorCount, beaconCount, godThoughtCount;
    public Button selectButton;
    bool selectable = true;


    public void Init(PathManager _manager, FloorChunk _packet) {
        manager = _manager;
        button = GetComponent<Button>();
        
        
        floorPacket = _packet;

        floorCount.text = _packet.packetType == FloorChunk.PacketType.BOSS? "?" : floorPacket.packetLength.ToString();
        beaconCount.text = "x" + Mathf.Ceil(_packet.packetLength / 4).ToString();

        GameObject prefab = null;
            
        if (floorPacket.packetMods.Count == 0 && floorPacket.packetType != FloorChunk.PacketType.BOSS) {
            prefab = vanillaTagPrefab;
            godThoughtCount.text = _packet.packetType == FloorChunk.PacketType.I? "x1" : "x2";
        } else if (floorPacket.packetType == FloorChunk.PacketType.BOSS) {
            prefab = bossTagPrefab;
            beaconCount.text = "x0";
            godThoughtCount.text = "x0";
        } else {
            godThoughtCount.text = _packet.packetType == FloorChunk.PacketType.I? "x2" : "x3";
            for (int i = floorPacket.packetMods.Count - 1; i >= 0; i--) {
                switch (floorPacket.packetMods[i]) {
                    default:
                    case FloorChunk.PacketMods.Hazard:
                        prefab = hazardTagPrefab;
                    break;
                    case FloorChunk.PacketMods.Elite:
                        prefab = eliteTagPrefab;
                    break;
                    case FloorChunk.PacketMods.Extreme:
                        prefab = extremeTagPrefab;
                    break;
                }
            }
        }
        Instantiate(prefab, hazardsContainer);

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
