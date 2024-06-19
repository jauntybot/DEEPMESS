using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Relics;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;

public class PathManager : MonoBehaviour {

    FloorSequence floorSequence;
    [SerializeField] UpgradeManager upgradeManager;
    [SerializeField] RelicManager relicManager;


    List<PathCard> activeCards;
    [HideInInspector] public PathCard selectedPathCard;
    [SerializeField] Transform pathCardContainer;
    [SerializeField] GameObject pathCardPrefab, pathChoiceContainer;
    [HideInInspector] public bool choosingPath = false;
    

    [SerializeField] List<Sprite> rewardSprites;
    
    [Header("Node Video Player")]
    bool cutsceneSkip;
    [SerializeField] VideoPlayer videoPlayer;
    [SerializeField] List<VideoClip> nodeClips;
    [SerializeField] List<AudioClip> nodeAudio;
    [SerializeField] AudioSource audioSource;

    int clipIndex;

// RELIC SYSTEM - DELETE
    public int objectiveDiscount;


    void Start() {
        cutsceneSkip = PersistentDataManager.instance.userData.cutsceneSkip;
        floorSequence = FloorManager.instance.floorSequence;
        objectiveDiscount = 0;
        clipIndex = ScenarioManager.instance.startCavity-1;
        if (clipIndex < 0) clipIndex = 0;
    }

    public IEnumerator PathSequence(bool save) {
        HorizontalLayoutGroup layout = pathCardContainer.GetComponent<HorizontalLayoutGroup>();
        layout.enabled = true;
        choosingPath = true;
        pathChoiceContainer.SetActive(true);
        activeCards = new();
        float t  = 0;

        if (save) PersistentDataManager.instance.SaveRun();
        
        for (int i = pathCardContainer.childCount - 1; i >= 0; i--) {
            Destroy(pathCardContainer.GetChild(i).gameObject);
        }

// Draw packets from floor sequence for nodes
        int rnd = 2;
        switch(floorSequence.currentThreshold) {
            case FloorChunk.PacketType.I: rnd = 3; break;
            case FloorChunk.PacketType.II: rnd = 4; break;
            case FloorChunk.PacketType.BOSS: rnd = 1; break;
        }
        List<FloorChunk> randomPackets = floorSequence.RandomNodes(rnd);
        
// Initialize PathCards
        for (int i = 0; i <= randomPackets.Count - 1; i++) {
            PathCard pc = Instantiate(pathCardPrefab, pathCardContainer).GetComponent<PathCard>();
            pc.Init(this, randomPackets[i]);
            activeCards.Add(pc);
        }
       
        LayoutRebuilder.ForceRebuildLayoutImmediate(pathCardContainer.GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
        yield return null;
        layout.enabled = false;
        t = 0; while (t < 0.2f) { t += Time.deltaTime; yield return null; }
        
        for (int i = 0; i <= pathCardContainer.childCount - 1; i++) {
            pathCardContainer.GetChild(i).GetComponent<Animator>().SetTrigger("SlideIn");
            t = 0; while (t < 0.1f) { t += Time.deltaTime; yield return null; }
        }
        t = 0; while (t < 0.2f) { t += Time.deltaTime; yield return null; }
        
        layout.enabled = true;
        LayoutRebuilder.ForceRebuildLayoutImmediate(pathCardContainer.GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();

        while (choosingPath)
            yield return null;

    }

    public IEnumerator SelectPath(PathCard selected) {
        Vector2 from = selected.transform.localPosition;
        HorizontalLayoutGroup layout = pathCardContainer.GetComponent<HorizontalLayoutGroup>();
        layout.enabled = false;
        yield return null;
        
        float t = 0;
// Destroy other path nodes
        activeCards = new() { selected };
        for (int i = pathCardContainer.childCount - 1; i >= 0; i--) {
            PathCard c = pathCardContainer.GetChild(i).GetComponent<PathCard>();
            if (c == selected) continue;
            c.GetComponent<Animator>().SetBool("Del", true);
            c.GetComponent<Animator>().SetTrigger("SlideOut");
            t = 0; while (t < 0.1f) { t += Time.deltaTime; yield return null; }
        }
        t = 0; while (t < 0.35f) { t += Time.deltaTime; yield return null; }

        Transform card = pathCardContainer.GetChild(0);
        t = 0; while (t <= 0.2f) { 
            card.localPosition = new Vector2(Mathf.Lerp(from.x, 0, t/0.2f), from.y);
            t += Time.deltaTime; 
            yield return null; 
        }
        card.localPosition = new Vector2(0, from.y);
        
        layout.enabled = true;
        t = 0; while (t <= 0.75f) { t += Time.deltaTime; yield return null; }
        card.GetComponent<Animator>().SetTrigger("SlideOut");
        t = 0; while (t <= 0.45f) { t += Time.deltaTime; yield return null; }

        pathChoiceContainer.SetActive(false);

        if (!cutsceneSkip) {
            videoPlayer.clip = nodeClips[clipIndex];
            videoPlayer.frame = 0;
            videoPlayer.gameObject.SetActive(true);
            audioSource.PlayOneShot(nodeAudio[clipIndex]);
            clipIndex++;
            RawImage vp = videoPlayer.GetComponent<RawImage>();
            vp.color = new Color(1,1,1,0);
            t = 0; while (t < 0.5f) { 
                vp.color = new Color(1,1,1,Mathf.Lerp(0,1,t/0.5f));
                t += Time.deltaTime; yield return null; 
            }
            vp.color = new Color(1,1,1,1);

            t = 0; while (t < 6f) { t += Time.deltaTime; yield return null; }
            t = 0; while (t < 0.5f) { 
                vp.color = new Color(1,1,1,Mathf.Lerp(1,0,t/0.5f));
                t += Time.deltaTime; yield return null; 
            }
            videoPlayer.gameObject.SetActive(false);
        } else yield return new WaitForSecondsRealtime(0.5f);
// Assign selected path
        floorSequence.StartPacket(selected.floorPacket);
        // if (selected.floorPacket.objectives.Count > 0) {
        //     tracker.gameObject.SetActive(true);
        //     SubscribeTracker(selected.floorPacket.objectives);
        // } else
        //tracker.gameObject.SetActive(false);
        
        choosingPath = false;
    }

    public void EndPathSequence() {
        choosingPath = false;
    }



    public void SelectCard(PathCard selected) {
        if (selectedPathCard)
            selectedPathCard.selectButton.gameObject.SetActive(false);
        if (selectedPathCard != selected) {
            selectedPathCard = selected;
            selectedPathCard.selectButton.gameObject.SetActive(true);
        } else 
            selectedPathCard = null;
    }
}
