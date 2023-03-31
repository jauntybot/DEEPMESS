using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


class CoordEditorPopup : EditorWindow
{
    static List<PopupButton> buttons = new List<PopupButton>();
    static EditorWindow window = null;
    int index = 0;

    public static void Init(Dictionary<string, Texture> _options) {
        foreach(KeyValuePair<string, Texture> entry in _options) {
            PopupButton button = new PopupButton();
            button.name = entry.Key;
            button.icon = entry.Value;
            buttons.Add(button);
        }
    }

    public static void Reload(Vector2 pos) {
        window = GetWindow<CoordEditorPopup>();
        window.position = new Rect(pos.x + 500, pos.y, 105, buttons.Count * 101);
        window.Show();
        
    }

    void OnGUI() {
        Rect h = EditorGUILayout.BeginVertical();
        for (int i = 0; i < buttons.Count; i++) {
            if (GUILayout.Button(buttons[i].icon, GUILayout.Width(100), GUILayout.Height(100))) {
                LevelEditor.UpdateCoord(buttons[i].name);
                if (i == 0) 
                    LevelEditor.UpdateCoord();
                CloseWindow();
            }
        }
        EditorGUILayout.EndVertical();
    }

    public void CloseWindow() {
        window.Close();
    }

}

class PopupButton {
    public string name;
    public Texture icon;
}

