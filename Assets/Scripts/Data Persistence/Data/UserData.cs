using System.Collections.Generic;

[System.Serializable]
public class UserData {

    public float musicVol;
    public float sfxVol;
    public bool cutsceneSkip;
    public bool scatterSkip;
    public bool fullscreen;
    public int resolutionIndex;
    public bool tooltipToggle;
    public Dictionary<string, bool> tooltipsEncountered;

    public UserData() {
        this.musicVol = 5f;
        this.sfxVol = 5f;
        this.cutsceneSkip = false;
        this.fullscreen = true;
        tooltipsEncountered = new();
    }


}
