using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script that generates and maintains the grid
// Stores all Grid Elements that are created

public class Grid : MonoBehaviour {
    
    FloorManager floorManager;
    [SerializeField] PlayerManager player;
    public int index = 0;
    public GameObject gridContainer, neutralGEContainer;
    [SerializeField] GameObject chessNotation;
    private bool notation = false;
    public UnitManager enemy;
    [SerializeField] GameObject enemyPrefab;

    [SerializeField] public Vector2 ORTHO_OFFSET = new Vector2(1.15f, 0.35f);
    [SerializeField] GameObject sqrPrefab, bloodTilePrefab, bileTilePrefab, gridCursor, selectedCursor;
    [SerializeField] static float fadeInDur = 0.25f;
    public FloorDefinition lvlDef;
    [SerializeField] Color offWhite;

    public List<GridSquare> sqrs = new List<GridSquare>();
    public List<GridElement> gridElements = new List<GridElement>();

    [HideInInspector] public bool overrideHighlight;


    void Awake() {
        if (FloorManager.instance) floorManager = FloorManager.instance;
        if (PlayerManager.instance) player = PlayerManager.instance;
    }

// loop through grid x,y, generate sqr grid elements, update them and add to list
    public IEnumerator GenerateGrid(int i, GameObject enemyOverride = null) {
        List<Vector2> bloodTiles = new List<Vector2>();
        List<Vector2> bileTiles = new List<Vector2>();
        foreach (FloorDefinition.Spawn spawn in lvlDef.initSpawns) {
            if (spawn.asset.prefab.GetComponent<GridElement>() is GridSquare s) {
                if (s.tileType == GridSquare.TileType.Blood)
                    bloodTiles.Add(spawn.coord);
                else if (s.tileType == GridSquare.TileType.Bile)
                    bileTiles.Add(spawn.coord);
            }
        }
        for (int y = 0; y < FloorManager.gridSize; y++) {
            for (int x = 0; x < FloorManager.gridSize; x++) {
                GridSquare sqr = null;
                if (bloodTiles.Contains(new Vector2(x,y)))
                    sqr = Instantiate(bloodTilePrefab, this.transform).GetComponent<GridSquare>();
                else if (bileTiles.Contains(new Vector2(x,y)))
                    sqr = Instantiate(bileTilePrefab, this.transform).GetComponent<GridSquare>();
                else
                    sqr = Instantiate(sqrPrefab, this.transform).GetComponent<GridSquare>();
//store bool for white sqrs
                sqr.white=false;
                if (x%2==0) { if (y%2==0) sqr.white=true; } 
                else { if (y%2!=0) sqr.white=true; }

                sqr.StoreInGrid(this);
                sqr.UpdateElement(new Vector2(x,y));

                sqrs.Add(sqr);
                sqr.transform.parent = gridContainer.transform;
            }
        }
        
        selectedCursor.transform.localScale = Vector3.one * FloorManager.sqrSize;
        selectedCursor.transform.SetAsLastSibling();
        gridCursor.transform.localScale = Vector3.one * FloorManager.sqrSize;
        gridCursor.transform.SetAsLastSibling();
        index = i;

        SpawnLevelDefinition(enemyOverride);
       
        NestedFadeGroup.NestedFadeGroup fade = GetComponent<NestedFadeGroup.NestedFadeGroup>();

        float timer = 0;
        while (timer <= fadeInDur) {
            fade.AlphaSelf = Mathf.Lerp(0, 1, timer/fadeInDur);

            timer += Time.deltaTime;
            yield return null;
        }
        LockGrid(true);
        fade.AlphaSelf = 1;
    }

// Function that instantiates and houses FloorAssets that are defined by the currently loaded Floor Definition
    void SpawnLevelDefinition(GameObject enemyOverride = null) {
        List<Vector2> nailSpawns = new List<Vector2>();
        List<Vector2> slagSpawns = new List<Vector2>();
// Create new EnemyManager if this floor is not overriden by Tutorial
        enemy = Instantiate(enemyOverride == null ? enemyPrefab : enemyOverride, this.transform).GetComponent<EnemyManager>(); 
        enemy.transform.SetSiblingIndex(2);
        enemy.StartCoroutine(enemy.Initialize());
        
        foreach (FloorDefinition.Spawn spawn in lvlDef.initSpawns) {
            GridElement ge = spawn.asset.prefab.GetComponent<GridElement>();
// Spawn a Unit
            if (ge is Unit u) 
            {
                if (u is EnemyUnit e) {
                    enemy.SpawnUnit(spawn.coord, spawn.asset.prefab.GetComponent<Unit>());
                } else if (u is PlayerUnit) {
                    slagSpawns.Add(spawn.coord);
                }
                else if (u is Nail) {
                    nailSpawns.Add(spawn.coord);
                }
// Spawn a Neutral Grid Element
            } else if (ge is not GridSquare) {
                GridElement neutralGE = Instantiate(spawn.asset.prefab, this.transform).GetComponent<GridElement>();
                neutralGE.transform.parent = neutralGEContainer.transform;

                neutralGE.StoreInGrid(this);
                neutralGE.UpdateElement(spawn.coord);
            }
        }
        floorManager.nailSpawnOverrides = nailSpawns;
        floorManager.playerDropOverrides = slagSpawns;
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

    public void UpdateTargetCursor(bool state, Vector2 coord) {
        gridCursor.SetActive(state);
        gridCursor.transform.position = PosFromCoord(coord);
        gridCursor.GetComponentInChildren<SpriteRenderer>().sortingOrder = SortOrderFromCoord(coord);
        if (overrideHighlight)
            gridCursor.SetActive(false);
    }

    public void UpdateSelectedCursor(bool state, Vector2 coord) {
        selectedCursor.SetActive(state);
        selectedCursor.transform.position = PosFromCoord(coord);
        selectedCursor.GetComponentInChildren<SpriteRenderer>().sortingOrder = SortOrderFromCoord(coord);
        if (overrideHighlight)
            selectedCursor.SetActive(false);
    }

// Toggle GridSquare highlights, apply color by index
    public void DisplayValidCoords(List<Vector2> coords, int? index = null, bool stack = false, bool fill = true) {
        if (!stack)
            DisableGridHighlight();

        Color c = floorManager.playerColor;
        if (index is int i)
            c = floorManager.GetFloorColor(i);

        if (coords != null) {
            foreach (Vector2 coord in coords) {
                if (sqrs.Find(sqr => sqr.coord == coord))
                    sqrs.Find(sqr => sqr.coord == coord).ToggleValidCoord(true, c, fill);
            }
        }

        if (overrideHighlight) DisableGridHighlight();
    }

// Disable GridSquare highlights
    public void DisableGridHighlight() {
        foreach(GridSquare sqr in sqrs)
            sqr.ToggleValidCoord(false);
    }

    public void LockGrid(bool state) {
        foreach (GridSquare sqr in sqrs)
            sqr.ToggleHitBox(!state);
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