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
        reviewingObjectives = true;
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

// Particles collected, sent to upgrade manager
        List<SlagEquipmentData.UpgradePath> particles = new();
        foreach (Objective ob in activeObjectives) {
            if (ob.succeeded) particles.Add(ob.reward);
        }

        assignAwardPanel.SetActive(false);
        packetCount++;
    }

    public IEnumerator AssignSequence() {
        reviewingObjectives = true;
        if (!ScenarioManager.instance.gpOptional.objectivesEncountered)
            yield return ScenarioManager.instance.gpOptional.StartCoroutine(ScenarioManager.instance.gpOptional.Objectives());
        assignAwardPanel.SetActive(true);
        continueButton.gameObject.SetActive(true);
        //continueButton.GetComponentInChildren<TMPro.TMP_Text>().text = "ACCEPT OBJECTIVES";
        rerollButton.gameObject.SetActive(true);

        StartCoroutine(RerollObjectives());

        while (reviewingObjectives)
            yield return null;

        assignAwardPanel.SetActive(false);
    }

    public void Reroll() {
        StartCoroutine(RerollObjectives());
    }

    IEnumerator RerollObjectives() {
       activeObjectives = new();

        List<Objective> packetObjectives;
        switch(ScenarioManager.instance.startCavity + packetCount) {
            default:
            case 1:
                packetObjectives = packetIObjectives;
            break;
            case 2:
                packetObjectives = packetIIObjectives;
            break;
            case 3:
                packetObjectives = packetIIIObjectives;
            break;
        }

// Randomly assign objectives
        ShuffleBag<Objective> rndBag = new();
        for (int i = packetObjectives.Count - 1; i >= 0; i--)
            rndBag.Add(packetObjectives[i]);
        for (int u = 0; u <= 2; u++)
            activeObjectives.Add(rndBag.Next());
        
// Create UI cards
        for (int i = objectiveCardParent.transform.childCount - 1; i >= 0; i--) 
            objectiveCardParent.transform.GetChild(i).GetComponent<ObjectiveCard>().Unsub();
        
        objectiveCardParent.GetComponent<HorizontalLayoutGroup>().enabled = true;
        foreach(Objective ob in activeObjectives) {
            ob.Init();
            Instantiate(objectiveCardPrefab, objectiveCardParent.transform);
        }
        
        tracker.AssignObjectives(activeObjectives, rewardSprites);
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(tracker.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(objectiveCardParent.GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();

        yield return null;
        objectiveCardParent.GetComponent<HorizontalLayoutGroup>().enabled = false;

// Init UI card animations
        for (int o = 0; o <= activeObjectives.Count - 1; o++) {
            objectiveCardParent.transform.GetChild(o).GetComponent<ObjectiveCard>().Init(activeObjectives[o]);
            float t = 0;
            while (t < 0.25f) {
                t += Time.deltaTime;
                yield return null;
            }
        }


    }

    public IEnumerator CollectRewards() {
        float timer = 0;
        while (timer <= 1) {

            yield return null;
            timer += Time.deltaTime;
        }
    }

    public void EndObjectiveSequence() {
        reviewingObjectives = false;
    }

}
