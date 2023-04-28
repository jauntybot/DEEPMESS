using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{

    [SerializeField] string targetScene;

    public void ChangeSceneButton() {
        SceneManager.LoadScene(targetScene, LoadSceneMode.Single);
    }

}
