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

    [SerializeField] GameObject floorRow, enemiesRow, buxRow, cutRow, quitButton;
    [SerializeField] Animator totalAnim;

    [SerializeField] TMP_Text resultsTMP, floorsCountUp, floorsMultCountUp, enemiesCountUp, enemiesMultCountUp, buxCountUp, buxMultCountUp, cutPercentCountUp, cutCountUp, totalCountUp;


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

    public IEnumerator UpdateAndDisplay(bool win, int floors, int enemies, int bux) {
        floorRow.SetActive(false); enemiesRow.SetActive(false); buxRow.SetActive(false); cutRow.SetActive(false); quitButton.SetActive(false);
        LayoutRebuilder.ForceRebuildLayoutImmediate(floorRow.GetComponentInParent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(floorRow.transform.parent.GetComponentInParent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(floorRow.transform.parent.transform.parent.GetComponentInParent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());

        int total = (floors * 10) + (enemies * 5) + (bux * 25);
        PersistentMenu.instance.upcomingCurrency = total + (win ? 0 : (int)(-total*0.2f));
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

        float t = 0; while (t <= 0.5f) { t += Time.deltaTime; yield return null; }
        floorRow.SetActive(true);
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(floorRow.GetComponentInParent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(floorRow.transform.parent.GetComponentInParent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(floorRow.transform.parent.transform.parent.GetComponentInParent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
        
        StartCoroutine(StringCountUp.CountUp(floors, 1f, (countUp) => { 
            floorsCountUp.text = countUp;
        }));
        StartCoroutine(StringCountUp.CountUp(floors * 10, 0.75f, (countUp) => { 
            floorsMultCountUp.text = countUp;
        }));
        t = 0; while (t <= 0.125f) { t += Time.deltaTime; yield return null; }
        enemiesRow.SetActive(true);
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(enemiesRow.GetComponentInParent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(enemiesRow.transform.parent.GetComponentInParent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(enemiesRow.transform.parent.transform.parent.GetComponentInParent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
        
        StartCoroutine(StringCountUp.CountUp(enemies, 0.75f, (countUp) => { 
            enemiesCountUp.text = countUp;
        }));
        StartCoroutine(StringCountUp.CountUp(enemies * 5, 0.75f, (countUp) => { 
            enemiesMultCountUp.text = countUp;
        }));
        t = 0; while (t <= 0.125f) { t += Time.deltaTime; yield return null; }
        buxRow.SetActive(true);
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(buxRow.GetComponentInParent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(buxRow.transform.parent.GetComponentInParent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(buxRow.transform.parent.transform.parent.GetComponentInParent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
        
        StartCoroutine(StringCountUp.CountUp(bux, 0.75f, (countUp) => { 
            buxCountUp.text = countUp;
        }));
        StartCoroutine(StringCountUp.CountUp(bux * 25, 0.75f, (countUp) => { 
            buxMultCountUp.text = countUp;
        }));
        t = 0; while (t <= 0.125f) { t += Time.deltaTime; yield return null; }
        
        if (!win) {
            cutRow.SetActive(true);
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(cutRow.GetComponentInParent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(cutRow.transform.parent.GetComponentInParent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(cutRow.transform.parent.transform.parent.GetComponentInParent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            Canvas.ForceUpdateCanvases();

            StartCoroutine(StringCountUp.CountUp(-20, 0.75f, (countUp) => { 
                cutPercentCountUp.text = countUp + "%";
            }));
            StartCoroutine(StringCountUp.CountUp(-Mathf.RoundToInt(total*0.2f), 0.75f, (countUp) => { 
                cutCountUp.text = countUp;
            }));
        }
        
        t = 0; while (t <= 0.125f) { t += Time.deltaTime; yield return null; }
        yield return StartCoroutine(StringCountUp.CountUp(total + (win ? 0 : (int)(-total*0.2f)), 0.75f, (countUp) => { 
            totalCountUp.text = countUp;
        }));
        
        quitButton.SetActive(true);
        totalAnim.SetTrigger("Total");  
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
