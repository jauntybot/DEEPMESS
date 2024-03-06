using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelicManager : MonoBehaviour {


    [SerializeField] Transform relicContainer;
    [SerializeField] GameObject relicPrefab;
    public List<Relic> relics;


    public void AcquireRelic(RelicData data) {
        Relic relic = Instantiate(relicPrefab, relicContainer).GetComponent<Relic>();
        relic.Init(data);
        relics.Add(relic);

    }

    public void DestroyRelic(RelicData relic) {

    }

}
