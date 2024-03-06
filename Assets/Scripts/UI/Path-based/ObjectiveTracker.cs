using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveTracker : MonoBehaviour {

    List<Objective> activeObjectives;
    [SerializeField] public GameObject objectiveCardPrefab, objectiveCardParent;

    public void AssignObjectives(List<Objective> _obs, List<Sprite> rewardSprites) {
        activeObjectives = _obs;
        
        for (int i = objectiveCardParent.transform.childCount - 1; i >= 0; i--) {
            objectiveCardParent.transform.GetChild(i).GetComponent<ObjectiveCard>().Unsub();
            Destroy(objectiveCardParent.transform.GetChild(i).gameObject);
        }
        
        foreach(Objective ob in activeObjectives) {
            ObjectiveCard card = Instantiate(objectiveCardPrefab, objectiveCardParent.transform).GetComponent<ObjectiveCard>();
            card.Init(ob);
        }
    }

    public void UnsubObjectives() {
        foreach(Transform child in objectiveCardParent.transform) {
            ObjectiveCard card = child.GetComponent<ObjectiveCard>();
            card.Unsub();
        }
    }

}
