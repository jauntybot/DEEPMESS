// Create a Vertical Compound Button

using UnityEngine;
using UnityEditor;

public class BeginVerticalExample : EditorWindow
{
    [MenuItem("Examples/Begin-End Vertical usage")]
    static void Init()
    {
        BeginVerticalExample window = (BeginVerticalExample)
            EditorWindow.GetWindow(typeof(BeginVerticalExample), true, "My Empty Window");
        window.Show();
    }

    void OnGUI()
    {
        Rect r = (Rect)EditorGUILayout.BeginVertical("Button");
        if (GUI.Button(r, GUIContent.none))
            Debug.Log("Go here");
        GUILayout.Label("I'm inside the button");
        GUILayout.Label("So am I");
        EditorGUILayout.EndVertical();
    }
}