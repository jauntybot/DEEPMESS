using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RelicReward : MonoBehaviour {

    Relics.RelicData data;
    [SerializeField] Image image;
    [SerializeField] TMP_Text nameTMP, descriptionTMP;
    
    public bool take;
    bool deciding;

    public IEnumerator RewardSequence(Relics.RelicData relic) {
        deciding = true;
        data = relic;

        transform.GetChild(0).gameObject.SetActive(true);
        GetComponent<AudioSource>().Play();

        image.sprite = relic.sprite;
        nameTMP.text = relic.name;
        descriptionTMP.text = relic.description;

        while (deciding) { yield return null; }
        float t = 0; while (t < 0.65f) { yield return null; t += Time.deltaTime; }

        transform.GetChild(0).gameObject.SetActive(false);
    }

    public void TakeScrapRelic(bool _take) {
        deciding = false;
        take = _take;
    }
}
