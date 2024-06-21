using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using Relics;
using System;

public class PersistentMenu : MonoBehaviour, IUserDataPersistence, IRunDataPersistence {
    ScenarioManager scenario;
    public MusicController musicController;
    public PauseMenu pauseMenu;
    public int targetFPS;
    //[SerializeField] int fps;
    TooltipSystem toolTips;
    [SerializeField] AudioMixer mixer;
    [SerializeField] Slider musicSlider, sfxSlider;
    [SerializeField] TMP_Dropdown resolutionsDropdown;
    [SerializeField] Toggle fullscreenToggle, cutsceneToggle, tooltipToggle;
    public Toggle scatterToggle;
    [SerializeField] GameObject menuButton;
    public Animator fadeToBlack;

    const string MIXER_MUSIC = "musicVolume";
    const string MIXER_SFX = "sfxVolume";

    public int upcomingCurrency;
    [SerializeField] TMP_Dropdown relicDropdown;
    [SerializeField] Animator splashAnim;
    bool splash;
    [SerializeField] GameObject loadIcon;
    public static PersistentMenu instance;
    [SerializeField] Color keyColor;
    private void Awake() {
        if (PersistentMenu.instance) {
            Debug.Log("Warning! More than one instance of PersistentMenu found!");
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this);
        splash = false;

        musicController = GetComponentInChildren<MusicController>();
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        
        ConfigureResolutionDropdown();
        // musicSlider.value = 5f;
        // sfxSlider.value = 5f;

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFPS;
        //InvokeRepeating("GetFPS", 1, 1);

        Time.timeScale = 1;
        loadedRun = null;

        SceneManager.sceneLoaded += UpdateRefs;
    }

    int currentResolutionIndex;
    List<Resolution> filteredResolutions = new();
    void ConfigureResolutionDropdown() {
        Resolution[] resolutions = Screen.resolutions;
        float currentRefereshRate;

        resolutionsDropdown.ClearOptions();
        currentRefereshRate = Screen.currentResolution.refreshRate;

        for (int i = resolutions.Length - 1; i >= 0 ; i--) {
            if (resolutions[i].refreshRate == currentRefereshRate &&
                ((resolutions[i].width == 1280 && resolutions[i].height == 720) ||
                (resolutions[i].width == 1280 && resolutions[i].height == 800) ||
                (resolutions[i].width == 1920 && resolutions[i].height == 1080) ||
                (resolutions[i].width == 1920 && resolutions[i].height == 1200) ||
                (resolutions[i].width == 2560 && resolutions[i].height == 1440) ||
                (resolutions[i].width == 2560 && resolutions[i].height == 1600)))
                filteredResolutions.Add(resolutions[i]);
//                Debug.Log(resolutions[i].width + ", " + resolutions[i].height + ", " + resolutions[i].refreshRate);
        }

        List<string> options = new List<string>();
        for (int i = 0; i < filteredResolutions.Count; i++) {
            string resolutionOption = filteredResolutions[i].width + "x" + filteredResolutions[i].height;
            options.Add(resolutionOption);
            if (filteredResolutions[i].width == Screen.width && filteredResolutions[i].height == Screen.height)
                currentResolutionIndex = i;
        }

        if (resolutionsDropdown) {
            resolutionsDropdown.AddOptions(options);
            resolutionsDropdown.value = currentResolutionIndex;
            resolutionsDropdown.RefreshShownValue();
        }
    }

    public void LoadUser(UserData user) {
        musicSlider.value = user.musicVol;
        sfxSlider.value = user.sfxVol;
        
        SetResolution(user.resolutionIndex);
        SetMusicVolume(user.musicVol);
        SetSFXVolume(user.sfxVol);
        
        fullscreenToggle.isOn = user.fullscreen;
        Screen.fullScreen = user.fullscreen;
        cutsceneToggle.isOn = user.cutsceneSkip;
        tooltipToggle.isOn = user.tooltipToggle;

    }

