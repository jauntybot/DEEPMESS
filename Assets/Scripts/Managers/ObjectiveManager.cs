using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveManager : MonoBehaviour {

    ScenarioManager scenario;
    
    [SerializeField] GameObject objectiveScreen;
    [SerializeField] Animator ginosAnim;
    [SerializeField] List<ObjectiveBeaconCard> objectiveCards;
    List<int> objectiveIndices;
    [SerializeField] List<Objective> activeObjectives;

    [Header("Serialized Objective Pools")]
    [SerializeField] List<Objective> serializedObjectives;
    ShuffleBag<Objective> objectivePoolC1, objectivePoolC2, objectivePoolC3;

    AudioSource audioSource;
    [SerializeField] SFX ginoSFX;
    [SerializeField] NuggetDisplay nuggets;
    [SerializeField] ObjectiveTracker tracker;

    void Start() {
        scenario = ScenarioManager.instance;
        ClearObjectives();
        tracker.gameObject.SetActive(false);
        objectiveScreen.SetActive(false);
        activeObjectives = new();
        objectiveIndices = new() { 0, 0, 0 };
        objectivePoolC1 = new();
        objectivePoolC2 = new();
        objectivePoolC3 = new();
        foreach (Objective ob in serializedObjectives) {
            if (ob.chunk == FloorChunk.PacketType.I) objectivePoolC1.Add(ob);
            if (ob.chunk == FloorChunk.PacketType.II) objectivePoolC2.Add(ob);
            if (ob.chunk == FloorChunk.PacketType.III) objectivePoolC3.Add(ob);
        }

        audioSource = GetComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = ginoSFX.outputMixerGroup;
    }

    bool reviewing;
    public IEnumerator ObjectiveSequence(bool beacon) {
        reviewing = true;
        if (beacon && !scenario.gpOptional.objectivesEncountered) yield return scenario.gpOptional.StartCoroutine(scenario.gpOptional.Objectives());
        else if (beacon && !scenario.gpOptional.beaconObjectivesEncountered) yield return scenario.gpOptional.StartCoroutine(scenario.gpOptional.BeaconObjectives());

        nuggets.UpdateNuggetCount();
        nuggets.gameObject.SetActive(true);
        
        audioSource.PlayOneShot(ginoSFX.Get());

        if (activeObjectives.Count == 0) {
            activeObjectives = new List<Objective> {null, null, null};
            for (int i = 0; i <= objectiveCards.Count - 1; i++) {
                objectiveCards[i].Reroll();
                objectiveCards[i].DisableButton();
            }
            objectiveScreen.SetActive(true);
        } else {
            objectiveScreen.SetActive(true);
            yield return new WaitForSecondsRealtime(0.25f);
            for (int i = 0; i <= activeObjectives.Count - 1; i++) {
                objectiveCards[i].UpdateCard(objectiveCards[i].objective);
            }
        }

        while (reviewing) yield return null;

        if (!beacon) nuggets.gameObject.SetActive(false);

        SubscribeTracker(activeObjectives);
        ginosAnim.SetTrigger("Disappear");
    }

    public void RollObjectiveCard(int index) {
        Objective rolled;
        objectiveIndices[index]++;

        ShuffleBag<Objective> bag = objectivePoolC1;
        if (objectiveIndices[index] > 1 && objectiveIndices[index] <= 3) bag = objectivePoolC2; 
        else if (objectiveIndices[index] > 3) bag = objectivePoolC3; 
        
        rolled = bag.Next(); 
        bag.Remove(rolled); 

        if (bag.Count == 0) {
            foreach (ObjectiveBeaconCard c in objectiveCards) c.DisableButton();
        }

        rolled = rolled.Init();
        objectiveCards[index].Unsub();
        activeObjectives[index] = rolled;
        objectiveCards[index].Init(rolled);
    }

    public void RollObjectiveCard(ObjectiveBeaconCard card, bool collect) {
        Objective rolled;
        ShuffleBag<Objective> bag;
        switch(scenario.floorManager.floorSequence.currentThreshold) {
            default:
            case FloorChunk.PacketType.I: bag = objectivePoolC1; break;
            case FloorChunk.PacketType.II: bag = objectivePoolC2; break;
        }
        rolled = bag.Next(); 
        //bag.Remove(rolled); 
        if (bag.Count == 0) {
            foreach (ObjectiveBeaconCard c in objectiveCards) c.DisableButton();
        }

        if (collect) {
            scenario.player.collectedNuggets++;
            nuggets.CollectNugget();
        }

        rolled = rolled.Init();
        card.Unsub();
        int index = objectiveCards.IndexOf(card);
        activeObjectives[index] = rolled;
        objectiveCards[index].Init(rolled);

        LayoutRebuilder.ForceRebuildLayoutImmediate(objectiveCards[0].transform.parent.GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
    }
    
    public void ClearObjectives() {
        foreach (ObjectiveCard card in objectiveCards) {
            card.Unsub();
        }
        activeObjectives = new();
        SubscribeTracker();
    }

        
    void SubscribeTracker(List<Objective> objs = null) {
        if (objs != null) {
            tracker.gameObject.SetActive(true);
            tracker.AssignObjectives(objs);
        }
        else {
            for (int i = 0; i <= tracker.activeObjectives.Count - 1; i++) {
                tracker.UnsubObjective(i);
            }
            tracker.gameObject.SetActive(false);
        }
    }

    public void EndObjectiveSequence() {
        reviewing = false;
    }

}
