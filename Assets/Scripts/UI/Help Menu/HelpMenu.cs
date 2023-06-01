using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpMenu : MonoBehaviour
{
    [SerializeField] HelpExplaination explainer;

    [SerializeField] Transform sectionsContainer;

    void Start() {
        for (int i = sectionsContainer.childCount - 1; i >= 0; i--) {
            HelpSection section = sectionsContainer.GetChild(i).GetComponent<HelpSection>();
            if (section) {
                section.Initialize(this);
            }
        }
    }

    public void SwitchSections(HelpSection newSection) {
        explainer.UpdateExplaination(newSection.title, newSection.body, newSection.image);
    }

}
