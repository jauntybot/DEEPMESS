using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LevelEditor : EditorWindow {



    static LevelDefinition lvl;

    public static void Init(LevelDefinition _lvl)
    {
        LevelEditor window = (LevelEditor)
            EditorWindow.GetWindow(typeof(LevelEditor), true, "Level Editor");
        window.Show();

        lvl = _lvl;
    }

    void OnGUI()
    {
        Event evt = Event.current;
        Rect h = (Rect)EditorGUILayout.BeginVertical();
        for (int y = 0; y < 8; y++) {
            Rect r = (Rect)EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < 8; x++) {
                if (GUILayout.Button("("+x+","+y+")")) {
                    Vector2 mousePos = evt.mousePosition;
                    SquareContentsEditor.Init(mousePos);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
    }



}

