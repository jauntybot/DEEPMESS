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

    [SerializeField] GameObject floorRow, enemiesRow, tasksRow, godThoughtRow, crushesRow, quitButton;
    [SerializeField] Animator totalAnim;

    [SerializeField] TMP_Text resultsTMP, floorsCountUp, floorsMultCountUp, enemiesCountUp, enemiesMultCountUp, tasksCountUp, tasksMultCountUp, godThoughtCountUp, godThoughtMultCountUp, crushesCountUp, crushesMultCountUp, totalCountUp, highScoreCountUp;


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

    public IEnumerator UpdateAndDisplay(bool win, int floors, int enemies, int thoughts, int tasks, int crushes) {
        floorRow.SetActive(false); enemiesRow.SetActive(false); tasksRow.SetActive(false); godThoughtRow.SetActive(false); crushesRow.SetActive(false); quitButton.SetActive(false);
        LayoutRebuilder.ForceRebuildLayoutImmediate(floorRow.GetComponentInParent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(floorRow.transform.parent.GetComponentInParent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(floorRow.transform.parent.transform.parent.GetComponentInParent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());

        int total = (floors * 50) + (enemies * 25) + (tasks * 150) + (thoughts * 100) + (crushes * 200);
        PersistentMenu.instance.upcomingCurrency = total + (win ? 0 : (int)(-total*0.2f));
        panel.SetActive(true);
        if (win) {
            resultsTMP.text = "Excavation Complete";
            resultsTMP.color = Color.white;
        } else {
            resultsTMP.text = "Excavation Failed";
            resultsTMP.color = Color.red;
        }

        // float minutes = Mathf.FloorToInt(playTime / 60);
        // float seconds = Mathf.FloorToInt(playTime % 60);
        //playTimeTMP.text = string.Format("{0:00} : {1:00}", minutes, seconds);

        float t = 0; while (t <= 0.5f) { t += Time.deltaTime; yield return null; }
        enemiesRow.SetActive(true);
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(enemiesRow.GetComponentInParent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(enemiesRow.transform.parent.GetComponentInParent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(enemiesRow.transform.parent.transform.parent.GetComponentInParent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
        
        StartCoroutine(StringCountUp.CountUp(enemies, 0.75f, (countUp) => { 
            enemiesCountUp.text = countUp;
        }));
        StartCoroutine(StringCountUp.CountUp(enemies * 25, 0.75f, (countUp) => { 
            enemiesMultCountUp.text = countUp;
        }));
        t = 0; while (t <= 0.125f) { t += Time.deltaTime; yield return null; }
        floorRow.SetActive(true);
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(floorRow.GetComponentInParent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(floorRow.transform.parent.GetComponentInParent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(floorRow.transform.parent.transform.parent.GetComponentInParent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
        
        StartCoroutine(StringCountUp.CountUp(floors, 1f, (countUp) => { 
            floorsCountUp.text = countUp;
        }));
        StartCoroutine(StringCountUp.CountUp(floors * 50, 0.75f, (countUp) => { 
            floorsMultCountUp.text = countUp;
        }));
        t = 0; while (t <= 0.125f) { t += Time.deltaTime; yield return null; }
        godThoughtRow.SetActive(true);
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(godThoughtRow.GetComponentInParent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(godThoughtRow.transform.parent.GetComponentInParent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(godThoughtRow.transform.parent.transform.parent.GetComponentInParent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();

        StartCoroutine(StringCountUp.CountUp(thoughts, 0.75f, (countUp) => { 
            godThoughtCountUp.text = countUp;
        }));
        StartCoroutine(StringCountUp.CountUp(thoughts * 100, 0.75f, (countUp) => { 
            godThoughtMultCountUp.text = countUp;
        }));
        t = 0; while (t <= 0.125f) { t += Time.deltaTime; yield return null; }
        tasksRow.SetActive(true);
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(tasksRow.GetComponentInParent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(tasksRow.transform.parent.GetComponentInParent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(tasksRow.transform.parent.transform.parent.GetComponentInParent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
        
        StartCoroutine(StringCountUp.CountUp(tasks, 0.75f, (countUp) => { 
            tasksCountUp.text = countUp;
        }));
        StartCoroutine(StringCountUp.CountUp(tasks * 150, 0.75f, (countUp) => { 
            tasksMultCountUp.text = countUp;
        }));
        t = 0; while (t <= 0.125f) { t += Time.deltaTime; yield return null; }
        crushesRow.SetActive(true);
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(crushesRow.GetComponentInParent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(crushesRow.transform.parent.GetComponentInParent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(crushesRow.transform.parent.transform.parent.GetComponentInParent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
        
        StartCoroutine(StringCountUp.CountUp(crushes, 0.75f, (countUp) => { 
            crushesCountUp.text = countUp;
        }));
        StartCoroutine(StringCountUp.CountUp(crushes * 200, 0.75f, (countUp) => { 
            crushesMultCountUp.text = countUp;
        }));
        t = 0; while (t <= 0.125f) { t += Time.deltaTime; yield return null; }
        
        int highScore = PersistentDataManager.instance.userData.highScore;

        if (total > highScore) {
            highScore = total;
            PersistentDataManager.instance.userData.highScore = highScore;
        }
        StartCoroutine(StringCountUp.CountUp(highScore, 0.75f, (countUp) => { 
            highScoreCountUp.text = countUp;
        }));

        yield return StartCoroutine(StringCountUp.CountUp(total, 0.75f, (countUp) => { 
            totalCountUp.text = countUp;
        }));
        
        quitButton.SetActive(true);
        totalAnim.SetTrigger("Total");  
    }

    public void CloseResults() {
        panel.SetActive(false);
    }

    public void RestartRun() {
        scenario.scenario = ScenarioManager.Scenario.Null;
// Skip tutorial after completing it on restart
        if (ScenarioManager.instance.startCavity == 0 && ScenarioManager.instance.floorManager.floorSequence.activePacket.packetType != FloorChunk.PacketType.Tutorial) 
            ScenarioManager.instance.startCavity = 1;
        PersistentMenu.instance.StartCoroutine(PersistentMenu.instance.FadeToScene(1));
    }

    public void MainMenu() {
        scenario.scenario = ScenarioManager.Scenario.Null;
        PersistentMenu.instance.StartCoroutine(PersistentMenu.instance.FadeToScene(0));
    }

}
