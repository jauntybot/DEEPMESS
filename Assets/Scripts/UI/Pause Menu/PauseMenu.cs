using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour {
    void OnEnable() {
        
        Time.timeScale = 0;
    }

    public void ResumeButton() {   
        Time.timeScale = 1;
        gameObject.SetActive(false);        
    }

    public void RestartButton() {
        Time.timeScale = 1;
        ScenarioManager.instance.StartCoroutine(ScenarioManager.instance.Lose());
        gameObject.SetActive(false);   
    }

    public void MainMenu() {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }
}
