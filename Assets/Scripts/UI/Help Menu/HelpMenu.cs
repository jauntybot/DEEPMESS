using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpMenu : MonoBehaviour
{
    [SerializeField] HelpExplaination explainer;
    [SerializeField] Transform sectionsContainer;
    List<HelpSection> helpSections;

    void Start() {
        helpSections = new();
        for (int i = sectionsContainer.childCount - 1; i >= 0; i--) {
            HelpSection section = sectionsContainer.GetChild(i).GetComponent<HelpSection>();
            if (section) {
                helpSections.Add(section);
                section.Initialize(this);
            }
        }
    }

    public void SwitchSections(HelpSection newSection) {
        foreach (HelpSection section in helpSections)
            section.SelectSection(false);
        if (newSection.gif)
            explainer.UpdateExplaination(newSection.title, newSection.body, newSection.anims);
        else
            explainer.UpdateExplaination(newSection.title, newSection.body, newSection.image);
    }

}
