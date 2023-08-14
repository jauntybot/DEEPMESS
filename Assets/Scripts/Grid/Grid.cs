using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

// Script that generates and maintains the grid
// Stores all Grid Elements that are created

public class Grid : MonoBehaviour {
    
    FloorManager floorManager;
    [Header("References")]
    [SerializeField] PlayerManager player;
    public UnitManager enemy;
    public GameObject gridContainer, neutralGEContainer;
    [SerializeField] GameObject chessNotation;
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] GameObject tilePrefab, selectedCursor;
    public bool overrideHighlight;
    

    [Header("Grid Data")]
    public int index = 0;
    public FloorDefinition lvlDef;
    [HideInInspector] public List<Vector2> slagSpawns, nailSpawns;
    private bool notation = false;
    [SerializeField] public Vector2 ORTHO_OFFSET = new Vector2(1.15f, 0.35f);
    [SerializeField] float shockwaveDur = 1.25f, collapseDur = 0.25f;
    [SerializeField] AnimationCurve shockwaveCurve;

    public List<Tile> tiles = new List<Tile>();
    public List<GridElement> gridElements = new List<GridElement>();




    void Awake() {
        if (FloorManager.instance) floorManager = FloorManager.instance;
        if (PlayerManager.instance) player = PlayerManager.instance;
    }

// loop through grid x,y, generate Tile grid elements, update them and add to list
    public void GenerateGrid(int i) {
        List<Vector2> altTiles = new List<Vector2>();

        foreach (FloorDefinition.Spawn spawn in lvlDef.initSpawns) {
            if (spawn.asset.prefab.GetComponent<GridElement>() is Tile) {
                altTiles.Add(spawn.coord);
            }
        }
        for (int y = 0; y < FloorManager.gridSize; y++) {
            for (int x = 0; x < FloorManager.gridSize; x++) {
                Tile tile = null;
                if (altTiles.Contains(new Vector2(x,y)))
                    tile = Instantiate(lvlDef.initSpawns.Find(s => s.coord == new Vector2(x,y)).asset.prefab, transform).GetComponent<Tile>();
                else
                    tile = Instantiate(tilePrefab, transform).GetComponent<Tile>();
// Assign Tile white or black
                tile.white=false;
                if (x%2==0) { if (y%2==0) tile.white=true; } 
                else { if (y%2!=0) tile.white=true; }

                tile.StoreInGrid(this);
                tile.UpdateElement(new Vector2(x,y));

                tiles.Add(tile);
                tile.transform.parent = gridContainer.transform;
            }
        }
        
        selectedCursor.transform.localScale = Vector3.one * FloorManager.sqrSize;
        selectedCursor.transform.SetAsLastSibling();
        index = i;

        SpawnLevelDefinition();
       

        LockGrid(true);
    }

