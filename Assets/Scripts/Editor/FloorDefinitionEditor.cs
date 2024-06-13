using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FloorDefinition))]
[CanEditMultipleObjects]
public class FloorDefinitionEditor : Editor
{
    FloorDefinition arg;
    SerializedObject tar;
    SerializedProperty equipmentGenList;

 
    void OnEnable(){
        arg = target as FloorDefinition;
        tar = new SerializedObject(arg);
        equipmentGenList = tar.FindProperty("equipmentTable"); // Find the List in our script and create a refrence of it
        
    }
    public override void OnInspectorGUI() {
        arg.atlas = EditorGUILayout.ObjectField("Atlas", arg.atlas, typeof(FloorAtlas), false) as FloorAtlas;
        if (arg.atlas) {
            if (GUILayout.Button("Open FloorEditor"))
                FloorEditor.Init(arg);
            //EditorList.Show(tar.FindProperty("initSpawns"));
            EditorGUILayout.LabelField(arg.spawnBeacon?"spawn" : "no spawn");
            
        } else {
            EditorGUILayout.HelpBox("Serialize a FloorAtlas to open FloorEditor.", MessageType.Warning);
        }    
        
    }
 
}

