using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class PersistentMenu : MonoBehaviour
{
    ScenarioManager scenario;
    public MusicController musicController;
    public PauseMenu pauseMenu;
    TooltipSystem toolTips;
    private bool tooltipToggle = true;
    [SerializeField] AudioMixer mixer;
    [SerializeField] Slider musicSlider, sfxSlider;
    bool uiToggle = true;
    bool contextToggle = true;
    [SerializeField] GameObject battleCanvas, menuButton;
    [SerializeField] TMPro.TMP_Text tooltipText;
    [SerializeField] GameObject menuButtons;
    public Animator fadeToBlack;
    public int startCavity;

    const string MIXER_MUSIC = "musicVolume";
    const string MIXER_SFX = "sfxVolume";


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

        // float init;
        // mixer.GetFloat(MIXER_MUSIC, out init); 
        // musicSlider.value = Mathf.Log(init)/20;
        // mixer.GetFloat(MIXER_SFX, out init); 
        // sfxSlider.value = Mathf.Log(init)/20;
        
        Time.timeScale = 1;

        SceneManager.sceneLoaded += UpdateRefs;
    }

    void UpdateRefs(Scene scene = default, LoadSceneMode mode = default) {
        battleCanvas = null;

        if (ScenarioManager.instance) {
// Initialize scenario manager starts game
            scenario = ScenarioManager.instance;
            if (startCavity != -1) {
                scenario.StartCoroutine(scenario.Init(startCavity));
            }
            else
                scenario.StartCoroutine(scenario.Init());
            if (UIManager.instance) 
                battleCanvas = UIManager.instance.gameObject;
            menuButtons.SetActive(true);
        }
// Initialize the MainMenuManager
        else if (MainMenuManager.instance) {
            menuButtons.SetActive(false);
            MainMenuManager.instance.optionsButton.onClick.AddListener(MainMenuPause);
        }

// First ever scene load, initialize MusicController
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

    public void FadeToBlack(bool state) {
        fadeToBlack.SetBool("Fade", state);
    }

    void MainMenuPause() {
        pauseMenu.gameObject.SetActive(true);
    }

    void SetMusicVolume(System.Single vol) {
        mixer.SetFloat(MIXER_MUSIC, Mathf.Log10(vol) * 20);

    }
    
    void SetSFXVolume(System.Single vol) {
        mixer.SetFloat(MIXER_SFX, Mathf.Log10(vol) * 20);
        
    }

    public void ToggleUI() {
        uiToggle = !uiToggle;
        if (battleCanvas) 
            battleCanvas.SetActive(uiToggle);
        menuButton.SetActive(uiToggle);
    }

    public void ToggleGridHighlights() {
        if (FloorManager.instance) {
            FloorManager.instance.GridHighlightToggle();
        }
    }

    public void ToggleContext() {
        contextToggle = !contextToggle;
        if (ScenarioManager.instance) {
            ScenarioManager.instance.player.contextuals.ToggleValid(contextToggle);
            ScenarioManager.instance.player.contextuals.toggled = contextToggle;
        }
    }

    public void ToggleTooltips() {
        tooltipToggle = !tooltipToggle;
        if (toolTips)
            toolTips.gameObject.SetActive(tooltipToggle);
        string state = tooltipToggle ? "ON" : "OFF";
        tooltipText.text = "TOGGLE TOOLTIPS: " +  state;
    }

    public void TriggerCascade() {
        if (FloorManager.instance && ScenarioManager.instance) {
            ScenarioManager.instance.prevTurn = ScenarioManager.Turn.Descent;
            FloorManager.instance.Descend(true);
        }
    }

    public void HealAllUnitsToFull() {
        foreach (Unit u in ScenarioManager.instance.player.units) {
            if (u is Nail) 
                StartCoroutine(u.TakeDamage(u.hpCurrent - u.hpMax));
            else if (u is PlayerUnit pu) {
                if (pu.conditions.Contains(Unit.Status.Disabled))
                    pu.Stabilize();
                StartCoroutine(u.TakeDamage(u.hpCurrent-u.hpMax));
            }
        }
    }

}
