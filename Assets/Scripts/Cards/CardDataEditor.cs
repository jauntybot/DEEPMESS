using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CardData))]
public class CardDataEditor : Editor
{
    public override void OnInspectorGUI() 
    {
        base.OnInspectorGUI();
        CardData arg = target as CardData;


        if (arg.action == CardData.Action.Move) {
            arg.adjacency = (CardData.AdjacencyType)EditorGUILayout.EnumPopup("Adjacency Type", arg.adjacency);
            arg.range = EditorGUILayout.IntField("Move Range", arg.range);
        }

        if (arg.action == CardData.Action.Attack) {
            arg.adjacency = (CardData.AdjacencyType)EditorGUILayout.EnumPopup("Adjacency Type", arg.adjacency);
            arg.range = EditorGUILayout.IntField("Attack Range", arg.range);
            arg.dmg = EditorGUILayout.IntField("Damage", arg.dmg);
        }        

        if (arg.action == CardData.Action.Defend) {
            arg.shield = EditorGUILayout.IntField("Shield Value", arg.shield);
        }
    }
}
