using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{

    public static MainMenuManager instance;
    void Awake() {
        if (MainMenuManager.instance) {
            Debug.Log("Warning! More than one instance of MainMenuManager found!");
            return;
        }
        instance = this;
    }
    
    public Button optionsButton;
    [SerializeField] GameObject buttonColumn, credits;


    public void ToggleCredits(bool state) {
        buttonColumn.SetActive(!state);
        credits.SetActive(state);
    }
}
