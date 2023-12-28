using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveManager : MonoBehaviour {

    [SerializeField] ObjectiveTracker tracker;
    [SerializeField] UpgradeManager upgrade;

    [SerializeField] GameObject assignAwardPanel;
    [SerializeField] GameObject objectiveCardParent, objectiveCardPrefab;

    [SerializeField] Button continueButton;

    [SerializeField] List<Objective> possibleObjectives;
    [SerializeField] List<Sprite> possibleRewardSprites;
    public List<Objective> activeObjectives = new();

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
        
        activeObjectives.Add(possibleObjectives[0]);
        activeObjectives.Add(possibleObjectives[1]);

        tracker.AssignObjectives(activeObjectives);

        for (int i = objectiveCardParent.transform.childCount - 1; i >= 0; i--)
            Destroy(objectiveCardParent.transform.GetChild(i).gameObject);
        
        foreach(Objective ob in activeObjectives) {
            ob.Init();
            ObjectiveCard obUI = Instantiate(objectiveCardPrefab, objectiveCardParent.transform).GetComponent<ObjectiveCard>();
            obUI.Init(ob);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(objectiveCardParent.GetComponent<RectTransform>());

        while (reviewingObjectives)
            yield return null;

        assignAwardPanel.SetActive(false);
    }

    public IEnumerator RewardSequence() {
        reviewingObjectives = true;
        assignAwardPanel.SetActive(true);

        foreach(Objective ob in activeObjectives)
            ob.ProgressCheck(true);


        while (reviewingObjectives)
            yield return null;

        assignAwardPanel.SetActive(false);
    }

    public void EndObjectiveSequence() {
        reviewingObjectives = false;
    }

}
