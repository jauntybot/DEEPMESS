using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class RunDataTracker : MonoBehaviour {

    ScenarioManager scenario;
    public GameObject panel;

    // float playTime;
    // bool runInProgress;

    [SerializeField] GameObject floorRow, enemiesRow, scrapRow, cutRow;

    [SerializeField] TMP_Text resultsTMP, floorsCountUp, floorsMultCountUp, enemiesCountUp, enemiesMultCountUp, scrapCountUp, cutPercentCountUp, cutCountUp, totalCountUp;


    public void Init(ScenarioManager scen) {
        scenario = scen;
        //StartCoroutine(TrackTime());

    }

    // IEnumerator TrackTime() {
    //     playTime = 0;
    //     runInProgress = true;

    //     while (runInProgress) {
    //         yield return null;
    //         playTime += Time.deltaTime;
    //     }
    // }

    public IEnumerator UpdateAndDisplay(bool win, int floors, int enemies, int scrap) {
        panel.SetActive(true);
        if (win) {
            resultsTMP.text = "EXCAVATION COMPLETE";
            resultsTMP.color = Color.white;
            //cutRow.SetActive(false);
            cutPercentCountUp.gameObject.SetActive(false);
            cutCountUp.gameObject.SetActive(false);
        } else {
            resultsTMP.text = "EXCAVATION FAILED";
            resultsTMP.color = Color.red;
            //cutRow.SetActive(true);
            cutPercentCountUp.gameObject.SetActive(true);
            cutCountUp.gameObject.SetActive(true);
        }

        // float minutes = Mathf.FloorToInt(playTime / 60);
        // float seconds = Mathf.FloorToInt(playTime % 60);
        //playTimeTMP.text = string.Format("{0:00} : {1:00}", minutes, seconds);

        float t = 0; while (t <= 0.25f) { t += Time.deltaTime; yield return null; }
        floorRow.SetActive(true);
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(floorRow.GetComponentInParent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(floorRow.transform.parent.GetComponentInParent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(floorRow.transform.parent.transform.parent.GetComponentInParent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
        yield return null;
        StartCoroutine(StringCountUp.CountUp(floors, 1f, (countUp) => { 
            floorsCountUp.text = countUp;
        }));
        yield return StartCoroutine(StringCountUp.CountUp(floors * 10, 0.75f, (countUp) => { 
            floorsMultCountUp.text = countUp;
        }));
        t = 0; while (t <= 0.25f) { t += Time.deltaTime; yield return null; }
        enemiesRow.SetActive(true);
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(enemiesRow.GetComponentInParent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(enemiesRow.transform.parent.GetComponentInParent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(enemiesRow.transform.parent.transform.parent.GetComponentInParent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
        yield return null;
        StartCoroutine(StringCountUp.CountUp(enemies, 0.75f, (countUp) => { 
            enemiesCountUp.text = countUp;
        }));
        yield return StartCoroutine(StringCountUp.CountUp(enemies * 5, 0.75f, (countUp) => { 
            enemiesMultCountUp.text = countUp;
        }));
        t = 0; while (t <= 0.25f) { t += Time.deltaTime; yield return null; }
        scrapRow.SetActive(true);
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrapRow.GetComponentInParent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrapRow.transform.parent.GetComponentInParent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrapRow.transform.parent.transform.parent.GetComponentInParent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
        yield return null;
        yield return StartCoroutine(StringCountUp.CountUp(scrap, 0.75f, (countUp) => { 
            scrapCountUp.text = countUp;
        }));
        t = 0; while (t <= 0.25f) { t += Time.deltaTime; yield return null; }
        
        int total = (floors * 10) + (enemies * 5) + scrap;
        if (!win) {
            cutRow.SetActive(true);
            yield return null;
            LayoutRebuilder.ForceRebuildLayoutImmediate(cutRow.GetComponentInParent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(cutRow.transform.parent.GetComponentInParent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(cutRow.transform.parent.transform.parent.GetComponentInParent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            Canvas.ForceUpdateCanvases();
            yield return null;
            StartCoroutine(StringCountUp.CountUp(-20, 0.75f, (countUp) => { 
                cutPercentCountUp.text = countUp + "%";
            }));
            yield return StartCoroutine(StringCountUp.CountUp(-Mathf.RoundToInt(total*0.2f), 0.75f, (countUp) => { 
                cutCountUp.text = countUp;
            }));
        }
        
        t = 0; while (t <= 0.25f) { t += Time.deltaTime; yield return null; }
        yield return StartCoroutine(StringCountUp.CountUp(total + (win ? 0 : (int)(-total*0.2f)), 0.75f, (countUp) => { 
            totalCountUp.text = countUp;
        }));
        PersistentMenu.instance.upcomingCurrency = total + (win ? 0 : (int)(-total*0.2f));
    }


    public void RestartRun() {
        scenario.scenario = ScenarioManager.Scenario.Null;
        if (ScenarioManager.instance.startCavity == 0) ScenarioManager.instance.startCavity = 1;
        StartCoroutine(FadeToScene(1));
    }

    public void MainMenu() {
        scenario.scenario = ScenarioManager.Scenario.Null;
        StartCoroutine(FadeToScene(0));
    }


    public IEnumerator FadeToScene(int index) {
        yield return new WaitForSecondsRealtime(0.25f);
        PersistentMenu.instance.FadeToBlack(true);
        if (index == 0) {
            PersistentMenu.instance.musicController.SwitchMusicState(MusicController.MusicState.MainMenu, true);
        }
        yield return new WaitForSecondsRealtime(1f);
        SceneManager.LoadScene(index, LoadSceneMode.Single);
    }

}
