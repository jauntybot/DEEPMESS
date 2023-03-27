using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LevelEditor : EditorWindow {

    static LevelDefinition lvl;
    static CoordEditorPopup popup;
    static Dictionary<string, Texture> options = new Dictionary<string, Texture>();
    static Vector2 activeCoord;

    public static void Init(LevelDefinition _lvl)
    {
        LevelEditor window = (LevelEditor)
            EditorWindow.GetWindow(typeof(LevelEditor), true, "Level Editor");
        window.minSize = new Vector2(825, 820);
        window.Show();

        lvl = _lvl;
        for (int i = 0; i < lvl.atlas.assets.Count; i++) {
            options.Add(lvl.atlas.assets[i].name, lvl.atlas.assets[i].icon);
        }
        CoordEditorPopup.Init(options);
    }

    public static void UpdateCoord(string content = null) {
        Spawn spawn = lvl.initSpawns.Find(s => s.coord == activeCoord);
        if (spawn != null) {
            if (content == null)
                lvl.initSpawns.Remove(spawn);
            else
                spawn.asset = lvl.atlas.assets.Find(a => a.name == content);
        } else {
            spawn = new Spawn();
            spawn.coord = activeCoord;
            spawn.asset = lvl.atlas.assets.Find(a => a.name == content);
            lvl.initSpawns.Add(spawn);
        }
    }


    void OnGUI()
    {
        Event evt = Event.current;
        Rect h = (Rect)EditorGUILayout.BeginVertical();
        for (int y = 0; y < 8; y++) {
            Rect r = (Rect)EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < 8; x++) {
                Spawn spawn = lvl.initSpawns.Find(s => s.coord == new Vector2(x, y));
                Texture buttonSprite = lvl.atlas.assets[0].icon;
                if (spawn != null) buttonSprite = spawn.asset.icon;
                if (GUILayout.Button(buttonSprite, GUILayout.Width(100), GUILayout.Height(100))) {
                    Vector2 mousePos = evt.mousePosition;
                    activeCoord = new Vector2(x,y);
                    CoordEditorPopup.Reload(mousePos);    
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
    }
}
