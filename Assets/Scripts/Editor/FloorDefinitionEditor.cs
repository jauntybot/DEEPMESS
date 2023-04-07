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
        arg.floorType = (FloorDefinition.FloorType)EditorGUILayout.EnumPopup("Floor Type", arg.floorType);
        if (arg.floorType == FloorDefinition.FloorType.Combat) {
            arg.atlas = EditorGUILayout.ObjectField("Atlas", arg.atlas, typeof(FloorAtlas), false) as FloorAtlas;
            if (arg.atlas) {
                if (GUILayout.Button("Open FloorEditor"))
                    FloorEditor.Init(arg);
            } else {
                GUILayout.Box("Serialize a FloorAtlas to open FloorEditor.");
            }    
        } else {
            
        }
        //base.OnInspectorGUI();
    }

}
