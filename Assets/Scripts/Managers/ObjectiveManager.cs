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
    [HideInInspector] public List<int> objectiveIndices;
    public List<Objective> activeObjectives;

    [Header("Serialized Objective Pools")]
    [SerializeField] List<Objective> serializedObjectives;
    ShuffleBag<Objective> objectivePoolC1, objectivePoolC2, objectivePoolC3;

    AudioSource audioSource;
    [SerializeField] SFX ginoSFX;
    [SerializeField] NuggetDisplay nuggets;
    [SerializeField] ObjectiveTracker tracker;

    public void Init(RunData run) {
        scenario = ScenarioManager.instance;
        ClearObjectives();
        objectiveScreen.SetActive(false);
        activeObjectives = new();
        objectiveIndices = new() { 0, 0, 0 };
        objectivePoolC1 = new();
        objectivePoolC2 = new();
        objectivePoolC3 = new();
        foreach (Objective ob in serializedObjectives) {
            if (run != null && run.objectives.ContainsKey(ob.name)) continue;
            else if (ob.chunk == FloorChunk.PacketType.I) objectivePoolC1.Add(ob);
            else if (ob.chunk == FloorChunk.PacketType.II) objectivePoolC2.Add(ob);
            else if (ob.chunk == FloorChunk.PacketType.III) objectivePoolC3.Add(ob);
        }

        audioSource = GetComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = ginoSFX.outputMixerGroup;
        
        if (run != null && run.objectives.Count > 0) { 
            LoadObjectives(run);
        } else 
            tracker.gameObject.SetActive(false);
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

    void LoadObjectives(RunData run) {
        activeObjectives = new List<Objective> {null, null, null};
        int i = 0;
        foreach (KeyValuePair<String, int> entry in run.objectives) {
            Objective o = serializedObjectives.Find(o => o.name == entry.Key);
            if (o != null) {
                o.Init();
                o.progress = entry.Value;
                o.ProgressCheck();
                activeObjectives[i] = o;
                objectiveCards[i].Init(o);
            }
            i++;
        }
        
        SubscribeTracker(activeObjectives);
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
        activeObjectives[index] = rolled;
        objectiveCards[index].Init(rolled);

        LayoutRebuilder.ForceRebuildLayoutImmediate(objectiveCards[0].transform.parent.GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
    }

    public void RollObjectiveCard(ObjectiveBeaconCard card, bool collect) {
        int index = objectiveCards.IndexOf(card);
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

        if (collect) {
            scenario.player.collectedNuggets++;
            nuggets.CollectNugget();
        }

        rolled = rolled.Init();
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
        Debug.Log("clear tracker");
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
