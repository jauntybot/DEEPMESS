using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(OutlineGenerator))]
public class OutlineGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        OutlineGenerator arg = target as OutlineGenerator;
        base.OnInspectorGUI();
            if (GUILayout.Button("Generate Outline"))
                arg.GenerateOutline();
    }
}
