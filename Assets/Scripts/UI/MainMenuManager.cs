using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{

    public static MainMenuManager instance;
    void Awake() {
        if (MainMenuManager.instance) {
            Debug.Log("Warning! More than one instance of MainMenuManager found!");
            return;
        }
        instance = this;
    }
    
    public Button optionsButton;
    [SerializeField] GameObject buttonColumn, credits;
    [SerializeField] Animator nailAnim;

    public void ToggleCredits(bool state) {
        buttonColumn.SetActive(!state);
        credits.SetActive(state);
    }

    public void SendNail(string scene) {
        nailAnim.SetTrigger("Start Game");
        StartCoroutine(FadeOut(scene));
    }

    IEnumerator FadeOut(string scene) {
        yield return new WaitForSecondsRealtime(1f);
        PersistentMenu.instance.FadeToBlack(true);
        yield return new WaitForSecondsRealtime(0.5f);
        SceneManager.LoadScene(scene, LoadSceneMode.Single);
    }
}
