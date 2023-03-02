using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] MusicController music;
    private float prevVol;
    public Slider audioVolume;



    void OnEnable() {
        music.AudioVolume(0.1f);
        Time.timeScale = 0;
    }

    public void ResumeButton()
    {   
        Time.timeScale = 1;
        music.AudioVolume(audioVolume.value);
        
        gameObject.SetActive(false);        
    }

    public void RestartButton()
    {
        gameObject.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
