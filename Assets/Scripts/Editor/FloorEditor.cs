using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FloorEditor : EditorWindow {

    static FloorEditor window = null;
    static FloorDefinition lvl;
    static CoordEditorPopup popup;
    static Dictionary<string, Texture> options = new Dictionary<string, Texture>();
    static Vector2 activeCoord;

    public static void Init(FloorDefinition _lvl)
    {
        window = (FloorEditor)
            EditorWindow.GetWindow(typeof(FloorEditor), false, "Floor Editor");
        window.minSize = new Vector2(825, 820);
        window.Show();

        lvl = _lvl;
        activeCoord = Vector2.zero;
        ReloadAssetsFromAtlas();
        Dictionary<string, Texture> options = new Dictionary<string, Texture>();
        for (int i = 0; i < lvl.atlas.assets.Count; i++) {
            options.Add(lvl.atlas.assets[i].name, lvl.atlas.assets[i].icon);
        }
        CoordEditorPopup.Init(options, lvl);
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
        if (window)
            window.hasUnsavedChanges = true;
    }

    string Notation(int x) {
        switch(x) {
            default: return "a";
            case 0: return "a";
            case 1: return "b";
            case 2: return "c";
            case 3: return "d";
            case 4: return "e";
            case 5: return "f";
            case 6: return "g";
            case 7: return "h";
        }
    }


    void OnGUI()
    {
        if (lvl) {
            Event evt = Event.current;
            Rect h = (Rect)EditorGUILayout.BeginVertical();
            using (new EditorGUI.DisabledScope(!hasUnsavedChanges))
            {
            GUILayout.BeginArea(new Rect(0,0, 400, 35));
                if (GUILayout.Button("Discard Changes"))
                    DiscardChanges();
            GUILayout.EndArea();
            GUILayout.BeginArea(new Rect(400,0, 400, 35));
                if (GUILayout.Button("Save Floor"))
                    SaveChanges();
            GUILayout.EndArea();
            }
            for (int y = 0; y < 8; y++) {
                Rect r = (Rect)EditorGUILayout.BeginHorizontal();
                for (int x = 7; x >= 0; x--) {
                    Spawn spawn = lvl.initSpawns.Find(s => s.coord == new Vector2(x, y));
                    Texture buttonSprite = lvl.atlas.assets[0].icon;
                    if (spawn != null) buttonSprite = spawn.asset.icon;
                    GUILayout.BeginArea(new Rect(x*101, (7-y)*101 + 35, 100, 100));
                    if (GUILayout.Button(buttonSprite, GUILayout.Width(100), GUILayout.Height(75))) {
                        Vector2 mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                        activeCoord = new Vector2(x,y);
                        CoordEditorPopup.Reload(mousePos);    
                    }
                    GUILayout.Box(Notation(x)+(y+1));
                    GUILayout.EndArea();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
    } 

    protected static void ReloadAssetsFromAtlas() {
        for (int i = lvl.initSpawns.Count - 1; i >= 0; i--) {
            lvl.initSpawns[i].asset = lvl.atlas.assets.Find(a => a.name == lvl.initSpawns[i].asset.name);
        }
    }

    public override void SaveChanges()
    {
        // Your custom save procedures here
        EditorUtility.SetDirty(lvl);
        Debug.Log($"{this} saved successfully!!!");
        base.SaveChanges();
    }

    public override void DiscardChanges()
    {
        // Your custom procedures to discard changes

        Debug.Log($"{this} discarded changes!!!");
        base.DiscardChanges();
    }
}
