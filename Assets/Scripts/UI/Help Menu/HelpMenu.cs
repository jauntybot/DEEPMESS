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
        for (int i = 0; i <= sectionsContainer.childCount - 1; i++) {
            HelpSection section = sectionsContainer.GetChild(i).GetComponent<HelpSection>();
            if (section) {
                helpSections.Add(section);
                section.Initialize(this);
            }
        }
        helpSections[0].SelectSection(true);
    }

    public void SwitchSections(HelpSection newSection) {
        foreach (HelpSection section in helpSections)
            section.SelectSection(false);
        if (newSection.gif)
            explainer.UpdateExplaination(newSection.title, newSection.anims);
        else
            explainer.UpdateExplaination(newSection.title, newSection.image);
    }


}
