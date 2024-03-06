using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveManager : MonoBehaviour {

    [SerializeField] ObjectiveTracker tracker;
    [SerializeField] UpgradeManager upgrade;
    int packetCount = 0;

    [SerializeField] GameObject assignAwardPanel;
    [SerializeField] GameObject objectiveCardParent, objectiveCardPrefab;

    [SerializeField] Button rerollButton, continueButton;

    [Header("Serialized Objective Pools")]
    [SerializeField] List<Objective> packetIObjectives;
    [SerializeField] List<Objective> packetIIObjectives, packetIIIObjectives;
    [SerializeField] List<Sprite> rewardSprites;
    public List<Objective> activeObjectives = new();

    bool reviewingObjectives;

    #region Singleton (and Awake)

    public static ObjectiveManager instance;
    private void Awake() {
        if (ObjectiveManager.instance) {
            Debug.Log("Warning! More than one instance of ObjectiveManager found!");
            return;
        }
        ObjectiveManager.instance = this;
        packetCount = 0;
    }
    #endregion

    public IEnumerator RewardSequence() {
        foreach(Objective ob in activeObjectives)
            ob.ProgressCheck(true);

        assignAwardPanel.SetActive(true);
        continueButton.gameObject.SetActive(false);
        rerollButton.gameObject.SetActive(false);

        float t = 0;
        foreach(Transform child in objectiveCardParent.transform) {
            Animator anim = child.GetComponent<Animator>();
            Objective ob = child.GetComponent<ObjectiveCard>().objective;
            int c = 0;
            switch (ob.reward) {
                default:
                case SlagEquipmentData.UpgradePath.Shunt: break;
                case SlagEquipmentData.UpgradePath.Scab: c = 1; break;
                case SlagEquipmentData.UpgradePath.Sludge: c = 2; break;
            }
            anim.SetInteger("Color", c);
            anim.SetTrigger("SlideIn");
            anim.SetTrigger("Resolved");
            anim.SetBool("Success", ob.succeeded);

// Particles collected, sent to upgrade manager
            if (ob.succeeded) upgrade.CollectNugget(ob.reward);
            
            t = 0;
            while (t < 1.25f) {
                t += Time.deltaTime;
                yield return null;
            }
            Debug.Log("Card Done");
        }

        t = 0;
        while (t < 1.25f) {
            t += Time.deltaTime;
            yield return null;
        }
        Debug.Log("Review Done");


        assignAwardPanel.SetActive(false);
        packetCount++;
    }

    public List<Objective> GetObjectives(int count) {
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
        return rolledObjectives;
    }


    public void SubscribeTracker(List<Objective> objs = null) {
        if (objs != null)
            tracker.AssignObjectives(objs, rewardSprites);
        else {
            tracker.UnsubObjectives();
        }
    }

    public void EndObjectiveSequence() {
        reviewingObjectives = false;
    }

}
