using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class FloorEditor : EditorWindow {

    static FloorEditor window = null;
    static FloorDefinition lvl;
    static List<FloorDefinition.Spawn> grid = new();
    static List<FloorDefinition.Spawn> units = new();
    static string dataPath = "Assets/Scriptable Objects/Floors/Floors/";
    static string activePath = "011524 MAGFest Build/Packet 1";

    
    static FloorDefinition previewLvl;
    static Vector2 activeCoord;
    enum AssetLayer { Environment, Unit };
    static bool placing;
    static AssetLayer assetDropdown;
    static FloorAsset activeAsset;

    static Rect headerSection;
    static Rect assetSection;
    static float scrollbarWidth = 30f;
    Vector2 assetScrollPos;
    static Vector2 assetDim = new Vector2(100, 100);
    static float sectionPadding = 10;
    static Rect gridSection;
    static Rect previewSection;
    static bool previewing;
    static bool pNotation;
    static DirectoryInfo rootFolder;
    static Dictionary<DirectoryInfo, DirectoryInfo[]> folders;
    static Dictionary<DirectoryInfo, bool> folderStates;
    static DirectoryInfo[] subFolders;
    static List<FloorDefinition> previewList = new();

    public static void Init(FloorDefinition _lvl) {
    

// Create Editor window
        window = (FloorEditor)
            EditorWindow.GetWindow(typeof(FloorEditor), false, "Floor Editor");
        window.Show();

// Initialize FloorDefinition and Editor values
        lvl = _lvl;
        grid = new(lvl.initSpawns.FindAll(t => t.asset.prefab.GetComponent<GridElement>() is Tile));
        units = new(lvl.initSpawns.FindAll(t => t.asset.prefab.GetComponent<GridElement>() is not Tile));
        previewLvl = _lvl;
        
        activeCoord = Vector2.zero;
        assetDropdown = AssetLayer.Environment;
        activeAsset = lvl.atlas.environmentAssets[0];
        
        UpdatePreviewFolder();    
        ReloadAssetsFromAtlas();
        LayoutInit();
    }


    static void LayoutInit() {
        assetDim = Vector2.one * (window.position.height - 50 - sectionPadding*3)/8;

        headerSection.x = 0; headerSection.y = 0;
        headerSection.height = 50 + sectionPadding;

        assetSection.x = 0; assetSection.y = headerSection.height;
        assetSection.width = assetDim.x + scrollbarWidth; 

        gridSection.x = assetSection.width; gridSection.y = headerSection.height;
        gridSection.width = (assetDim.x+1) * 8 + sectionPadding; gridSection.height = (assetDim.y+1) * 8 + sectionPadding;

        previewSection.x = gridSection.xMax; previewSection.y = headerSection.height;
        previewSection.width = (assetDim.x+1)*4 + sectionPadding; previewSection.height = gridSection.height;

        assetSection.height = gridSection.height;
        previewSection.x = gridSection.xMax; previewSection.y = headerSection.height; 
        previewSection.height = gridSection.height; previewSection.width = previewing ? (assetDim.x+1)*4 + sectionPadding : 30;

        headerSection.width = previewSection.xMax;

        window.minSize = new Vector2(previewSection.xMax, 450);
        window.maxSize = new Vector2(previewSection.xMax, 3000);
    }

    Vector2 prevWindowDim = new();
    void Update() {
        Vector2 windowSize = new Vector2(window.position.width, window.position.height);
        if (!Equals(windowSize, prevWindowDim)) {
            prevWindowDim = windowSize;
            LayoutInit();
        }
    }

    public static void UpdateCoord(FloorAsset asset) {
        List<FloorDefinition.Spawn> activeList = null;
        if (assetDropdown == AssetLayer.Environment) {
            activeList = grid;
            if (asset != null && asset.prefab.GetComponent<GridElement>() is not Tile) activeList = units;
        } else {
            activeList = units;
        }

        FloorDefinition.Spawn spawn = activeList.Find(s => s.coord == activeCoord);
// Place passed asset
        if (placing && asset != null) {
            if (spawn != null) {
                spawn.asset = asset;
            } else {
                spawn = new FloorDefinition.Spawn{
                    coord = activeCoord,
                    asset = asset
                };
                activeList.Add(spawn);
            } 
            window.hasUnsavedChanges = true;
// Remove if asset placed
        } else {
            if (spawn == null && assetDropdown == AssetLayer.Environment) {
                activeList = units;
                spawn = activeList.Find(s => s.coord == activeCoord);
            }
            if (spawn != null && 
                ((assetDropdown == AssetLayer.Environment && lvl.atlas.environmentAssets.Contains(spawn.asset)) || 
                (assetDropdown == AssetLayer.Unit && lvl.atlas.unitAssets.Contains(spawn.asset)))) {
                activeList.Remove(spawn);
                window.hasUnsavedChanges = true;
            }
        }

    }


    void OnGUI() {
        DrawHeader();
        DrawAssets();
        DrawGrid();
        DrawPreviews();
    } 


    void DrawHeader() {
        EditorGUI.DrawRect(headerSection, Color.black);
        Rect r = headerSection;
        r.x+=sectionPadding/2; r.xMax=gridSection.xMax-sectionPadding/2;
        r.y+=sectionPadding/2; r.yMax-=sectionPadding/2;

        GUILayout.BeginArea(r);

        GUILayout.BeginHorizontal();
        float w =  EditorStyles.label.CalcSize(new GUIContent("Editing")).x + 5;
        GUILayout.Label("Editing: ", GUILayout.Width(w));
        lvl.name = GUILayout.TextField(lvl.name);
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        using (new EditorGUI.DisabledScope(!hasUnsavedChanges)) {
            if (GUILayout.Button("Discard Changes", GUILayout.Width(200)))
                DiscardChanges();
            if (GUILayout.Button("Save Floor", GUILayout.Width(200)))
                SaveChanges();
        }
        if (GUILayout.Button("Close Editor", GUILayout.Width(200))) {
            
            window.Close();
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

        GUILayout.BeginHorizontal();
        GUI.enabled = !placing;
        float w =  EditorStyles.label.CalcSize(new GUIContent(" + ")).x + 3;
        if (GUILayout.Button("+", GUILayout.Width(w))) placing = true;
        GUI.enabled = placing;
        w =  EditorStyles.label.CalcSize(new GUIContent(" - ")).x + 3;
        if (GUILayout.Button("-", GUILayout.Width(w))) placing = false;
        GUI.enabled = true;
        w =  EditorStyles.label.CalcSize(new GUIContent(" CLEAR ")).x + 3;
        GUI.enabled = grid.Count > 0 || units.Count > 0;
        if (GUILayout.Button("CLEAR", GUILayout.Width(w))) { grid = new(); units = new(); window.hasUnsavedChanges = true; }
        GUI.enabled = true;
        GUILayout.EndHorizontal();
        
        string title = placing ? "Placing:" : "Removing:";
        GUILayout.Label(title);
        
        
        GUILayout.Box(AssetPreview.GetAssetPreview(activeAsset.icon), GUILayout.Width(assetDim.x), GUILayout.Height(assetDim.y));
        GUILayout.Label(activeAsset.name);
        
        List<FloorAsset> currentAssets;
        switch(assetDropdown) {
            default:
            case AssetLayer.Environment:
                currentAssets = lvl.atlas.environmentAssets;
            break;
            case AssetLayer.Unit:
                currentAssets = lvl.atlas.unitAssets;
            break;
        }

        DrawUILine(Color.white);

        GUILayout.Label("Layer:");
        GUI.changed = false;
        assetDropdown = (AssetLayer)EditorGUILayout.EnumPopup(assetDropdown);
        if (GUI.changed) {
            activeAsset = assetDropdown == AssetLayer.Environment ? lvl.atlas.environmentAssets[0] : lvl.atlas.unitAssets[0];
            placing = true;
        }

        r.x = 0;
        assetScrollPos = EditorGUILayout.BeginScrollView(assetScrollPos, false, false, GUILayout.Width(r.x));
        
        int a = 0;
        for (int i = 0; i < currentAssets.Count; i++) {
            if (currentAssets[i] == lvl.atlas.baseTile) continue;
            if (GUILayout.Button(AssetPreview.GetAssetPreview(currentAssets[i].icon), GUILayout.Width(assetDim.x), GUILayout.Height(assetDim.y))) {
                activeAsset = currentAssets[i];
                placing = true;
            }
            
            GUILayout.BeginArea(new Rect(0, a*(assetDim.y+2), assetDim.x, assetDim.y));
            GUILayout.FlexibleSpace();
            GUILayout.Box(currentAssets[i].name);
            GUILayout.EndArea();
            a++;
        }
        EditorGUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    void DrawGrid() {
        //EditorGUI.DrawRect(gridSection, Color.black);
        Rect r = gridSection;
        r.x+=sectionPadding/2; r.xMax-=sectionPadding;
        r.y+=sectionPadding/2; r.yMax-=sectionPadding;

        GUILayout.BeginArea(r);

        for (int y = 7; y >= 0; y--) {
            for (int x = 0; x <= 7; x++) {
                Rect assetBox = new Rect(x*(assetDim.x+1), (7-y)*(assetDim.y+1), assetDim.x, assetDim.y);
                
// Coordinate button
                GUILayout.BeginArea(assetBox);
                if (GUILayout.Button("", GUILayout.Width(assetDim.x), GUILayout.Height(assetDim.y))) {
                    if (Event.current.button == 0) {
                        activeCoord = new Vector2(7-y, x);
                        UpdateCoord(activeAsset);
                    } else {
                        activeCoord = new Vector2(7-y, x);
                        UpdateCoord(null);
                    }
                }
                GUILayout.EndArea();
                
                Texture2D tileSprite = AssetPreview.GetAssetPreview(lvl.atlas.baseTile.icon);
                FloorDefinition.Spawn spawn = grid.Find(t => t.coord == new Vector2(7-y,x));
                if (spawn != null) tileSprite = AssetPreview.GetAssetPreview(spawn.asset.icon);
                Texture2D unitSprite = null;
                spawn = units.Find(t => t.coord == new Vector2(7-y,x));
                if (spawn != null) unitSprite = AssetPreview.GetAssetPreview(spawn.asset.icon);

                Rect rT = new Rect(assetBox); rT.x += assetDim.x/40; rT.y += assetDim.y/20; rT.width -= assetDim.x/20; rT.height -= assetDim.y/20;
                GUILayout.BeginArea(rT);
                GUILayout.Box(tileSprite, GUIStyle.none);
                GUILayout.EndArea();
                if (unitSprite != null) {
                    Rect uT = new Rect(assetBox); 
                    uT.x += assetDim.x/9; uT.width -= assetDim.x/9; uT.y-=assetDim.y/18; uT.height-=assetDim.y/4.5f;
                    GUILayout.BeginArea(uT);
                    GUILayout.Box(unitSprite, GUIStyle.none);
                    GUILayout.EndArea();
                }


// Notation
                GUILayout.BeginArea(assetBox);
                GUILayout.FlexibleSpace();
                GUILayout.Box(Notation(x)+(y+1));
                GUILayout.EndArea();
            }
        }

        GUILayout.EndArea();
    }

    void DrawPreviews() {
        Rect r = previewSection;
        if (previewing) {
            //GUILayout.BeginHorizontal();    
            Rect grid1 = new Rect(r.x, r.y, (r.width - 35), r.height/2 + 20);
            GUILayout.BeginArea(grid1);
            Preview(grid1, previewLvl);
            GUILayout.EndArea();
            Rect grid2 = new Rect(r.x, r.y + r.height/2 + 20, r.width - 35, r.height/2 - 20);
            GUILayout.BeginArea(grid2);
            //Preview(grid2, lvl, 2);
            ListFloors(grid2);
            GUILayout.EndArea();
            GUILayout.BeginArea(new Rect(r.x + r.width - 30, r.y, 30, r.height));
            if (GUILayout.Button("<", GUILayout.Width(25), GUILayout.Height(r.height))) {
                 previewing = false;
                 LayoutInit();
            }
            GUILayout.EndArea();
            //GUILayout.EndHorizontal();
        } else {
            r.y+=sectionPadding/2; r.yMax-=sectionPadding;
            GUILayout.BeginArea(r);
            if (GUILayout.Button(">", GUILayout.Width(25), GUILayout.Height(r.height))) {
                previewing = true;
                LayoutInit();
            }   
            GUILayout.EndArea();
        }
    }

    void Preview(Rect container, FloorDefinition _lvl) {
        FloorAsset asset = _lvl.atlas.baseTile;
        float scale = container.width/8;
        for (int y = 7; y >= 0; y--) {
            for (int x = 0; x <= 7; x++) {
                GUILayout.BeginArea(new Rect(x * scale, (7-y) * scale, scale, scale));
                asset = _lvl.atlas.baseTile;
                if (_lvl.initSpawns.Find(a => a.coord == new Vector2(7-y, x)) != null)
                    asset = _lvl.initSpawns.Find(a => a.coord == new Vector2(7-y, x)).asset;
                GUILayout.Box(AssetPreview.GetAssetPreview(asset.icon), GUILayout.Width(scale*1.25f), GUILayout.Height(scale*1.25f));
                GUILayout.EndArea();
                if (pNotation) {
                    GUILayout.BeginArea(new Rect(x * scale, (7-y) * scale, scale, scale));
                    GUILayout.FlexibleSpace();
                    GUILayout.Box(Notation(x)+(y+1));
                    GUILayout.EndArea();
                }
            }
        }
        GUILayout.BeginArea(new Rect(0, container.width, container.width, container.height-container.width));
        GUILayout.Label("Previewing: " + _lvl.name);
        GUILayout.BeginHorizontal();
        pNotation = GUILayout.Toggle(pNotation, "Notation");
        float w =  EditorStyles.label.CalcSize(new GUIContent("Editing")).x + 3;
        GUI.enabled = lvl != previewLvl;
        if (GUILayout.Button("EDIT", GUILayout.Width(w))) {
            
            FloorDefinition tmp = lvl;
            lvl = previewLvl;
            grid = new(lvl.initSpawns.FindAll(t => t.asset.prefab.GetComponent<GridElement>() is Tile));
            units = new(lvl.initSpawns.FindAll(t => t.asset.prefab.GetComponent<GridElement>() is not Tile));
        }
        GUI.enabled = true;
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    Vector2 previewScroll = new();
    void ListFloors(Rect container) {
        DrawUILine(Color.white);
        GUILayout.BeginHorizontal();
        float w =  EditorStyles.label.CalcSize(new GUIContent("Editing")).x + 3;
        GUILayout.Label("Floors/", GUILayout.Width(w));
        GUI.changed = false;
        activePath = GUILayout.TextField(activePath);
        //if (GUI.changed) UpdatePreviewFolder();
        GUILayout.EndHorizontal();
        GUILayout.Box("", GUILayout.Height(5));
       
        previewScroll = GUILayout.BeginScrollView(previewScroll, false, true, GUILayout.Width(container.width));
        Folder(rootFolder, 0);
        if (previewList.Count == 0)
            EditorGUILayout.HelpBox("Invalid file path", MessageType.Warning);
        GUILayout.EndScrollView();
    }

    static void UpdatePreviewFolder() {
        rootFolder = new DirectoryInfo(dataPath);

        folders = new();
        folderStates = new();
        RegisterFolder(rootFolder);

        Debug.Log(AssetDatabase.GetAssetPath(lvl.GetInstanceID()));
        activePath = AssetDatabase.GetAssetPath(lvl.GetInstanceID()).TrimEnd((lvl.name+".asset").ToCharArray());
        Debug.Log(dataPath+activePath);
        previewList = LoadAssetsFromFolder<FloorDefinition>(activePath);
    }

    static void RegisterFolder(DirectoryInfo folder) {
        if (!folderStates.ContainsKey(folder))
            folderStates.Add(folder, false);
        DirectoryInfo[] subFolders = folder.GetDirectories();
        foreach (DirectoryInfo f in subFolders) {
            if (!folderStates.ContainsKey(f))
                folderStates.Add(f, false);   
        }
        if (!folders.ContainsKey(folder))
            folders.Add(folder, subFolders);
    }

    static void Folder(DirectoryInfo folder, int subIndex) {
        if (folderStates.ContainsKey(folder)) {
            GUILayout.BeginHorizontal();
            GUILayout.Box("", GUIStyle.none, GUILayout.Width(subIndex*12));
            folderStates[folder] = EditorGUILayout.Foldout(folderStates[folder], folder.Name);
            GUILayout.EndHorizontal();
            if (folderStates[folder]) {
                if (!folders.ContainsKey(folder)) {
                    RegisterFolder(folder);
                }
                
// Draw nesteed folders
                for (int i = 0; i <= folders[folder].Length - 1; i++) 
                    Folder(folders[folder][i], subIndex+1);

// Draw floor definition preview buttons
                string path = "";
                DirectoryInfo index = folder;
                for (int i = 0; i <= subIndex - 1; i++) {
                    path = index.Name + "/" + path;
                    index = index.Parent;
                }
                
                List<FloorDefinition> floors = LoadAssetsFromFolder<FloorDefinition>(dataPath + path);
                for (int i = 0; i <= floors.Count - 1; i++) {
                    GUILayout.BeginHorizontal();
                    GUIStyle style = GUIStyle.none;
                    style.normal.textColor = new Color(0.9f, 0.9f, 0.9f);;
                    style.onHover.textColor = new Color(0.6f, 0.6f, 0.6f);
                    
                    GUILayout.Box("", GUIStyle.none, GUILayout.Width((subIndex+2)*12));
                    if (GUILayout.Button(floors[i].name, style)) {
                        previewLvl = floors[i];
                    }
                    GUILayout.EndHorizontal();
                }
            }
        }
    }



    protected static void ReloadAssetsFromAtlas() {
        if (grid.Count > 0) {
            for (int i = grid.Count - 1; i >= 0; i--) {
                FloorAsset reload = lvl.atlas.environmentAssets.Find(a => a == grid[i].asset);
                if (reload != null) {
                    grid[i].asset = reload;
                }
            }
        }
        //window.SaveChanges();
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
        List<FloorDefinition.Spawn> spawns = new(grid);
        spawns.AddRange(units);
        lvl.initSpawns = new(spawns);
        base.SaveChanges();
        Debug.Log(lvl.name + " saved.");
    }

    public override void DiscardChanges() {
        Debug.Log(lvl.name + " discarded changes.");
        grid = new(lvl.initSpawns.FindAll(t => t.asset.prefab.GetComponent<GridElement>() is Tile));
        units = new(lvl.initSpawns.FindAll(t => t.asset.prefab.GetComponent<GridElement>() is not Tile));
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

    public static List<T> LoadAssetsFromFolder<T>(string folderPath) where T : class {
        List<T> loadedAssets = new List<T>();
        string searchString = $"t:{typeof(T)}";
        string[] assetGUIDs = AssetDatabase.FindAssets(searchString, new[] { folderPath });

        foreach (var assetGUID in assetGUIDs) {
            string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
            T loadedAsset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(T)) as T;
            
            if (assetPath.TrimEnd((loadedAsset.ToString() +".asset").ToCharArray()) == folderPath) {
                loadedAssets.Add(loadedAsset);
            }
        }

        return loadedAssets;
    }
    
}
