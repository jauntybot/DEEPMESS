using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.UI;


public class ScratchOffCard : MonoBehaviour {


    UpgradeManager manager;
    Animator anim;
    [SerializeField] Sprite[] bgSprites;
    [SerializeField] GameObject scratchPrefab;
    [SerializeField] Transform scratchLayout;
    List<ScratchUpgrade> scratches;
    [SerializeField] AudioSource scratchAudio;

    public void BuildCard(UpgradeManager m, List<GearUpgrade> upgrades, int cardLvl) {
        manager = m;
        anim = GetComponent<Animator>();
        
        GetComponent<Image>().sprite = bgSprites[cardLvl-1];

        scratches = new();
        if (upgrades.Count / 3 > 1) scratchLayout.GetComponent<GridLayoutGroup>().constraintCount = 2;
        for (int i = 0; i <= upgrades.Count - 1; i++) {
            ScratchUpgrade upgrade = Instantiate(scratchPrefab, scratchLayout).GetComponent<ScratchUpgrade>();
            upgrade.Init(this, upgrades[i]);
            scratches.Add(upgrade);

        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(scratchLayout.GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
    }

    public void ScratchCard() {
        StartCoroutine(ScratchAnim());
    }

    public IEnumerator ScratchAnim() {
        scratchAudio.Play();
        for (int i = 0; i <= scratchLayout.childCount - 1; i++) {
            yield return new WaitForSecondsRealtime(0.25f);
            ScratchUpgrade upgrade = scratchLayout.GetChild(i).GetComponent<ScratchUpgrade>();
            upgrade.ScratchOff();
        }
        yield return new WaitForSecondsRealtime(0.7f);
        RectTransform rect = GetComponent<RectTransform>();
        
        
        float timer = 0;
        while (timer <= 0.5f) {
            yield return null;
            rect.anchorMin = new Vector2(Mathf.Lerp(0.5f, 0.2f, timer/0.5f), 0.55f);
            rect.anchorMax = new Vector2(Mathf.Lerp(0.5f, 0.2f, timer/0.5f), 0.55f);
            timer += Time.deltaTime;
        }
    }

    public void SelectUpgrade(ScratchUpgrade upgrade) {
        foreach(ScratchUpgrade scratch in scratches) {
            scratch.selection.SetActive(false);
        }
        upgrade.selection.SetActive(true);

        manager.SelectUpgrade(upgrade.upgrade, upgrade.ttTrigger);

    }

    public void DestroyCard() {
        anim.SetTrigger("Destroy");
    }

}
