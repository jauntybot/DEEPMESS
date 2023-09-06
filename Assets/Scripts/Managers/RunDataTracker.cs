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

    [SerializeField] TMP_Text resultsTMP, playTimeTMP, floorCountTMP, enemiesCountTMP;
    [SerializeField] List<UnitResults> unitResults;


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

    public void UpdateAndDisplay(bool win, int floors, int enemies) {
        panel.SetActive(true);
        resultsTMP.text = win ? "EXCAVATION COMPLETE" : "EXCAVATION FAILED";
        resultsTMP.color = win ? Color.white : Color.red;

        float minutes = Mathf.FloorToInt(playTime / 60);
        float seconds = Mathf.FloorToInt(playTime % 60);

        playTimeTMP.text = string.Format("{0:00} : {1:00}", minutes, seconds);
        floorCountTMP.text = floors.ToString() + " floors";
        enemiesCountTMP.text = enemies.ToString() + " enemies";

        int i = 0;
        foreach (Unit u in scenario.player.units) {
            if (u is PlayerUnit || u is Nail) {
                unitResults[i].Init(u);
                i++;
            }
        }

    }

    public void RestartRun() {
        scenario.scenario = ScenarioManager.Scenario.Null;
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
