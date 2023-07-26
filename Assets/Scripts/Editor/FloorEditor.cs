using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FloorEditor : EditorWindow {

    static FloorEditor window = null;
    static FloorDefinition lvl;
    List<FloorDefinition.Spawn> spawns = new List<FloorDefinition.Spawn>();

    [SerializeField]
    SerializedProperty previewLvl;
    static Dictionary<string, Texture> options = new Dictionary<string, Texture>();

    static Vector2 activeCoord;
    enum AssetType { Environment, Unit };
    static AssetType assetDropdown;
    static FloorAsset activeAsset;

    static Rect headerSection;
    static Rect assetSection;
    Vector2 assetScrollPos;
    static Vector2 assetDim = new Vector2(100, 100);
    static float sectionPadding = 10;
    static Rect gridSection;
    static Rect previewSection;

    public static void Init(FloorDefinition _lvl)
    {
// Create Editor window
        window = (FloorEditor)
            EditorWindow.GetWindow(typeof(FloorEditor), false, "Floor Editor");
        window.minSize = new Vector2(825, 820);
        window.Show();

// Initialize FloorDefinition and Editor values
        lvl = _lvl;
        
        activeCoord = Vector2.zero;
        assetDropdown = AssetType.Environment;
        activeAsset = lvl.atlas.baseTile;
        ReloadAssetsFromAtlas();
        LayoutInit();
    }


    static void LayoutInit() {

        headerSection.x = 0; headerSection.y = 0;
        headerSection.height = 50 + sectionPadding;

        assetSection.x = 0; assetSection.y = headerSection.height;
        assetSection.width = assetDim.x + sectionPadding*2; 

        gridSection.x = assetSection.width; gridSection.y = headerSection.height;
        gridSection.width = (assetDim.x+1) * 8 + sectionPadding; gridSection.height = (assetDim.y+1) * 8 + sectionPadding;

        previewSection.x = gridSection.xMax; previewSection.y = headerSection.height;
        previewSection.width = (assetDim.x+1)*4 + sectionPadding; previewSection.height = gridSection.height;

        assetSection.height = gridSection.height;
        headerSection.width = gridSection.xMax;

        //window.minSize = new Vector2(gridSection.xMax, gridSection.yMax);
        //window.maxSize = new Vector2(gridSection.xMax, gridSection.yMax);

    }

    public static void UpdateCoord(FloorAsset asset) {
        Undo.RecordObject(lvl, "Setting Value");
        FloorDefinition.Spawn spawn = lvl.initSpawns.Find(s => s.coord == activeCoord);
        if (spawn != null) {
            if (asset == lvl.atlas.baseTile)
                lvl.initSpawns.Remove(spawn);
            else
                spawn.asset = asset;
        } else if (asset != lvl.atlas.baseTile) {
            spawn = new FloorDefinition.Spawn();
            spawn.coord = activeCoord;
            spawn.asset = asset;
            lvl.initSpawns.Add(spawn);
        } else
            return;

        EditorUtility.SetDirty(lvl);
        PrefabUtility.RecordPrefabInstancePropertyModifications(lvl);
        if (window)
            window.hasUnsavedChanges = true;
    }


    void OnGUI()
    {
        DrawHeader();
        DrawAssets();
        DrawGrid();
        DrawPreview();
    } 


    void DrawHeader() {
        EditorGUI.DrawRect(headerSection, Color.black);
        Rect r = headerSection;
        r.x+=sectionPadding/2; r.xMax-=sectionPadding;
        r.y+=sectionPadding/2; r.yMax-=sectionPadding;

        Event evt = Event.current;

        GUILayout.BeginArea(r);
        GUILayout.Label("Editing: " + lvl.name);

        GUILayout.BeginHorizontal();
        using (new EditorGUI.DisabledScope(!hasUnsavedChanges)) {
            if (GUILayout.Button("Discard Changes"))
                DiscardChanges();
            if (GUILayout.Button("Save Floor"))
                SaveChanges();
        }
        GUILayout.EndHorizontal();

        DrawUILine(Color.white);

        GUILayout.EndArea();
    }

    void DrawAssets() {
        EditorGUI.DrawRect(assetSection, Color.black);
        Rect r = assetSection;
        r.x+=sectionPadding/2; r.xMax-=sectionPadding;
        r.y+=sectionPadding/2; r.yMax-=sectionPadding;

        GUILayout.BeginArea(r);
        GUILayout.Label("Placing:");
        
        GUILayout.Box(AssetPreview.GetAssetPreview(activeAsset.icon), GUILayout.Width(assetDim.x), GUILayout.Height(assetDim.y));
        GUILayout.Label(activeAsset.name);
        DrawUILine(Color.white);

        GUILayout.Label("Assets");
        assetDropdown = (AssetType)EditorGUILayout.EnumPopup(assetDropdown);

        List<FloorAsset> currentAssets = new List<FloorAsset>();
        switch(assetDropdown) {
            default:
            case AssetType.Environment:
                currentAssets = lvl.atlas.environmentAssets;
            break;
            case AssetType.Unit:
                currentAssets = lvl.atlas.unitAssets;
            break;
        }

        r.xMin = 0;  r.yMin+=135; 
        GUILayout.BeginArea(r);
        GUILayout.BeginVertical();
        assetScrollPos = EditorGUILayout.BeginScrollView(assetScrollPos, false, true);
        
        for (int i = 0; i < currentAssets.Count; i++) {
            GUILayout.BeginArea(new Rect(0, i*(assetDim.y), assetDim.x, assetDim.y));
            if (GUILayout.Button(AssetPreview.GetAssetPreview(currentAssets[i].icon), GUILayout.Width(assetDim.x), GUILayout.Height(assetDim.y))) {
                activeAsset = currentAssets[i];
            }
            GUILayout.EndArea();
            GUILayout.BeginArea(new Rect(0, i*(assetDim.y), assetDim.x, assetDim.y));
            GUILayout.FlexibleSpace();
            GUILayout.Box(currentAssets[i].name);
            GUILayout.EndArea();
        }
        EditorGUILayout.EndScrollView();
        GUILayout.EndVertical();
        GUILayout.EndArea();
        GUILayout.EndArea();
    }

    void DrawGrid() {
        //EditorGUI.DrawRect(gridSection, Color.black);
        Rect r = gridSection;
        r.x+=sectionPadding/2; r.xMax-=sectionPadding;
        r.y+=sectionPadding/2; r.yMax-=sectionPadding;

        GUILayout.BeginArea(r);

        for (int y = 0; y < 8; y++) {
            for (int x = 7; x >= 0; x--) {
                FloorDefinition.Spawn spawn = null;
                if (lvl.initSpawns.Count > 0)
                    spawn = lvl.initSpawns.Find(s => s.coord == new Vector2(x, y));
                Texture2D buttonSprite = AssetPreview.GetAssetPreview(lvl.atlas.baseTile.icon);
                if (spawn != null) buttonSprite = AssetPreview.GetAssetPreview(spawn.asset.icon);
                
                GUILayout.BeginArea(new Rect(x*(assetDim.x+1), (7-y)*(assetDim.y+1), assetDim.x, assetDim.y));
    
                if (GUILayout.Button(buttonSprite, GUILayout.Width(assetDim.x), GUILayout.Height(assetDim.y))) {
                    activeCoord = new Vector2(x,y);
                    UpdateCoord(activeAsset);
                }
                GUILayout.EndArea();
                GUILayout.BeginArea(new Rect(x*(assetDim.x+1), (7-y)*(assetDim.y+1), assetDim.x, assetDim.y));
                GUILayout.FlexibleSpace();
                GUILayout.Box(Notation(x)+(y+1));
                GUILayout.EndArea();
            }
        }

        GUILayout.EndArea();
    }

    void DrawPreview() {
        Rect r = previewSection;
        r.x+=sectionPadding/2; r.xMax-=sectionPadding;
        r.y+=sectionPadding/2; r.yMax-=sectionPadding;

        GUILayout.BeginArea(r);

        //GUILayout.BeginArea(new Rect(0, r.y/3, r.width, r.height/3));
        for (int y = 0; y < 8; y++) {
            for (int x = 7; x >= 0; x--) {
                GUILayout.BeginArea(new Rect(x * (assetDim.x+1)/2, (7-y) * (assetDim.y+1)/2, assetDim.x/2, assetDim.y/2));
                FloorAsset asset = lvl.atlas.baseTile;
                if (lvl.initSpawns.Find(a => a.coord == new Vector2(x,y)) != null)
                    asset = lvl.initSpawns.Find(a => a.coord == new Vector2(x,y)).asset;
                GUILayout.Box(AssetPreview.GetAssetPreview(asset.icon), GUILayout.Width(assetDim.x/2), GUILayout.Height(assetDim.y/2));
                GUILayout.EndArea();
                GUILayout.BeginArea(new Rect(x * (assetDim.x+1)/2, (7-y) * (assetDim.y+1)/2, assetDim.x/2, assetDim.y/2));
                GUILayout.FlexibleSpace();
                GUILayout.Box(Notation(x)+(y+1));
                GUILayout.EndArea();
            }
        }

        
        //GUILayout.EndArea();

        GUILayout.EndArea();
    }


    protected static void ReloadAssetsFromAtlas() {
        if (lvl.initSpawns.Count > 0) {
            for (int i = lvl.initSpawns.Count - 1; i >= 0; i--) {
                FloorAsset reload = lvl.atlas.environmentAssets.Find(a => a == lvl.initSpawns[i].asset);
                if (reload != null) {
                    lvl.initSpawns[i].asset = reload;
                    Debug.Log("Reloaded");
                } else
                    Debug.Log("Reloaded failed");
            }
        }
        window.SaveChanges();
    }

    public static void DrawUILine(Color color, int thickness = 2, int padding = 10) {
        Rect r = new Rect();
        r = EditorGUILayout.GetControlRect(GUILayout.Height(padding+thickness));
        r.height = thickness;
        r.y+=padding/2;
        r.x-=2;
        r.width+=6;            

        EditorGUI.DrawRect(r, color);
    }


    public override void SaveChanges() {
// Set ScriptableObject as dirty so that it is saved in source control
        EditorUtility.SetDirty(lvl);
        Debug.Log(lvl.name + " saved.");
        base.SaveChanges();
    }

    public override void DiscardChanges() {
        Debug.Log(lvl.name + " discarded changes.");
        base.DiscardChanges();
        ReloadAssetsFromAtlas();
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
}
