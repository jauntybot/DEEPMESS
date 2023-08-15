using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class RunDataTracker : MonoBehaviour
{

    ScenarioManager scenario;
    public GameObject panel;

    float playTime;


    [SerializeField] TMP_Text playTimeTMP, floorCountTMP, enemiesCountTMP;

    bool runInProgress;


    public void Init(ScenarioManager scen) {
        scenario = scen;
        StartCoroutine(TrackTime());

    }

    IEnumerator TrackTime() {
        playTime = 0;
        runInProgress = true;

        while (runInProgress) {
            yield return null;
            playTime += Time.deltaTime;
        }
    }

    public void UpdateAndDisplay() {
        panel.SetActive(true);


    }

    public void RestartRun() {
        scenario.scenario = ScenarioManager.Scenario.Null;
        StartCoroutine(FadeToScene(SceneManager.GetActiveScene().name));
    }

    public void MainMenu() {
        scenario.scenario = ScenarioManager.Scenario.Null;
        StartCoroutine(FadeToScene(null, 0));
    }


    public IEnumerator FadeToScene(string targetScene = null, int index = default) {
        yield return new WaitForSecondsRealtime(1.5f);
        if (targetScene != null)
            SceneManager.LoadScene(targetScene, LoadSceneMode.Single);
        else
            SceneManager.LoadScene(index, LoadSceneMode.Single);

    }

}
