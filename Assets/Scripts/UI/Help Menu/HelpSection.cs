using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HelpSection : MonoBehaviour
{

    Button button;
    HelpMenu helpMenu;
    public string title;
    [TextArea] public string body;

    public void Initialize(HelpMenu menu) {
        button = GetComponent<Button>();
        helpMenu = menu;
    }

    public void SelectSection() {
        if (helpMenu) {
            helpMenu.SwitchSections(this);
        }
    }

}
