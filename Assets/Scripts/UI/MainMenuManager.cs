using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
    
    public Button optionsButton, continueButton;
    [SerializeField] GameObject buttonColumn;
    [SerializeField] Animator creditsAnim, mainButtonsAnim, playButtonsAnim, tutorialButtonsAnim, nailAnim;

    [Header("What's To Come Refs")]
    [SerializeField] DialogueTooltip tooltip;
    [SerializeField] TMP_Text currencyCountUp;
    [SerializeField] Animator slimeBucksAnim, bodegaAnim, cannonAnim, logoAnim;

    public void Init(bool runSaved) {
        mainButtonsAnim.SetTrigger("FadeIn");
        continueButton.gameObject.SetActive(runSaved);
    }

    public IEnumerator WhatsToCome(int currency) {
        float t = 0;
        bodegaAnim.gameObject.SetActive(true);
        cannonAnim.gameObject.SetActive(true);
        mainButtonsAnim.gameObject.SetActive(false);
        logoAnim.gameObject.SetActive(false);

        t = 0; while (t <= 1f) { t += Time.deltaTime; yield return null; }
        slimeBucksAnim.gameObject.SetActive(true);

        yield return StartCoroutine(StringCountUp.CountUp(currency, 0.75f, (countUp) => { 
            currencyCountUp.text = countUp;
        }));
        t = 0; while (t <= 0.25f) { t += Time.deltaTime; yield return null; }
        tooltip.transform.GetChild(0).gameObject.SetActive(true);
        tooltip.SetText(
            ColorToRichText("Nice work down there, squish!") + "</b>. Welcome back to the SLime Hub. this is where you'll <b>" + ColorToRichText("beef up your arsenal") + "</b> before taking another crack at the Big One's brain."
        , "Slime Hub", true);
        while (!tooltip.skip) yield return null;
        tooltip.SetText(
            "Down the road, you'll be able to use your earnings to snag <b>" + ColorToRichText("new Gear") + "</b> for your Slags, carve out <b>" + ColorToRichText("shortcuts") + "</b> through the skull, and unlock <b>" + ColorToRichText("new god thoughts") + "</b> for your deep dives."
        , "Coming Soon", true);
        while (!tooltip.skip) yield return null;
        tooltip.SetText(
            "This demo will be updated as development continues, bringing exciting new content for you to experience and enjoy. Don't forget to wishlist DEEPMESS and join the Discord for updates."
        , "Next Time", true);
        while (!tooltip.skip) yield return null;

        tooltip.transform.GetChild(0).gameObject.SetActive(false);

        t = 0; 
        logoAnim.gameObject.SetActive(true);
        mainButtonsAnim.gameObject.SetActive(true);
        mainButtonsAnim.SetTrigger("FadeIn");
        slimeBucksAnim.SetTrigger("SlideOut");
        while (t < 0.5f) {
            float c = Mathf.Lerp(1, 0, t/0.5f);
            bodegaAnim.GetComponent<SpriteRenderer>().color = new Color (1, 1, 1, c);
            cannonAnim.GetComponent<SpriteRenderer>().color = new Color (1, 1, 1, c);
            logoAnim.GetComponent<SpriteRenderer>().color = new Color (1, 1, 1, 1-c);
            t += Time.deltaTime;
            yield return null;
        }

        slimeBucksAnim.gameObject.SetActive(true);
        PersistentMenu.instance.upcomingCurrency = 0;
    }

    public void ToggleCredits(bool state) {
        for (int i = buttonColumn.transform.childCount - 1; i >= 0; i--) {
            Button b = buttonColumn.transform.GetChild(i).GetComponent<Button>();
            if (b != null)
                buttonColumn.transform.GetChild(i).GetComponent<Button>().interactable = !state;
        }
        if (state) {
            creditsAnim.gameObject.SetActive(state);
            creditsAnim.SetBool("Active", true);
        } else
            creditsAnim.SetBool("Active", false);
    }

    public void TogglePlayButtons(bool state) {
        for (int i = buttonColumn.transform.childCount - 1; i >= 0; i--) {
            Button b = buttonColumn.transform.GetChild(i).GetComponent<Button>();
            if (b != null)
                buttonColumn.transform.GetChild(i).GetComponent<Button>().interactable = !state;
        }

        if (state) {
            playButtonsAnim.gameObject.SetActive(true);
            playButtonsAnim.SetBool("Active", true);
        } else 
            playButtonsAnim.SetBool("Active", false);
    }

    public void ToggleTutorialButtons(bool state) {
        for (int i = buttonColumn.transform.childCount - 1; i >= 0; i--) {
            Button b = buttonColumn.transform.GetChild(i).GetComponent<Button>();
            if (b != null)
                buttonColumn.transform.GetChild(i).GetComponent<Button>().interactable = !state;
        }

        if (state) {
            tutorialButtonsAnim.gameObject.SetActive(true);
            tutorialButtonsAnim.SetBool("Active", true);
        } else 
            tutorialButtonsAnim.SetBool("Active", false);
    }

    public void SendNail(int index) {
        nailAnim.SetTrigger("Start Game");
        for (int i = playButtonsAnim.transform.childCount - 1; i >= 0; i--) 
            playButtonsAnim.transform.GetChild(i).GetComponent<Button>().interactable = false;

        PersistentMenu.instance.startIndex = index;
        if (index == 1) PersistentDataManager.instance.DeleteRun();
        StartCoroutine(FadeOut(index));
    }

    IEnumerator FadeOut(int index) {
        yield return new WaitForSecondsRealtime(1f);
        PersistentMenu.instance.FadeToBlack(true);
        if (index != 0) {
            PersistentMenu.instance.musicController.SwitchMusicState(MusicController.MusicState.Game, true);
            if (index > 1)
                PersistentDataManager.instance.LoadRun();
        }
        else {    
            PersistentMenu.instance.musicController.SwitchMusicState(MusicController.MusicState.Tutorial, true);
        }
        yield return new WaitForSecondsRealtime(1f);
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }
    
    public void ExitApplication() {
        Application.Quit();
    }

    static string ColorToRichText(string str) {
        return "<color=#" + ColorUtility.ToHtmlStringRGB(keyColor) + ">" + str + "</color>";
    }
}
