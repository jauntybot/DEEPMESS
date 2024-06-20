using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HelpSection : MonoBehaviour {

    Button button;
    HelpMenu helpMenu;
    [SerializeField] TMPro.TMP_Text titleTMP;
    [SerializeField] GameObject selected;
    public string title;

    public bool gif;
    public Sprite image;
    public List<RuntimeAnimatorController> anims;
    [SerializeField] Color normal, highlight, pressed, select;

    public void Initialize(HelpMenu menu) {
        titleTMP.text = title;
        helpMenu = menu;
        button = titleTMP.gameObject.GetComponent<Button>();
    }

    public void SelectSection(bool state) {
        if (helpMenu && state) {
            helpMenu.SwitchSections(this);
            selected.SetActive(true);
            titleTMP.fontMaterial.color = Color.black;
            var colors = button.colors;
            colors.selectedColor = select;
            colors.normalColor = select;
            colors.highlightedColor = select;
            colors.pressedColor = select;
            button.colors = colors;
        } else if (!state) {
            selected.SetActive(false);
            titleTMP.fontMaterial.color = Color.white;
            var colors = button.colors;
            colors.selectedColor = normal;
            colors.normalColor = normal;
            colors.highlightedColor = highlight;
            colors.pressedColor = pressed;
            button.colors = colors;
        }
    }

}
