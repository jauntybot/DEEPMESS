using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelicManager : MonoBehaviour {

    public static RelicManager instance;
    private void Awake() {
        if (RelicManager.instance) {
            Debug.LogWarning("Warning! More than one instance of RelicManager found.");
            Destroy(gameObject);
        } 
        instance = this;
    }

    [SerializeField] RelicReward reward;
    [SerializeField] Transform relicContainer;
    [SerializeField] GameObject relicPrefab;
    [SerializeField] List<Relics.RelicData> relicPool;
    public List<Relic> collectedRelics;

    void Start() {
        reward.OnCollectRelic += CollectRelic;
        reward.OnScrapRelic += ScrapRelic;
    }

    
    public IEnumerator PresentRelic() {
        yield return reward.StartCoroutine(reward.RewardSequence(relicPool[0]));
    }

    public void CollectRelic(Relics.RelicData data) {
        Relic relic = Instantiate(relicPrefab, relicContainer).GetComponent<Relic>();
        relic.Init(data);
        collectedRelics.Add(relic);

    }

    public void ScrapRelic(Relics.RelicData relic) {

    }
}
