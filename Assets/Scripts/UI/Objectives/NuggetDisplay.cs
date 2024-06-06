using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NuggetDisplay : MonoBehaviour {

    [SerializeField] Animator anim;
    [SerializeField] TMPro.TMP_Text countTMP;

    void Start() {
        countTMP.text = ScenarioManager.instance.player.collectedNuggets.ToString();
    }

    public void CollectNugget() {
        anim.SetTrigger("Collect");
        UpdateNuggetCount();
    }

    public void UpdateNuggetCount() {
        countTMP.text = ScenarioManager.instance.player.collectedNuggets.ToString();
    }

}
