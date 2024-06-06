using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour {
    
    [SerializeField] Button abandonButton;
    [SerializeField] GameObject mainDirectory, options, helpMenu, quitPanel, abandonPanel;
    void OnEnable() {
        Time.timeScale = 0;
    }

    public void Anbandonable(bool state) {
        abandonButton.gameObject.SetActive(state);
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
        ResumeButton();

        ScenarioManager.instance.StartCoroutine(ScenarioManager.instance.Lose());        
    }

    public void QuitToMainMenu() {
        ResumeButton();
        PersistentDataManager.instance.SaveRun();
        QuitButton(false);

        
        PersistentMenu.instance.StartCoroutine(PersistentMenu.instance.FadeToScene(0));
    }

    public void QuitToDesktop() {
        ResumeButton();
        PersistentDataManager.instance.SaveRun();
    }

}
