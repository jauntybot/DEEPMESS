using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelDefinition))]
public class LevelDefinitionEditor : Editor
{
    public override void OnInspectorGUI() 
    {
        base.OnInspectorGUI();
        LevelDefinition arg = target as LevelDefinition;


        if (GUILayout.Button("Open Level Editor"))
            LevelEditor.Init(arg);
    }

}
