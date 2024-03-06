using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PathManager : MonoBehaviour {

    FloorSequence floorSequence;
    ObjectiveManager objectiveManager;
    [SerializeField] UpgradeManager upgradeManager;

    List<PathCard> activeCards;
    [SerializeField] Transform pathCardContainer;
    [SerializeField] GameObject pathCardPrefab, pathChoiceContainer;

    public bool choosingPath = false;

    void Start() {
        floorSequence = FloorManager.instance.floorSequence;
        objectiveManager = ObjectiveManager.instance;
        ClearObjectives();
    }

    public IEnumerator PathSequence() {
        choosingPath = true;
        pathChoiceContainer.SetActive(true);
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
            totalObjectives += randomPackets[i].bonusObjectives;
            activeCards.Add(pc);
        }

// Assign random objectives based on packet, prevents duplicates
        List<Objective> objectives = objectiveManager.GetObjectives(totalObjectives);
        for (int i = 0; i <= activeCards.Count - 1; i++) {
            List<Objective> packetObjs = new();
            for (int o = randomPackets[i].bonusObjectives - 1; o >= 0; o--) {
                packetObjs.Add(objectives[o].Init());
                objectives.RemoveAt(o);
            }
            activeCards[i].AssignObjectives(packetObjs);
        }
        
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(pathCardContainer.GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();

        while (choosingPath)
            yield return null;


        pathChoiceContainer.SetActive(false);
    }

    public void SelectPath(PathCard selected) {
// Destroy other path nodes and unsub objectives
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
        objectiveManager.SubscribeTracker(selected.floorPacket.objectives);
        
        choosingPath = false;
    }

    public IEnumerator PathRewardSequence() {
        foreach (Objective ob in floorSequence.activePacket.objectives)
            ob.ProgressCheck(true);

        pathChoiceContainer.SetActive(true);
        PathCard card = pathCardContainer.GetChild(0).GetComponent<PathCard>();

// Shuffle bag for random nugget rewards
        int nuggets = floorSequence.activePacket.nuggets;
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
        for (int i = nuggets - 1; i >= 0; i--) {
            Destroy(card.rewardContainer.GetChild(i).gameObject);
            t = 0; while (t < 0.25f) { t += Time.deltaTime; yield return null; }
             
            upgradeManager.CollectNugget(rewards[i]);
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
        objectiveManager.SubscribeTracker();
    }

    public void EndPathSequence() {
        choosingPath = false;
    }

}
