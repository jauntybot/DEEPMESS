using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FloorDefinition))]
public class FloorDefinitionEditor : Editor
{
    FloorDefinition t;
    SerializedObject getTarget;
    SerializedProperty equipmentGenList;

 
    void OnEnable(){
        t = target as FloorDefinition;
        getTarget = new SerializedObject(t);
        equipmentGenList = getTarget.FindProperty("equipmentTable"); // Find the List in our script and create a refrence of it
        
    }
    public override void OnInspectorGUI() 
    {
        //getTarget.Update();

        t.floorType = (FloorDefinition.FloorType)EditorGUILayout.EnumPopup("Floor Type", t.floorType);
        if (t.floorType == FloorDefinition.FloorType.Combat) {
            t.atlas = EditorGUILayout.ObjectField("Atlas", t.atlas, typeof(FloorAtlas), false) as FloorAtlas;
            if (t.atlas) {
                if (GUILayout.Button("Open FloorEditor"))
                    FloorEditor.Init(t);
                //EditorList.Show(t.FindProperty("initSpawns"));
            } else {
                GUILayout.Label("Serialize a FloorAtlas to open FloorEditor.");
            }    
        } else if (t.floorType == FloorDefinition.FloorType.SlotMachine) {
            t.slotsType = (FloorDefinition.SlotsType)EditorGUILayout.EnumPopup("Slots Type", t.slotsType);
            if (t.slotsType == FloorDefinition.SlotsType.Equipment) {
                EditorList.Show(equipmentGenList, EditorList.EditorListOption.ListLabel);
            }
        }
        //base.OnInspectorGUI();
    }
 
}

