using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveTracker : MonoBehaviour {

    List<Objective> activeObjectives;
    [SerializeField] GameObject objectiveCardPrefab, objectiveCardParent;

    public void AssignObjectives(List<Objective> _obs) {
        activeObjectives = _obs;
        
        for (int i = objectiveCardParent.transform.childCount - 1; i >= 0; i--)
            Destroy(objectiveCardParent.transform.GetChild(i).gameObject);
        
        foreach(Objective ob in activeObjectives) {
            ob.Init();
            ObjectiveCard obUI = Instantiate(objectiveCardPrefab, objectiveCardParent.transform).GetComponent<ObjectiveCard>();
            obUI.Init(ob);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(objectiveCardParent.GetComponent<RectTransform>());
    }

}
