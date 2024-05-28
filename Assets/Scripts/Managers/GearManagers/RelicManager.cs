using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Relics {
    
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
        public List<RelicData> serializedRelics;
        public ShuffleBag<RelicData> relicPool;
        public List<Relic> collectedRelics;
        public int scrapValue;

        void Start() {
            ClearRelics();
            relicPool = new ShuffleBag<RelicData>(serializedRelics.ToArray());
            scrapValue = 0;
        }

        public IEnumerator PresentRelic(RelicData data = null) {
            if (data == null)
                data = relicPool.Next();
            else if (relicPool.Contains(data)) relicPool.Remove(data);
            
            yield return reward.StartCoroutine(reward.RewardSequence(data));
            
            if (reward.take)
                CollectRelic(data);
            else    
                ScrapRelic(data);
        }

        public void CollectRelic(RelicData data) {
            Relic relic = Instantiate(relicPrefab, relicContainer).GetComponent<Relic>();
            relic.Init(data);
            collectedRelics.Add(relic);

        }

        public void ScrapRelic(RelicData relic) {
            scrapValue += relic.scrapValue;
        }

        public void ClearRelics() {
            for (int r = collectedRelics.Count - 1; r >= 0; r--) {
                collectedRelics[r].ScrapRelic();
                collectedRelics.RemoveAt(r);
            }
        }

        public void GiveAllRelics() {
            foreach (RelicData relic in serializedRelics) {
                CollectRelic(relic);
            }
        }

    }
    
}

