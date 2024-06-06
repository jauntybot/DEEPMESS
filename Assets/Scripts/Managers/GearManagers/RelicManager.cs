using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
        [SerializeField] GameObject relicPrefab, onFloorRelicPrefab;
        public List<RelicData> serializedRelics;
        public ShuffleBag<RelicData> relicPool;
        public List<Relic> collectedRelics;
        public int scrapValue;
        [SerializeField] bool queuing = false;
        [SerializeField] Queue<IEnumerator> relicQueue;
        Queue<Animator> onFloorQueue;

        void Start() {
            ClearRelics();
            relicPool = new ShuffleBag<RelicData>(serializedRelics.ToArray());
            scrapValue = 0;

            relicQueue = new();
            onFloorQueue = new();
            queuing = false;
        }
        public void QueueRelicReward(GridElement origin) {
            relicQueue.Enqueue(PresentRelicWhenIdle());
            
            GameObject go = Instantiate(onFloorRelicPrefab, origin.grid.neutralGEContainer.transform);
            go.transform.position = origin.grid.PosFromCoord(origin.coord);
            go.GetComponentInChildren<SpriteRenderer>().sortingOrder = origin.grid.SortOrderFromCoord(origin.coord);
            onFloorQueue.Enqueue(go.GetComponentInChildren<Animator>());

            if (!queuing)
                StartCoroutine(relicQueue.Dequeue());
        }
        
        public IEnumerator PresentRelicWhenIdle() {
            queuing = true;
            bool turn = false;
            while (ScenarioManager.instance.currentTurn != ScenarioManager.Turn.Player) {
                turn = true;
                yield return null;
            }
            if (turn) yield return new WaitForSecondsRealtime(1.5f);
            
            yield return new WaitForSecondsRealtime(0.2f);
            while (ScenarioManager.instance.player.unitActing) yield return null;
            
            queuing = false;

            onFloorQueue.Dequeue().SetTrigger("Collect");
            yield return new WaitForSecondsRealtime(1.4f);
            yield return StartCoroutine(PresentRelic());
            if (relicQueue.Count > 0) StartCoroutine(relicQueue.Dequeue());
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