// Function that instantiates and houses FloorAssets that are defined by the currently loaded Floor Definition
    void SpawnLevelDefinition() {
// Create new EnemyManager if this floor is not overriden by Tutorial
        enemy = Instantiate(enemyPrefab, transform).GetComponent<EnemyManager>(); 
        enemy.transform.SetSiblingIndex(2);
        enemy.StartCoroutine(enemy.Initialize(this));
        
        slagSpawns = new(); nailSpawns = new();
        foreach (FloorDefinition.Spawn spawn in lvlDef.initSpawns) {
            GridElement ge = spawn.asset.prefab.GetComponent<GridElement>();
// Spawn a Unit
            if (ge is Unit u) 
            {
                if (u is EnemyUnit e && e is not BossUnit) {
                    enemy.SpawnUnit(spawn.coord, spawn.asset.prefab.GetComponent<Unit>());
                } else if (u is PlayerUnit) {
                    slagSpawns.Add(spawn.coord);
                }
                else if (u is Nail) {
                    nailSpawns.Add(spawn.coord);
                }
// Spawn a Neutral Grid Element
            } else if (ge is not Tile) {
                GridElement neutralGE = Instantiate(spawn.asset.prefab, this.transform).GetComponent<GridElement>();
                neutralGE.transform.parent = neutralGEContainer.transform;

                neutralGE.StoreInGrid(this);
                neutralGE.UpdateElement(spawn.coord);
            }
        }
    }

    public IEnumerator ShockwaveCollapse() {
        Vector2 origin = player.nail.coord;

        float timer = 0;
        StartCoroutine(CollapseTile(origin));
        for (int r = 1; r <= 15; r++) {
            List<Vector2> activeCoords = new();
            for (int i = r; i >= 0; i--) {
                activeCoords.Add(new Vector2(origin.x + r - i, origin.y + i)); 
                activeCoords.Add(new Vector2(origin.x - i, origin.y + r - i)); 
                activeCoords.Add(new Vector2(origin.x - r + i, origin.y - i));
                activeCoords.Add(new Vector2(origin.x + i, origin.y - r + i));
            }
            activeCoords = EquipmentAdjacency.RemoveOffGridCoords(activeCoords);
            if (activeCoords.Count == 0) break;

            foreach (Vector2 coord in activeCoords) 
                StartCoroutine(CollapseTile(coord));

            
            while (timer < (shockwaveDur/15) * r) {
                yield return null;
                timer += Time.deltaTime;
            }
        }
        yield return new WaitForSecondsRealtime(0.25f);
        yield return null;



    }

    public IEnumerator CollapseTile(Vector2 coord) {
        Dictionary<GridElement, Vector2> affected = new();
        Tile tile = tiles.Find(t => t.coord == coord);
        affected.Add(tile, tile.transform.position);
        if (tile.anim) tile.anim.SetTrigger("Collapse");
        foreach (GridElement ge in CoordContents(coord)) {
            affected.Add(ge, ge.transform.position);
            if (ge is Wall w) w.StartCoroutine(w.DestroyElement());
        }

        float timer = 0;
        while (timer <= collapseDur * 2) {
            yield return null;
            foreach (KeyValuePair<GridElement, Vector2> entry in affected) {
                if (entry.Key is Unit u) {
                    u.transform.position =  new Vector3(entry.Value.x, entry.Value.y + shockwaveCurve.Evaluate(timer/collapseDur));
                    u.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = 1 - shockwaveCurve.Evaluate(Mathf.Clamp((timer - collapseDur*2/3), 0, collapseDur/3) / (collapseDur/3));
                }
                else
                entry.Key.transform.position = new Vector3(entry.Value.x, entry.Value.y + Mathf.Sin(shockwaveCurve.Evaluate(timer/collapseDur)*3)/2);
            }
            timer += Time.deltaTime;
        }

    }

    public void ToggleChessNotation(bool state) {
        chessNotation.SetActive(state);
    }

    public void AddElement(GridElement ge) {
        gridElements.Add(ge);
    }

    public void RemoveElement(GridElement ge) 
    {
        gridElements.Remove(ge);
    }

    public void UpdateSelectedCursor(bool state, Vector2 coord) {
        selectedCursor.SetActive(state);
        selectedCursor.transform.position = PosFromCoord(coord);
        selectedCursor.GetComponentInChildren<LineRenderer>().sortingOrder = SortOrderFromCoord(coord);
        if (overrideHighlight)
            selectedCursor.SetActive(false);
    }

// Toggle Tile highlights, apply color by index
    public void DisplayValidCoords(List<Vector2> coords, int? index = null, bool stack = false, bool fill = true) {
        if (!stack)
            DisableGridHighlight();

        Color c = floorManager.playerColor;
        if (index is int i)
            c = floorManager.GetFloorColor(i);

        if (coords != null) {
            foreach (Vector2 coord in coords) {
                if (tiles.Find(tile => tile.coord == coord))
                    tiles.Find(tile => tile.coord == coord).ToggleValidCoord(true, c, fill);
            }
        }

        if (overrideHighlight) DisableGridHighlight();
    }

// Disable Tile highlights
    public void DisableGridHighlight() {
        foreach(Tile tile in tiles)
            tile.ToggleValidCoord(false);
    }

    public void LockGrid(bool state) {
        foreach (Tile tile in tiles)
            tile.ToggleHitBox(!state);
    }

    public List<GridElement> CoordContents(Vector2 coord) {
        return gridElements.FindAll(ge => ge.coord == coord);
    }

     public Vector3 PosFromCoord(Vector2 coord) {
        return new Vector3(
// offset from scene origin + coord to pos conversion + ortho offset + center measure
            transform.position.x - (FloorManager.sqrSize * FloorManager.gridSize * ORTHO_OFFSET.x/1.355f) + (coord.x * FloorManager.sqrSize * ORTHO_OFFSET.x/2.5f) + (ORTHO_OFFSET.x * FloorManager.sqrSize * coord.y) + (FloorManager.sqrSize * ORTHO_OFFSET.x), 
            transform.position.y + (FloorManager.sqrSize * FloorManager.gridSize * ORTHO_OFFSET.y/1.75f) + (coord.y * FloorManager.sqrSize * ORTHO_OFFSET.y/1.1f) - (ORTHO_OFFSET.y*2.1f * FloorManager.sqrSize * coord.x),             
            0);
    }

    public int SortOrderFromCoord(Vector2 coord) {
        return 8 + (int)coord.x - (int)coord.y;
    }
}