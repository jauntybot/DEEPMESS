using System.Collections;
using System.Collections.Generic;
using UnityEditor.Compilation;
using UnityEngine;

[System.Serializable]
public class UserData {

    public float musicVol;
    public float sfxVol;
    public int resolutionIndex;

    public UserData() {
        this.musicVol = 5f;
        this.sfxVol = 5f;
    }


}
