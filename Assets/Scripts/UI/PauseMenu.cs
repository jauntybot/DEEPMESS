using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] MusicController music;
    private float prevVol;
    string restartString;


    void OnEnable() {
        
        Time.timeScale = 0;
        restartString = SceneManager.GetActiveScene().name;
    }

    public void ResumeButton()
    {   
        Time.timeScale = 1;
        gameObject.SetActive(false);        
    }

    public void RestartButton()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(restartString);
    }

    public void MainMenu() {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }
}
