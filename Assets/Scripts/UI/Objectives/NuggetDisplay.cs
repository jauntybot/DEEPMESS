using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NuggetDisplay : MonoBehaviour {

    [SerializeField] Animator anim;
    [SerializeField] TMPro.TMP_Text countTMP;
    [SerializeField] AudioSource audioSource;
    [SerializeField] SFX collectSFX;

    void Start() {
        countTMP.text = ScenarioManager.instance.player.collectedNuggets.ToString();
    }

    public void CollectNugget() {
        anim.SetTrigger("Collect");
        UpdateNuggetCount();
        audioSource.PlayOneShot(collectSFX.Get());
    }

    public void UpdateNuggetCount() {
        countTMP.text = ScenarioManager.instance.player.collectedNuggets.ToString();
    }

}
