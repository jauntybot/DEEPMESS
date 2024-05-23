using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResolutionManager {



    Resolution[] resolutions;
    List<Resolution> filteredResolutions;

    float currentRefereshRate;
    int currentResolutionIndex;

    public ResolutionManager() {
        resolutions = Screen.resolutions;
        filteredResolutions = new();

        SetResolution(0);
    }

    public void SetResolution(int resolutionIndex) {
        if (filteredResolutions.Count > 0) {
            Resolution resolution = filteredResolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, true);
        }
    }

}
