using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour {

    ScenarioManager scenario;
    
    [SerializeField] GameObject objectiveScreen;
    [SerializeField] List<ObjectiveCard> objectiveCards;

    [Header("Serialized Objective Pools")]
    [SerializeField] List<Objective> serializedObjectives;
    ShuffleBag<Objective> objectivePoolC1, objectivePoolC2;

    List<Objective> activeObjectives;

            
    [SerializeField] ObjectiveTracker tracker;

    void Start() {
        scenario = ScenarioManager.instance;
        ClearObjectives();
        objectiveScreen.SetActive(false);
        activeObjectives = new();
        foreach (Objective ob in serializedObjectives) {
            if (ob.chunk == FloorChunk.PacketType.I) objectivePoolC1.Add(ob);
            if (ob.chunk == FloorChunk.PacketType.II) objectivePoolC2.Add(ob);
        }
    }

    bool reviewing;
    public IEnumerator ObjectiveSequence() {
        reviewing = true;
        if (activeObjectives.Count == 0) {
            for (int i = 0; i <= activeObjectives.Count - 1; i++) {
                RollObjectiveCard(i, scenario.floorManager.floorSequence.currentThreshold);
            }
        }
        
        objectiveScreen.SetActive(true);


        while (reviewing) yield return null;

        SubscribeTracker(activeObjectives);
        objectiveScreen.SetActive(false);
    }

    public void RollObjectiveCard(int index, FloorChunk.PacketType chunk) {
        Objective rolled;
        switch(chunk) {
            default:
            case FloorChunk.PacketType.I: rolled = objectivePoolC1.Next(); break;
            case FloorChunk.PacketType.II: rolled = objectivePoolC2.Next(); break;
        }
        objectiveCards[index].Init(rolled);
    }
    
    public void ClearObjectives() {
        foreach (ObjectiveCard card in objectiveCards) {
            card.Unsub();
        }
        activeObjectives = new();
        SubscribeTracker();
    }

        
    void SubscribeTracker(List<Objective> objs = null) {
        if (objs != null)
            tracker.AssignObjectives(objs);
        else {
            tracker.UnsubObjectives();
        }
    }

    public void EndObjectiveSequence() {
        reviewing = false;
    }

}
