using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FloorDefinition))]
public class FloorDefinitionEditor : Editor
{
    public override void OnInspectorGUI() 
    {
        //SerializedObject so = target;
        FloorDefinition arg = target as FloorDefinition;
        arg.floorType = (FloorDefinition.FloorType)EditorGUILayout.EnumPopup("Floor Type", arg.floorType);
        if (arg.floorType == FloorDefinition.FloorType.Combat) {
            arg.atlas = EditorGUILayout.ObjectField("Atlas", arg.atlas, typeof(FloorAtlas), false) as FloorAtlas;
            if (arg.atlas) {
                if (GUILayout.Button("Open FloorEditor"))
                    FloorEditor.Init(arg);
                //EditorList.Show(arg.FindProperty("initSpawns"));
            } else {
                GUILayout.Box("Serialize a FloorAtlas to open FloorEditor.");
            }    
        } else if (arg.floorType == FloorDefinition.FloorType.SlotMachine) {
            arg.slotsType = (FloorDefinition.SlotsType)EditorGUILayout.EnumPopup("Slots Type", arg.slotsType);
            if (arg.slotsType == FloorDefinition.SlotsType.Equipment) {
                //arg.equipmentTable = EditorGUILayout.ObjectField(arg.equipmentTable, typeof(EquipmentTable), false) as EquipmentTable;
            }
        }
        //base.OnInspectorGUI();
    }

}
