[System.Serializable]
public class UserData {

    public float musicVol;
    public float sfxVol;
    public bool cutsceneSkip;
    public bool scatterSkip;
    public bool fullscreen;
    public int resolutionIndex;

    public UserData() {
        this.musicVol = 5f;
        this.sfxVol = 5f;
        this.cutsceneSkip = false;
        this.scatterSkip = false;
        this.fullscreen = true;
    }


}
