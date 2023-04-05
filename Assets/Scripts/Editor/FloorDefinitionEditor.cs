using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FloorDefinition))]
public class FloorDefinitionEditor : Editor
{
    public override void OnInspectorGUI() 
    {
        FloorDefinition arg = target as FloorDefinition;
        if (arg.atlas) {
            if (GUILayout.Button("Open FloorEditor"))
                FloorEditor.Init(arg);
        } else {
            GUILayout.Box("Serialize a FloorAtlas to open FloorEditor.");
        }    
        base.OnInspectorGUI();
    }

}
