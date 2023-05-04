using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class PersistentMenu : MonoBehaviour
{

    [SerializeField] AudioMixer mixer;
    [SerializeField] Slider musicSlider, sfxSlider;

    const string MIXER_MUSIC = "musicVolume";
    const string MIXER_SFX = "sfxVolume";


    public static PersistentMenu instance;
    private void Awake() {
        if (instance) {
            Debug.Log("Warning! More than one instance of PersistentMenu found!");
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this);

        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        
        Time.timeScale = 1;
    }

    void SetMusicVolume(float vol) {
        mixer.SetFloat(MIXER_MUSIC, Mathf.Log10(vol) * 20);

    }

    
    void SetSFXVolume(float vol) {
        mixer.SetFloat(MIXER_SFX, Mathf.Log10(vol) * 20);
        
    }



}
