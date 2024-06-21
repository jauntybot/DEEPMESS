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
    public Dictionary<string, bool> tooltipsEncountered;

    public UserData() {
        this.musicVol = 5f;
        this.sfxVol = 5f;
        this.cutsceneSkip = false;
        this.resolution = new();
        this.fullscreen = true;
        this.tooltipToggle = false;
        tooltipsEncountered = new();
    }

    public UserData(Dictionary<string, bool> tooltips, bool _tooltipToggle) {
        this.musicVol = 5f;
        this.sfxVol = 5f;
        this.cutsceneSkip = false;
        this.resolution = new();
        this.fullscreen = true;
        this.tooltipToggle = _tooltipToggle;
        tooltipsEncountered = new(tooltips);
    }

}
