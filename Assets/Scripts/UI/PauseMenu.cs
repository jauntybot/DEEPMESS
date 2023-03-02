using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject musicController;
    AudioSource audioSource;

    void Start()
    {
        audioSource = musicController.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.activeSelf){
            //game is paused
            audioSource.volume = .10f;
        }
    }

    public void ResumeButton()
    {
        gameObject.SetActive(false);
        audioSource.volume = 1f;
    }

    public void RestartButton()
    {
        gameObject.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
