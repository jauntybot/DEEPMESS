using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using Relics;

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

    [SerializeField] GameObject menuButton;
    [SerializeField] TMP_Text tooltipText;
    [SerializeField] GameObject menuButtons;
    public Animator fadeToBlack;
    public int startCavity;

    const string MIXER_MUSIC = "musicVolume";
    const string MIXER_SFX = "sfxVolume";

    public int upcomingCurrency;
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
        
        ConfigureResolutionDropdown();
        // musicSlider.value = 5f;
        // sfxSlider.value = 5f;

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFPS;
        //InvokeRepeating("GetFPS", 1, 1);

        Time.timeScale = 1;

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
                Debug.Log(resolutions[i].width + ", " + resolutions[i].height + ", " + resolutions[i].refreshRate);
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
        
    }

    public void SaveUser(ref UserData user) {
        user.musicVol = musicSlider.value;
        user.sfxVol = sfxSlider.value;
        user.resolutionIndex = currentResolutionIndex;
    }

    
    public void LoadRun(RunData run) {

    }

    public void SaveRun(ref RunData run) {
        RunData data = new RunData(scenario.floorManager.currentFloor.lvlDef, scenario.floorManager.floorSequence.activePacket, scenario.player.units, scenario.relicManager.collectedRelics);
        
        run.currentFloor = scenario.floorManager.currentFloor.lvlDef;
        run.activeChunk = scenario.floorManager.floorSequence.activePacket;

        run.gear1 = (SlagGearData)scenario.player.units[0].equipment[1];
        run.gear1Upgrades = run.gear1.upgrades;
    }

    // void GetFPS() {
    //     fps = (int) (1f / Time.unscaledDeltaTime);
    // }

    void UpdateRefs(Scene scene = default, LoadSceneMode mode = default) {
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

            menuButtons.SetActive(true);
        }
// Initialize the MainMenuManager
        else if (MainMenuManager.instance) {
            menuButtons.SetActive(false);
            MainMenuManager.instance.optionsButton.onClick.AddListener(MainMenuPause);
            if (upcomingCurrency > 0) MainMenuManager.instance.StartCoroutine(MainMenuManager.instance.WhatsToCome(upcomingCurrency));
            else MainMenuManager.instance.Init();
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

    public IEnumerator FadeToScene(int index) {
        yield return new WaitForSecondsRealtime(0.25f);
        fadeToBlack.SetBool("Fade", true);
        if (index == 0) {
            PersistentMenu.instance.musicController.SwitchMusicState(MusicController.MusicState.MainMenu, true);
        }
        yield return new WaitForSecondsRealtime(1f);
        SceneManager.LoadScene(index, LoadSceneMode.Single);
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

    public void SetResolution(int index) {
        if (filteredResolutions.Count > 0) {
            Resolution resolution = filteredResolutions[index];
            Screen.SetResolution(resolution.width, resolution.height, true);
            currentResolutionIndex = index;
        }
    }
    
    public void SetResolution(Resolution resolution) {
        if (filteredResolutions.Contains(resolution)) {
            Screen.SetResolution(resolution.width, resolution.height, true);
            currentResolutionIndex = filteredResolutions.IndexOf(resolution);
        } else SetResolution(0);
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
}
