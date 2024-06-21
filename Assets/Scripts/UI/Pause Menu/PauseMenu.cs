using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour {
    
    [SerializeField] Button optionsBack, helpBack;
    [SerializeField] GameObject mainDirectory, options, helpMenu, quitPanel, abandonPanel;
    void OnEnable() {
        Time.timeScale = 0;
    }

    public void ResumeButton() {   
        Time.timeScale = 1;
        gameObject.SetActive(false);        
    }

    public void Options(bool toFrom) {
        mainDirectory.SetActive(!toFrom);
        options.SetActive(toFrom);
    }

    public void HelpButton(bool toFrom) {
        mainDirectory.SetActive(!toFrom);
        helpMenu.SetActive(toFrom);
    }

    public void QuitButton(bool toFrom) {
        mainDirectory.SetActive(!toFrom);
        quitPanel.SetActive(toFrom);
    }

    public void AbandonButton(bool toFrom) {
        mainDirectory.SetActive(!toFrom);
        abandonPanel.SetActive(toFrom);
    }

    public void AbandonRun() {
        AbandonButton(false);
        ResumeButton();

        ScenarioManager.instance.StartCoroutine(ScenarioManager.instance.Lose());        
    }

    public void ToggleOptionsBack(bool mainMenu) {
        optionsBack.onClick.RemoveAllListeners();
        optionsBack.onClick.AddListener(delegate { Options(false); });
        if (mainMenu) {
            optionsBack.onClick.AddListener(ResumeButton);
        }
    }

    public void ToggleHelpBack(bool mainMenu) {
        helpBack.onClick.RemoveAllListeners();
        helpBack.onClick.AddListener(delegate { HelpButton(false); });
        if (mainMenu) {
            helpBack.onClick.AddListener(ResumeButton);
        }
    }


    public void QuitToMainMenu() {
        QuitButton(false);
        ResumeButton();
        ScenarioManager.instance.objectiveManager.ClearObjectives();
        ScenarioManager.instance.relicManager.ClearRelics();
        
        PersistentMenu.instance.StartCoroutine(PersistentMenu.instance.FadeToScene(0));
    }

    public void QuitToDesktop() {
        QuitButton(false);
        ResumeButton();
        ScenarioManager.instance.objectiveManager.ClearObjectives();
        ScenarioManager.instance.relicManager.ClearRelics();
    }

}
