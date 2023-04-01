using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


class CoordEditorPopup : EditorWindow
{
    static EditorWindow window = null;
    static List<PopupButton> buttons = new List<PopupButton>();

    public static void Init(Dictionary<string, Texture> _options) {
        buttons = new List<PopupButton>();
        foreach(KeyValuePair<string, Texture> entry in _options) {
            PopupButton button = new PopupButton();
            button.name = entry.Key;
            button.icon = entry.Value;
            buttons.Add(button);
        }
    }

    public static void Reload(Vector2 pos) {
        window = GetWindow<CoordEditorPopup>(true, "Content");
        window.position = new Rect(pos.x, pos.y, 105, buttons.Count * 101);
        window.Show();
        
    }

    void OnGUI() {
        Rect h = EditorGUILayout.BeginVertical();
        for (int i = 0; i < buttons.Count; i++) {
            if (GUILayout.Button(buttons[i].icon, GUILayout.Width(100), GUILayout.Height(100))) {
                FloorEditor.UpdateCoord(buttons[i].name);
                if (i == 0) 
                    FloorEditor.UpdateCoord();
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

