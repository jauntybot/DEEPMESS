using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveManager : MonoBehaviour {

    [SerializeField] GameObject assignAwardPanel;
    [SerializeField] GameObject objectiveUIParent, objectiveUIPrefab;

    [SerializeField] Button continueButton;

    [SerializeField] List<Objective> possibleObjectives;
    [SerializeField] List<Sprite> possibleRewardSprites;
    public Dictionary<Objective, bool> activeObjectives = new();

    bool reviewingObjectives;

    #region Singleton (and Awake)

    public static ObjectiveManager instance;
    private void Awake() {
        if (ObjectiveManager.instance) {
            Debug.Log("Warning! More than one instance of ObjectiveManager found!");
            return;
        }
        ObjectiveManager.instance = this;
    }
    #endregion

    public IEnumerator AssignSequence() {
        reviewingObjectives = true;
        assignAwardPanel.SetActive(true);
        activeObjectives = new();
        
        activeObjectives.Add(possibleObjectives[0], false);
        activeObjectives.Add(possibleObjectives[1], false);

        
        for (int i = objectiveUIParent.transform.childCount - 1; i >= 0; i--)
            Destroy(objectiveUIParent.transform.GetChild(i).gameObject);
        
        foreach(KeyValuePair<Objective,bool> entry in activeObjectives) {
            entry.Key.resolved = false;
            entry.Key.ObjectiveSuccessCallback += ObjectiveCompleted;
            entry.Key.ObjectiveFailureCallback += ObjectiveFailed;
            ObjectiveUI ob = Instantiate(objectiveUIPrefab, objectiveUIParent.transform).GetComponent<ObjectiveUI>();
            ob.Init(entry.Key.objectiveString, entry.Key.goal, possibleRewardSprites[0]);
        }

        Canvas.ForceUpdateCanvases();

        while (reviewingObjectives)
            yield return null;
        assignAwardPanel.SetActive(false);
    }

    public IEnumerator RewardSequence() {
        reviewingObjectives = true;
        assignAwardPanel.SetActive(true);

        foreach(KeyValuePair<Objective, bool> entry in activeObjectives)
            entry.Key.ProgressCheck(true);


        while (reviewingObjectives)
            yield return null;
        assignAwardPanel.SetActive(false);
    }


    void ObjectiveCompleted(Objective objective) {

    }

    void ObjectiveFailed(Objective objective) {

    }

    public void EndObjectiveSequence() {
        reviewingObjectives = false;
    }

}
