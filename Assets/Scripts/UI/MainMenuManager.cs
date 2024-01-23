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
    [SerializeField] GameObject buttonColumn;
    [SerializeField] Animator creditsAnim, playButtonsAnim, nailAnim;

    public void ToggleCredits(bool state) {
        for (int i = buttonColumn.transform.childCount - 1; i >= 0; i--) 
            buttonColumn.transform.GetChild(i).GetComponent<Button>().interactable = !state;
        if (state) {
            creditsAnim.gameObject.SetActive(state);
            creditsAnim.SetBool("Active", true);
        } else
            creditsAnim.SetBool("Active", false);
    }

    public void TogglePlayButtons(bool state) {
        for (int i = buttonColumn.transform.childCount - 1; i >= 0; i--) 
            buttonColumn.transform.GetChild(i).GetComponent<Button>().interactable = !state;

        if (state) {
            playButtonsAnim.gameObject.SetActive(true);
            playButtonsAnim.SetBool("Active", true);
        } else 
            playButtonsAnim.SetBool("Active", false);
    }

    public void SendNail(int index) {
        nailAnim.SetTrigger("Start Game");
        for (int i = playButtonsAnim.transform.childCount - 1; i >= 0; i--) 
            playButtonsAnim.transform.GetChild(i).GetComponent<Button>().interactable = false;

        StartCoroutine(FadeOut(index));
    }

    IEnumerator FadeOut(int index) {
        yield return new WaitForSecondsRealtime(1f);
        PersistentMenu.instance.startCavity = index;
        PersistentMenu.instance.FadeToBlack(true);
        // if (index != 0)
        //     PersistentMenu.instance.musicController.SwitchMusicState(MusicController.MusicState.Game, true);
        // else
        //     PersistentMenu.instance.musicController.SwitchMusicState(MusicController.MusicState.Tutorial, true);
        yield return new WaitForSecondsRealtime(1f);
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }
    
    public void ExitApplication() {
        Application.Quit();
    }
}