    public void SaveUser(ref UserData user) {
        user.musicVol = musicSlider.value;
        user.sfxVol = sfxSlider.value;
        
        user.resolutionIndex = currentResolutionIndex;
        
        user.cutsceneSkip = cutsceneToggle.isOn;
        user.fullscreen = fullscreenToggle.isOn;
        user.tooltipToggle = tooltipToggle.isOn;

        if (scenario) {
            Dictionary<string, bool> tooltips = new() {
                { "bulbEncountered", scenario.gpOptional.bulbEncountered },
                { "deathReviveEncountered", scenario.gpOptional.deathReviveEncountered },
                { "bossEncountered", scenario.gpOptional.bossEncountered},
                { "objectivesEncountered", scenario.gpOptional.objectivesEncountered},
                { "bloatedBulbEncountered", scenario.gpOptional.bloatedBulbEncountered },
                { "beaconEncountered", scenario.gpOptional.beaconEncountered },
                { "beaconObjectivesEncountered", scenario.gpOptional.beaconObjectivesEncountered },
                { "beaconScratchOffEncountered", scenario.gpOptional.beaconScratchOffEncountered }
            };
            user.tooltipsEncountered = tooltips;
        }
    }
      
    public void LoadRun(RunData run) {
        loadedRun = run;
        Debug.Log("persistent menu loaded run");
    }

    public void SaveRun(ref RunData run) {
        run = new RunData((int)scenario.floorManager.floorSequence.currentThreshold, scenario.player.units, scenario.relicManager.collectedRelics, scenario.objectiveManager.activeObjectives, scenario.objectiveManager.objectiveIndices, scenario.player.collectedNuggets);
    }

    // void GetFPS() {
    //     fps = (int) (1f / Time.unscaledDeltaTime);
    // }

