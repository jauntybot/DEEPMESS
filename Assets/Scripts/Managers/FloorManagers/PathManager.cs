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
    
    [SerializeField] ObjectiveTracker tracker;

    List<PathCard> activeCards;
    [HideInInspector] public PathCard selectedPathCard;
    [SerializeField] TMP_Text sequenceTitle;
    [SerializeField] Transform pathCardContainer;
    [SerializeField] GameObject pathCardPrefab, pathChoiceContainer;
    [HideInInspector] public bool choosingPath = false;
    
    [Header("Serialized Objective Pools")]
    [SerializeField] List<Objective> packetIObjectives;
    [SerializeField] List<Objective> packetIIObjectives, packetIIIObjectives;
    [SerializeField] List<Sprite> rewardSprites;
    
    [Header("Node Video Player")]
    [SerializeField] VideoPlayer videoPlayer;
    [SerializeField] List<VideoClip> nodeClips;
    [SerializeField] List<AudioClip> nodeAudio;
    [SerializeField] AudioSource audioSource;

    int clipIndex;

// RELIC SYSTEM - DELETE
    public int objectiveDiscount;


    void Start() {
        floorSequence = FloorManager.instance.floorSequence;
        ClearObjectives();
        objectiveDiscount = 0;
        clipIndex = ScenarioManager.instance.startCavity-1;
        if (clipIndex < 0) clipIndex = 0;
    }

    public IEnumerator PathSequence() {
        HorizontalLayoutGroup layout = pathCardContainer.GetComponent<HorizontalLayoutGroup>();
        layout.enabled = true;
        choosingPath = true;
        pathChoiceContainer.SetActive(true);
        sequenceTitle.text = "CHOOSE A PATH";
        activeCards = new();
        float t  = 0;

        
        for (int i = pathCardContainer.childCount - 1; i >= 0; i--) {
            Destroy(pathCardContainer.GetChild(i).gameObject);
        }

// Draw packets from floor sequence for nodes
        int rnd = 2;
        switch(floorSequence.currentThreshold) {
            case FloorPacket.PacketType.I: rnd = 2; break;
            case FloorPacket.PacketType.II: rnd = 3; break;
            case FloorPacket.PacketType.III: rnd = 3; break;
            case FloorPacket.PacketType.BOSS: rnd = 1; break;
        }
        List<FloorPacket> randomPackets = floorSequence.RandomNodes(rnd);
        
// Initialize PathCards
        int totalObjectives = 0;
        for (int i = 0; i <= randomPackets.Count - 1; i++) {
            PathCard pc = Instantiate(pathCardPrefab, pathCardContainer).GetComponent<PathCard>();
            pc.Init(this, randomPackets[i]);
            totalObjectives += randomPackets[i].bonusNuggetObjectives + randomPackets[i].bonusRelicObjectives;
            activeCards.Add(pc);
        }

// Assign random objectives based on packet, prevents duplicates
        List<Objective> objectives = GetObjectives(totalObjectives);
        for (int i = 0; i <= activeCards.Count - 1; i++) {
            List<Objective> packetObjs = new();
            for (int o = randomPackets[i].bonusNuggetObjectives - 1; o >= 0; o--) {
                packetObjs.Add(objectives[0].Init(true, objectiveDiscount));
                objectives.RemoveAt(0);
            }
            for (int o = randomPackets[i].bonusRelicObjectives - 1; o >= 0; o--) {
                packetObjs.Add(objectives[0].Init(false, objectiveDiscount));
                objectives.RemoveAt(0);
            }
            activeCards[i].AssignObjectives(packetObjs);
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
// Destroy other path nodes and unsub objectives
        activeCards = new() { selected };
        for (int i = pathCardContainer.childCount - 1; i >= 0; i--) {
            PathCard c = pathCardContainer.GetChild(i).GetComponent<PathCard>();
            if (c == selected) continue;
            foreach(Transform child in c.bonusObjContainer) {
                BonusObjectiveCard o = child.GetComponent<BonusObjectiveCard>();
                if (o)
                    o.Unsub();
            }
            //card.transform.parent = null;
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
        t = 0; while (t <= .25f) { t += Time.deltaTime; yield return null; }
        
        videoPlayer.clip = nodeClips[clipIndex];
        videoPlayer.gameObject.SetActive(true);
        videoPlayer.frame = 0;
        audioSource.PlayOneShot(nodeAudio[clipIndex]);
        clipIndex++;
        t = 0; while (t < 0.5f) { 
            videoPlayer.GetComponent<RawImage>().color = new Color(1,1,1,Mathf.Lerp(0,1,t/0.5f));
            t += Time.deltaTime; yield return null; 
        }
        videoPlayer.GetComponent<RawImage>().color = new Color(1,1,1,1);
        pathChoiceContainer.SetActive(false);

        t = 0; while (t < 6f) { t += Time.deltaTime; yield return null; }
        t = 0; while (t < 0.5f) { 
            videoPlayer.GetComponent<RawImage>().color = new Color(1,1,1,Mathf.Lerp(1,0,t/0.5f));
            t += Time.deltaTime; yield return null; 
        }
        videoPlayer.gameObject.SetActive(false);

// Assign selected path
        floorSequence.StartPacket(selected.floorPacket);
        if (selected.floorPacket.objectives.Count > 0) {
            tracker.gameObject.SetActive(true);
            SubscribeTracker(selected.floorPacket.objectives);
        } else tracker.gameObject.SetActive(false);
        
        choosingPath = false;
    }

    public IEnumerator PathRewardSequence() {
        foreach (Objective ob in floorSequence.activePacket.objectives)
            ob.ProgressCheck(true);

        pathChoiceContainer.SetActive(true);
        sequenceTitle.text = "PATH RESULTS";
        PathCard card = pathCardContainer.GetChild(0).GetComponent<PathCard>();
        card.GetComponent<Animator>().SetTrigger("SlideIn");

// Shuffle bag for random nugget rewards
        ShuffleBag<SlagEquipmentData.UpgradePath> rndBag = new();
        for (int i = 0; i <= 3; i++) {
            rndBag.Add(SlagEquipmentData.UpgradePath.Shunt);
            rndBag.Add(SlagEquipmentData.UpgradePath.Scab);
            rndBag.Add(SlagEquipmentData.UpgradePath.Sludge);
        }

// Delay for anim in
        float t = 0; while (t < 1.25f) { t += Time.deltaTime; yield return null; }

// Collect nugget rewards sequentially
        for (int i = 0; i <= floorSequence.activePacket.nuggets - 1; i++) {
            Transform r = card.rewardContainer.GetChild(0);
            r.GetComponent<Animator>().SetTrigger("Reward");
            t = 0; while (t < 0.5f) { t += Time.deltaTime; yield return null; } 
            r.SetParent(null);
            upgradeManager.CollectNugget(rndBag.Next());
            t = 0; while (t < 0.6f) { t += Time.deltaTime; yield return null; }
        }
// Collect relic rewards sequentially
        for (int i = 0; i <= floorSequence.activePacket.relics - 1; i++) {
            Transform r = card.rewardContainer.GetChild(0);
            r.GetComponent<Animator>().SetTrigger("Reward");
            t = 0; while (t < 0.5f) { t += Time.deltaTime; yield return null; }
            r.SetParent(null);
            t = 0; while (t < 0.2f) { t += Time.deltaTime; yield return null; }
            yield return relicManager.StartCoroutine(relicManager.PresentRelic());
            t = 0; while (t < 0.6f) { t += Time.deltaTime; yield return null; }
        }

// Payout bonus objectives
        foreach (Objective ob in floorSequence.activePacket.objectives)
            ob.ProgressCheck(true);

        int objs = floorSequence.activePacket.objectives.Count - 1;
        for (int i = 0; i <= objs; i++) {
            Animator anim = card.bonusObjContainer.GetChild(i).GetComponent<Animator>();
            if (floorSequence.activePacket.objectives[i].succeeded) {
                anim.SetTrigger("ObReward");
                t = 0; while (t < 0.5f) { t += Time.deltaTime; yield return null; }
                LayoutRebuilder.ForceRebuildLayoutImmediate(anim.transform.parent.GetComponent<RectTransform>());
                Canvas.ForceUpdateCanvases();
                if (floorSequence.activePacket.objectives[i].nuggetReward) {
                    upgradeManager.CollectNugget(rndBag.Next());
                } else {
                    t = 0; while (t < 0.2f) { t += Time.deltaTime; yield return null; }
                    yield return relicManager.StartCoroutine(relicManager.PresentRelic());
                }
                t = 0; while (t < 0.6f) { t += Time.deltaTime; yield return null; }
            } else {
                anim.SetTrigger("ObFail");
                t = 0; while (t < 0.3f) { t += Time.deltaTime; yield return null; }
                LayoutRebuilder.ForceRebuildLayoutImmediate(anim.transform.parent.GetComponent<RectTransform>());
                Canvas.ForceUpdateCanvases();
                t = 0; while (t < 0.3f) { t += Time.deltaTime; yield return null; }
            }
            t = 0; while (t < 0.5f) { t += Time.deltaTime; yield return null; }
        }


// Delay for anim out
        t = 0; while (t < 1f) { t += Time.deltaTime; yield return null; }
        card.GetComponent<Animator>().SetTrigger("SlideOut");
        t = 0; while (t < 1f) { t += Time.deltaTime; yield return null; }
        
        pathChoiceContainer.SetActive(false);
        ClearObjectives();
    }

    public void ClearObjectives() {
        foreach (Transform child in pathCardContainer) {
            PathCard card = child.GetComponent<PathCard>();
            card.UnsubObjectives();
        }
        SubscribeTracker();
    }
    
    void SubscribeTracker(List<Objective> objs = null) {
        if (objs != null)
            tracker.AssignObjectives(objs, rewardSprites);
        else {
            tracker.UnsubObjectives();
        }
    }

    public void EndPathSequence() {
        choosingPath = false;
    }

    int packetCount = 0;
    List<Objective> GetObjectives(int count) {
         List<Objective> packetObjectives;
        switch(ScenarioManager.instance.startCavity + packetCount) {
            default:
            case 1: packetObjectives = packetIObjectives; break;
            case 2: packetObjectives = packetIIObjectives; break;
            case 3: packetObjectives = packetIIIObjectives; break;
        }

// Randomly assign objectives
        ShuffleBag<Objective> rndBag = new();
        List<Objective> rolledObjectives = new();
        for (int i = packetObjectives.Count - 1; i >= 0; i--)
            rndBag.Add(packetObjectives[i]);
        for (int c = 0; c <= count; c++){ 
            rolledObjectives.Add(rndBag.Next());
        }
        packetCount++;
        return rolledObjectives;
    }

    public void SelectCard(PathCard selected) {
        if (selectedPathCard)
            selectedPathCard.StartCoroutine(selectedPathCard.ExpandAnimation(false));
        if (selectedPathCard != selected) {
            selectedPathCard = selected;
            selectedPathCard.StartCoroutine(selectedPathCard.ExpandAnimation(true));
        } else 
            selectedPathCard = null;
    }
}
