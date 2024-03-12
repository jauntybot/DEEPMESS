using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Relics;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    
    [Header("Serialized Objective Pools")]
    [SerializeField] List<Objective> packetIObjectives;
    [SerializeField] List<Objective> packetIIObjectives, packetIIIObjectives;
    [SerializeField] List<Sprite> rewardSprites;
    public bool choosingPath = false;

// RELIC SYSTEM - DELETE
    public int objectiveDiscount;


    void Start() {
        floorSequence = FloorManager.instance.floorSequence;
        ClearObjectives();
        objectiveDiscount = 0;
    }

    public IEnumerator PathSequence() {
        choosingPath = true;
        pathChoiceContainer.SetActive(true);
        sequenceTitle.text = "CHOOSE A PATH";
        activeCards = new();

        
        for (int i = pathCardContainer.childCount - 1; i >= 0; i--) {
            Destroy(pathCardContainer.GetChild(i).gameObject);
        }

// Draw packets from floor sequence for nodes
        List<FloorPacket> randomPackets = floorSequence.RandomNodes(2);
        
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
        
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(pathCardContainer.GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();

        while (choosingPath)
            yield return null;
        float t = 0; while (t <= 0.15f) { t += Time.deltaTime; yield return null; }
        Transform card = pathCardContainer.GetChild(0);
        Vector2 from = card.localPosition;
        t = 0; while (t <= 0.2f) { 
            card.localPosition = new Vector2(Mathf.Lerp(from.x, 0, t/0.2f), from.y);
            t += Time.deltaTime; 
            yield return null; 
        }
        card.localPosition = new Vector2(0, from.y);
        pathCardContainer.GetComponent<HorizontalLayoutGroup>().enabled = true;
        t = 0; while (t <= 1.25f) { t += Time.deltaTime; yield return null; }


        pathChoiceContainer.SetActive(false);
    }

    public void SelectPath(PathCard selected) {
// Destroy other path nodes and unsub objectives
        pathCardContainer.GetComponent<HorizontalLayoutGroup>().enabled = false;
        for (int i = pathCardContainer.childCount - 1; i >= 0; i--) {
            PathCard card = pathCardContainer.GetChild(i).GetComponent<PathCard>();
            if (card == selected) continue;
            foreach(Transform child in card.bonusObjContainer) {
                BonusObjectiveCard c = child.GetComponent<BonusObjectiveCard>();
                if (c)
                    c.Unsub();
            }
            Destroy(card.gameObject);
        }

// Assign selected path
        floorSequence.StartPacket(selected.floorPacket);
        SubscribeTracker(selected.floorPacket.objectives);
        
        choosingPath = false;
    }

    public IEnumerator PathRewardSequence() {
        foreach (Objective ob in floorSequence.activePacket.objectives)
            ob.ProgressCheck(true);

        pathChoiceContainer.SetActive(true);
        sequenceTitle.text = "PATH RESULTS";
        PathCard card = pathCardContainer.GetChild(0).GetComponent<PathCard>();
        card.subcardContainer.GetChild(0).gameObject.SetActive(false);

// Shuffle bag for random nugget rewards
        int nuggets = floorSequence.activePacket.nuggets + floorSequence.activePacket.bonusNuggetObjectives;
        ShuffleBag<SlagEquipmentData.UpgradePath> rndBag = new();
        List<SlagEquipmentData.UpgradePath> rewards = new();
        for (int i = 0; i <= 1; i++) {
            rndBag.Add(SlagEquipmentData.UpgradePath.Shunt);
            rndBag.Add(SlagEquipmentData.UpgradePath.Scab);
            rndBag.Add(SlagEquipmentData.UpgradePath.Sludge);
        }
        for (int i = 0; i < nuggets; i++)
            rewards.Add(rndBag.Next());

// Delay for anim in
        float t = 0;        
        while (t < 1.25f) {
            t += Time.deltaTime;
            yield return null;
        }

// Collect nugget rewards sequentially
        for (int i = 0; i <= nuggets - 1; i++) {
            Destroy(card.rewardContainer.GetChild(0).gameObject);
            t = 0; while (t < 0.25f) { t += Time.deltaTime; yield return null; } 
            upgradeManager.CollectNugget(rewards[i]);
            t = 0; while (t < 0.25f) { t += Time.deltaTime; yield return null; }
        }
// Collect relic rewards sequentially
        int relics = floorSequence.activePacket.relics;
        for (int i = 0; i <= relics - 1; i++) {
            Destroy(card.rewardContainer.GetChild(0).gameObject);
            t = 0; while (t < 0.25f) { t += Time.deltaTime; yield return null; }
            yield return relicManager.StartCoroutine(relicManager.PresentRelic());
            t = 0; while (t < 0.25f) { t += Time.deltaTime; yield return null; }
        }

// Payout bonus objectives
        foreach (Objective ob in floorSequence.activePacket.objectives)
            ob.ProgressCheck(true);

        int objs = floorSequence.activePacket.objectives.Count - 1;
        for (int i = 0; i <= objs; i++) {
            Destroy(card.bonusObjContainer.GetChild(0).gameObject);
            t = 0; while (t < 0.25f) { t += Time.deltaTime; yield return null; }
            if (floorSequence.activePacket.objectives[i].succeeded) {
                if (floorSequence.activePacket.objectives[i].nuggetReward) {
                    upgradeManager.CollectNugget(rewards[i]);
                } else {
                    yield return relicManager.StartCoroutine(relicManager.PresentRelic());
                }
            }
            t = 0; while (t < 0.25f) { t += Time.deltaTime; yield return null; }
        }


// Delay for anim out
        t = 0;
        while (t < 1.25f) {
            t += Time.deltaTime;
            yield return null;
        }
        
        pathChoiceContainer.SetActive(false);
        ClearObjectives();
    }

    public void ClearObjectives() {
        foreach (Transform child in pathCardContainer) {
            PathCard card = child.GetComponent<PathCard>();
            card.UnsubObjectives();
        }
        ObjectiveEventManager.Clear();
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