    public RunData loadedRun;
    public int startIndex = -1;
    void UpdateRefs(Scene scene = default, LoadSceneMode mode = default) {
        if (ScenarioManager.instance) {
// Initialize scenario manager starts game
            scenario = ScenarioManager.instance;
            scenario.StartCoroutine(scenario.Init(loadedRun, startIndex));
    

            relicDropdown.ClearOptions();
            RelicManager relicManager = RelicManager.instance;
            List<string> options = new List<string>();
            for (int i = 0; i <= relicManager.serializedRelics.Count - 1; i++) {
                options.Add(relicManager.serializedRelics[i].name);
            }
            relicDropdown.AddOptions(options);
            relicDropdown.value = 0;
            relicDropdown.RefreshShownValue();

            pauseMenu.ToggleOptionsBack(false);
            pauseMenu.ToggleHelpBack(false);
            menuButton.SetActive(true);
        }
// Initialize the MainMenuManager
        else if (MainMenuManager.instance) {
            loadedRun = null;
            menuButton.SetActive(false);
            MainMenuManager.instance.optionsButton.onClick.AddListener(MainMenuOptions);
            MainMenuManager.instance.helpButton.onClick.AddListener(MainMenuHelp);
            pauseMenu.ToggleOptionsBack(true);
            pauseMenu.ToggleHelpBack(true);
            if (upcomingCurrency > 0) MainMenuManager.instance.StartCoroutine(MainMenuManager.instance.WhatsToCome(upcomingCurrency));
            else MainMenuManager.instance.Init(PersistentDataManager.instance.IsRunSaved());
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
        
        if (!splash && MainMenuManager.instance) {
            StartCoroutine(SplashDelay());
        } else {
            StartCoroutine(SceneLoadDelay());
        }
    }

    IEnumerator SceneLoadDelay() {
        float t = 0;
        while (t <= 1.5f) {
            t += Time.deltaTime;
            yield return null;
        }
        FadeToBlack(false);
    }


    public void SplashContinue() {
        splashContinue = true;
    }
    bool splashContinue;
    IEnumerator SplashDelay() {
        splashContinue = false;
        splashAnim.gameObject.SetActive(true);
        splashAnim.GetComponent<TMP_Text>().text = 
        "Yo, squish! <b>" + ColorToRichText("Welcome to DEEPMESS") + "</b>. You're about to enter the Slime Hub, where a parasitic pink slime mold has taken root - <b>" + ColorToRichText("right above god's shiny dome") + "</b>. Your goal: help the slime reach the source of the tastiest thoughts in existence (and <b>" + ColorToRichText("give god a lobotomy") + "</b> in the process). Explore a sampling of enemies, upgrades, and delectable god-thoughts in the <b>" + ColorToRichText("Cranium") + "</b> - a bony glimpse into the first of many biomes in god's big head." + 
        '\n' + '\n' + "<b>" + ColorToRichText("This is a demo build") + "</b>, a taste of the mess to come. Some features might not function perfectly. You might encounter bugs lurking in the cracks and crevices of god's brain." +
        '\n' + '\n' + "This demo <b>" + ColorToRichText("will be updated as development continues") + "</b>, bringing exciting new content for you to experience and enjoy. <b>" + ColorToRichText("Wishlist DEEPMESS") + "</b> and <b>" + ColorToRichText("join the discord") + "</b> for updates and to support development." +
        '\n' + '\n' + "Dig deep. Make mess.";

        float t = 0;
        while (t <= 3f) {
            t += Time.deltaTime;
            yield return null;
        }
        loadIcon.SetActive(false);
        splashAnim.SetTrigger("Continue");
        yield return null;
        while (!splashContinue) {
            yield return null;
        }
        splashAnim.SetTrigger("Continue");
        splash = true;
        FadeToBlack(false);
    }

    public IEnumerator FadeToScene(int index) {
        PersistentDataManager.instance.SaveUser();
        Time.timeScale = 1f;
        yield return new WaitForSecondsRealtime(0.25f);
        fadeToBlack.SetBool("Fade", true);
        if (index == 0) {
            PersistentMenu.instance.musicController.SwitchMusicState(MusicController.MusicState.MainMenu, true);
        }
        yield return new WaitForSecondsRealtime(1f);
        AsyncOperation op = SceneManager.LoadSceneAsync(index, LoadSceneMode.Single);
        Time.timeScale = 1f;
        while (!op.isDone) {
            yield return null;
        }
        Time.timeScale = 1f;
    }

    public void FadeToBlack(bool state) {
        loadIcon.SetActive(state);
        fadeToBlack.SetBool("Fade", state);
    }

    void MainMenuOptions() {
        pauseMenu.gameObject.SetActive(true);
        pauseMenu.Options(true);
    }
    
    void MainMenuHelp() {
        pauseMenu.gameObject.SetActive(true);
        pauseMenu.HelpButton(true);
    }


    void SetMusicVolume(System.Single vol) {
        System.Single map = Util.Remap(vol, 0, 9, .0001f, .5f);
        mixer.SetFloat(MIXER_MUSIC, Mathf.Log10(map) * 30);

    }
    
    void SetSFXVolume(System.Single vol) {
        System.Single map = Util.Remap(vol, 0, 9, .0001f, .6f);
        mixer.SetFloat(MIXER_SFX, Mathf.Log10(map) * 30);
        
    }

    public void ToggleFullscreen() {
        Screen.fullScreen = fullscreenToggle.isOn;
    }

    public void SetResolution(int index) {
        if (filteredResolutions.Count > 0) {
            Resolution resolution = filteredResolutions[index];
            Screen.SetResolution(resolution.width, resolution.height, true);
            currentResolutionIndex = index;
        }
    }
    
    public void SetResolution(Resolution resolution) {
        if (filteredResolutions.Contains(resolution)) {
            Screen.SetResolution(resolution.width, resolution.height, fullscreenToggle.isOn);
            currentResolutionIndex = filteredResolutions.IndexOf(resolution);
        } else SetResolution(0);
    }

    public void ResetTooltips() {
        PersistentDataManager.instance.userData.tooltipsEncountered = new();
        if (GameplayOptionalTooltips.instance)
            GameplayOptionalTooltips.instance.LoadTooltips();
    }

    public void ToggleTooltips(bool state) {
        PersistentDataManager.instance.userData.tooltipToggle = state;
        if (GameplayOptionalTooltips.instance)
            GameplayOptionalTooltips.instance.LoadTooltips();
    }


    #region Debug Functions
    public void TriggerCascade() {
        if (FloorManager.instance && ScenarioManager.instance && !FloorManager.instance.transitioning && !FloorManager.instance.peeking) {
            ScenarioManager.instance.prevTurn = ScenarioManager.Turn.Descent;
            FloorManager.instance.Descend(true);
        }
    }

    RelicData relic;
    public void SetRelic(int relicIndex) {
        relic = scenario.relicManager.serializedRelics[relicIndex];
    }
    public void GiveRelic() {
        StartCoroutine(scenario.relicManager.PresentRelic(relic));
    }

    public void GiveAllRelics() {
        scenario.relicManager.GiveAllRelics();
    }
    #endregion

    string ColorToRichText(string str) {
        return "<color=#" + ColorUtility.ToHtmlStringRGB(keyColor) + ">" + str + "</color>";
    }
}
