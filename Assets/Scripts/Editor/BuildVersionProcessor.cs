using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class BuildVersionProcessor : IPreprocessBuildWithReport {

    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report) {
        string[] currentVersion = FindCurrentVersion();
        UpdateVersion(currentVersion);
        PersistentDataManager.instance.NewUser();
    }

    string[] FindCurrentVersion() {
        string[] currentVersion = PlayerSettings.bundleVersion.Split('.');
        Debug.Log(currentVersion);
        return currentVersion;
    }

    void UpdateVersion(string[] version) {
        if (float.TryParse(version[3], out float versionNumber)) {
            float newVersion = versionNumber+1;
            Debug.Log(newVersion);
            PlayerSettings.bundleVersion = version[0]+"."+version[1]+"."+version[2]+"."+newVersion;
        }
    }

}
