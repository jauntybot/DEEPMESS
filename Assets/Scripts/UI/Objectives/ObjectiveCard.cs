using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectiveCard : MonoBehaviour {

    public Objective objective;
    [SerializeField] protected Animator nuggetAnim;
    [SerializeField] TMP_Text objectiveTitle, objectiveText, progressText;


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


    protected void UpdateCard(Objective ob) {
        objectiveTitle.text = objective.objectiveTitleString;
        objectiveText.text = objective.objectiveString;
        progressText.text = "("+objective.progress+"/"+objective.goal+")";
        objectiveText.fontMaterial.color = Color.white;
        progressText.fontMaterial.color = Color.white;
        
        TMPro.FontStyles style = TMPro.FontStyles.Normal;
        if (ob.resolved) 
            style = TMPro.FontStyles.Strikethrough;
        Color color = Color.black;
        Color title = Color.white;
        if (ob.resolved) {
            if (ob.succeeded) color = Color.green;
            else color = Color.red;
        }
        if (color != Color.black) title = color;

        //objectiveTitle.fontStyle = style;
        //objectiveText.fontStyle = style;
        //progressText.fontStyle = style;
        //objectiveTitle.color = title;
        //objectiveText.color = color;
        //progressText.color = color;
    }


}
