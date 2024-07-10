using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UserData {

    public float musicVol;
    public float sfxVol;
    public bool cutsceneSkip;
    public bool fullscreen;
    public List<int> resolution;
    public bool tooltipToggle;
    public bool promptTutorial;
    public int highScore;
    public Dictionary<string, bool> tooltipsEncountered;

    public UserData() {
        this.musicVol = 5f;
        this.sfxVol = 5f;
        this.cutsceneSkip = false;
        this.resolution = new();
        this.fullscreen = true;
        this.tooltipToggle = false;
        this.promptTutorial = true;
        this.highScore = 0;
        tooltipsEncountered = new();
    }

// Used for resetting options while preserving tooltips
    public UserData(bool tutPrompt, int _highScore, Dictionary<string, bool> tooltips, bool _tooltipToggle) {
        this.musicVol = 5f;
        this.sfxVol = 5f;
        this.cutsceneSkip = false;
        this.resolution = new();
        this.fullscreen = true;
        this.promptTutorial = tutPrompt;
        this.tooltipToggle = _tooltipToggle;
        this.highScore = _highScore;
        tooltipsEncountered = new(tooltips);
    }

}
