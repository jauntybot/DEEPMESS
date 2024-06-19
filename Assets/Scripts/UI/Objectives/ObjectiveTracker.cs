using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveTracker : MonoBehaviour {

    public List<Objective> activeObjectives;
    [SerializeField] GameObject objectiveCardPrefab, objectiveCardParent;



    public void AssignObjectives(List<Objective> _obs) {
        activeObjectives = new(_obs);
        
        for (int i = objectiveCardParent.transform.childCount - 1; i >= 0; i--) {
            objectiveCardParent.transform.GetChild(i).GetComponent<ObjectiveCard>().Unsub();
            Destroy(objectiveCardParent.transform.GetChild(i).gameObject);
        }

        foreach(Objective ob in activeObjectives) {
            ObjectiveCard card = Instantiate(objectiveCardPrefab, objectiveCardParent.transform).GetComponent<ObjectiveCard>();
            card.Init(ob);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(objectiveCardParent.GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
    }

    public void UpdateCards() {
        foreach(Transform child in objectiveCardParent.transform) {
            ObjectiveCard card = child.GetComponent<ObjectiveCard>();
            card.UpdateCard(card.objective);
        }
    }

    public void UnsubObjective(int index) {
        ObjectiveCard card = objectiveCardParent.transform.GetChild(index).GetComponent<ObjectiveCard>();
        card.Unsub();
        
    }


}
