using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectiveCard : MonoBehaviour {

    public Objective objective;
    [SerializeField] protected Animator nuggetAnim;
    [SerializeField] protected TMP_Text objectiveTitle, objectiveText, progressText;


    public virtual void Init(Objective _objective) {
        objective = _objective;
        objective.ObjectiveUpdateCallback += UpdateCard;
        int c = 0;
        switch (_objective.reward) {
            default:
            case SlagEquipmentData.UpgradePath.Shunt: break;
            case SlagEquipmentData.UpgradePath.Scab: c = 1; break;
            case SlagEquipmentData.UpgradePath.Sludge: c = 2; break;
        }
        if (nuggetAnim)
            nuggetAnim.SetInteger("Color", c);
        GetComponent<Animator>().SetInteger("Color", c);
        GetComponent<Animator>().SetTrigger("SlideIn");

        UpdateCard(objective);
    } 

    public virtual void Unsub() {
        if (objective)
            objective.ObjectiveUpdateCallback -= UpdateCard;
        Destroy(gameObject);
    }


    protected virtual void UpdateCard(Objective ob) {
        if (objectiveTitle)
            objectiveTitle.text = objective.objectiveTitleString;
        objectiveText.text = objective.objectiveString;
        if (progressText) {
            progressText.text = "("+objective.progress+"/"+objective.goal+")";
            progressText.fontMaterial.color = Color.white;
        }
        objectiveText.fontMaterial.color = Color.white;
        
    }


}
