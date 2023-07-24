using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FloorDefinition))]
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
    public override void OnInspectorGUI() 
    {
        arg.floorType = (FloorDefinition.FloorType)EditorGUILayout.EnumPopup("Floor Type", arg.floorType);
        if (arg.floorType == FloorDefinition.FloorType.Combat) {
            arg.atlas = EditorGUILayout.ObjectField("Atlas", arg.atlas, typeof(FloorAtlas), false) as FloorAtlas;
            if (arg.atlas) {
                if (GUILayout.Button("Open FloorEditor"))
                    FloorEditor.Init(serializedObject);
                EditorList.Show(tar.FindProperty("initSpawns"));
            } else {
                GUILayout.Label("Serialize a FloorAtlas to open FloorEditor.");
            }    
        } else if (arg.floorType == FloorDefinition.FloorType.SlotMachine) {
            arg.slotsType = (FloorDefinition.SlotsType)EditorGUILayout.EnumPopup("Slots Type", arg.slotsType);
            if (arg.slotsType == FloorDefinition.SlotsType.Equipment) {
                EditorList.Show(equipmentGenList, EditorList.EditorListOption.ListLabel);
            }
        }
        //base.OnInspectorGUI();
    }
 
}

