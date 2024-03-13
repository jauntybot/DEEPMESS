using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using Relics;

public class PersistentMenu : MonoBehaviour
{
    ScenarioManager scenario;
    public MusicController musicController;
    public PauseMenu pauseMenu;
    public int targetFPS;
    //[SerializeField] int fps;
    TooltipSystem toolTips;
    private bool tooltipToggle = true;
    [SerializeField] AudioMixer mixer;
    [SerializeField] Slider musicSlider, sfxSlider;

    [SerializeField] GameObject battleCanvas, menuButton;
    [SerializeField] TMP_Text tooltipText;
    [SerializeField] GameObject menuButtons;
    public Animator fadeToBlack;
    public int startCavity;

    const string MIXER_MUSIC = "musicVolume";
    const string MIXER_SFX = "sfxVolume";

    public float upcomingCurrency;
    [SerializeField] TMP_Dropdown relicDropdown;


    public static PersistentMenu instance;
    private void Awake() {
        if (PersistentMenu.instance) {
            Debug.Log("Warning! More than one instance of PersistentMenu found!");
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this);

        musicController = GetComponentInChildren<MusicController>();
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        musicSlider.value = 5f;
        sfxSlider.value = 5f;

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFPS;
        //InvokeRepeating("GetFPS", 1, 1);

        Time.timeScale = 1;

        SceneManager.sceneLoaded += UpdateRefs;
    }

    // void GetFPS() {
    //     fps = (int) (1f / Time.unscaledDeltaTime);
    // }

    void UpdateRefs(Scene scene = default, LoadSceneMode mode = default) {
        battleCanvas = null;

        if (ScenarioManager.instance) {
// Initialize scenario manager starts game
            scenario = ScenarioManager.instance;
            if (startCavity != -1) {
                scenario.StartCoroutine(scenario.Init(startCavity));
            } else
                scenario.StartCoroutine(scenario.Init());

            relicDropdown.ClearOptions();
            RelicManager relicManager = RelicManager.instance;
            List<string> options = new List<string>();
            for (int i = 0; i <= relicManager.serializedRelics.Count - 1; i++) {
                options.Add(relicManager.serializedRelics[i].name);
            }
            relicDropdown.AddOptions(options);
            relicDropdown.value = 0;
            relicDropdown.RefreshShownValue();

            if (UIManager.instance) 
                battleCanvas = UIManager.instance.gameObject;
            menuButtons.SetActive(true);
        }
// Initialize the MainMenuManager
        else if (MainMenuManager.instance) {
            menuButtons.SetActive(false);
            MainMenuManager.instance.optionsButton.onClick.AddListener(MainMenuPause);
            if (upcomingCurrency > 0) StartCoroutine(WhatsToCome());
        }

// initialize MusicController if not initialized
        if (musicController.currentState == MusicController.MusicState.Null) {
            if (SceneManager.GetActiveScene().buildIndex == 0)
                musicController.SwitchMusicState(MusicController.MusicState.MainMenu, false);
            else if (scenario.startCavity == 0)
                musicController.SwitchMusicState(MusicController.MusicState.Tutorial, false);
            else
                musicController.SwitchMusicState(MusicController.MusicState.Game, false);
        }
        if (TooltipSystem.instance)
            toolTips = TooltipSystem.instance;
        
        FadeToBlack(false);
    }

    RelicData relic;
    public void SetRelic(int relicIndex) {
        relic = scenario.relicManager.serializedRelics[relicIndex];
    }
    public void GiveRelic() {
        StartCoroutine(scenario.relicManager.PresentRelic(relic));
    }

    bool whatsToCome;
    IEnumerator WhatsToCome() {
        whatsToCome = true;

        while (whatsToCome) {

            yield return null;
        }    
        upcomingCurrency = 0;
    }

    public void ContinueWhatsToCome() {
        whatsToCome = false;
    }

    public void FadeToBlack(bool state) {
        fadeToBlack.SetBool("Fade", state);
    }

    void MainMenuPause() {
        pauseMenu.gameObject.SetActive(true);
        pauseMenu.Anbandonable(false);
    }

    void SetMusicVolume(System.Single vol) {
        System.Single map = Util.Remap(vol, 0, 9, .0001f, .5f);
        mixer.SetFloat(MIXER_MUSIC, Mathf.Log10(map) * 30);

    }
    
    void SetSFXVolume(System.Single vol) {
        System.Single map = Util.Remap(vol, 0, 9, .0001f, .6f);
        mixer.SetFloat(MIXER_SFX, Mathf.Log10(map) * 30);
        
    }


    public void TriggerCascade() {
        if (FloorManager.instance && ScenarioManager.instance) {
            ScenarioManager.instance.prevTurn = ScenarioManager.Turn.Descent;
            FloorManager.instance.Descend(true);
        }
    }

}
