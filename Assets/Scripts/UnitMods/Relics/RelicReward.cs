using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RelicReward : MonoBehaviour {

    Relics.RelicData data;
    [SerializeField] Image image;
    [SerializeField] TMP_Text nameTMP, descriptionTMP;

    public delegate void OnRelicReward(Relics.RelicData relic);
    public virtual event OnRelicReward OnCollectRelic;
    public virtual event OnRelicReward OnScrapRelic;

    bool deciding;
    public IEnumerator RewardSequence(Relics.RelicData relic) {
        deciding = true;
        data = relic;

        transform.GetChild(0).gameObject.SetActive(true);

        image.sprite = relic.sprite;
        nameTMP.text = relic.name;
        descriptionTMP.text = relic.description;

        while (deciding) { yield return null; }
        float t = 0; while (t < 1) { yield return null; t += Time.deltaTime; }

        transform.GetChild(0).gameObject.SetActive(false);
    }

    public void TakeScrapRelic(bool take) {
        deciding = false;
        if (take) {
            OnCollectRelic?.Invoke(data);
        } else {
            OnScrapRelic?.Invoke(data);
        }
    }
}
