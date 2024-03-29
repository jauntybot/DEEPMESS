using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResolutionManager : MonoBehaviour {

    [SerializeField] TMP_Dropdown dropdown;

    Resolution[] resolutions;
    List<Resolution> filteredResolutions;

    float currentRefereshRate;
    int currentResolutionIndex;

    void Start() {
        resolutions = Screen.resolutions;
        filteredResolutions = new();

        dropdown.ClearOptions();
        currentRefereshRate = Screen.currentResolution.refreshRate;

        for (int i = resolutions.Length - 1; i >= 0 ; i--) {
            if (resolutions[i].refreshRate == currentRefereshRate &&
                ((resolutions[i].width == 1280 && resolutions[i].height == 720) ||
                (resolutions[i].width == 1280 && resolutions[i].height == 800) ||
                (resolutions[i].width == 1920 && resolutions[i].height == 1080) ||
                (resolutions[i].width == 1920 && resolutions[i].height == 1200) ||
                (resolutions[i].width == 2560 && resolutions[i].height == 1440) ||
                (resolutions[i].width == 2560 && resolutions[i].height == 1600)))
                filteredResolutions.Add(resolutions[i]);
        }

        List<string> options = new List<string>();
        for (int i = 0; i < filteredResolutions.Count; i++) {
            string resolutionOption = filteredResolutions[i].width + "x" + filteredResolutions[i].height;
            options.Add(resolutionOption);
            if (filteredResolutions[i].width == Screen.width && filteredResolutions[i].height == Screen.height)
                currentResolutionIndex = i;
        }

        if (dropdown) {
            dropdown.AddOptions(options);
            dropdown.value = currentResolutionIndex;
            dropdown.RefreshShownValue();
        }

        SetResolution(0);
    }

    public void SetResolution(int resolutionIndex) {
        if (filteredResolutions.Count > 0) {
            Resolution resolution = filteredResolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, true);
        }
    }

}
