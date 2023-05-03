using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script that generates and maintains the grid
// Stores all Grid Elements that are created

public class Grid : MonoBehaviour {
    
    FloorManager floorManager;
    PlayerManager player;
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


    void Start() {
        if (FloorManager.instance) floorManager = FloorManager.instance;
        if (PlayerManager.instance) player = PlayerManager.instance;
    }

// loop through grid x,y, generate sqr grid elements, update them and add to list
    public IEnumerator GenerateGrid(int i) {
        List<Vector2> bloodTiles = new List<Vector2>();
        List<Vector2> bileTiles = new List<Vector2>();
        foreach (FloorDefinition.Spawn spawn in lvlDef.initSpawns) {
            if (spawn.asset.ge is GridSquare s) {
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
        
        gridCursor.transform.localScale = Vector3.one * FloorManager.sqrSize;
        gridCursor.transform.SetAsLastSibling();
        selectedCursor.transform.localScale = Vector3.one * FloorManager.sqrSize;
        selectedCursor.transform.SetAsLastSibling();
        index = i;

        yield return StartCoroutine(SpawnLevelDefinition());
       
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

    IEnumerator SpawnLevelDefinition() {
        enemy = Instantiate(enemyPrefab, this.transform).GetComponent<EnemyManager>(); 
        enemy.transform.SetSiblingIndex(2);
        yield return StartCoroutine(enemy.Initialize());
        foreach (FloorDefinition.Spawn spawn in lvlDef.initSpawns) {
            if (spawn.asset.ge is Unit u) 
            {
                if (u is EnemyUnit e) {
                    enemy.SpawnUnit(spawn.coord, e);
                }
            } else if (spawn.asset.ge is not GridSquare) {
                GridElement ge = Instantiate(spawn.asset.prefab, this.transform).GetComponent<GridElement>();
                ge.transform.parent = neutralGEContainer.transform;

                ge.StoreInGrid(this);
                ge.UpdateElement(spawn.coord);
            }
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

    public void UpdateTargetCursor(bool state, Vector2 coord) {
        gridCursor.SetActive(state);
        gridCursor.transform.position = PosFromCoord(coord);
        gridCursor.GetComponentInChildren<SpriteRenderer>().sortingOrder = SortOrderFromCoord(coord);
    }

    public void UpdateSelectedCursor(bool state, Vector2 coord) {
        selectedCursor.SetActive(state);
        selectedCursor.transform.position = PosFromCoord(coord);
        selectedCursor.GetComponentInChildren<SpriteRenderer>().sortingOrder = SortOrderFromCoord(coord);
    }

// Toggle GridSquare highlights, apply color by index
    public void DisplayValidCoords(List<Vector2> coords, int? index = null, bool stack = false) {
        if (!stack)
            DisableGridHighlight();

        Color c = floorManager.moveColor;
        if (index is int i) {
            switch (i) {
             case 0:
                c = floorManager.moveColor;
             break;
             case 1:
                c = floorManager.attackColor;
             break;
             case 2:
                c = floorManager.hammerColor;
            break;
            case 3:
                c = floorManager.playerColor;
            break;
            }
        }

        if (coords != null) {
            foreach (Vector2 coord in coords) {
                if (sqrs.Find(sqr => sqr.coord == coord))
                    sqrs.Find(sqr => sqr.coord == coord).ToggleValidCoord(true, c);
            }
        }
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
            transform.position.x - (FloorManager.sqrSize * FloorManager.gridSize * ORTHO_OFFSET.x/1.25f) + (coord.x * FloorManager.sqrSize * ORTHO_OFFSET.x/2.25f) + (ORTHO_OFFSET.x * FloorManager.sqrSize * coord.y) + (FloorManager.sqrSize * ORTHO_OFFSET.x), 
            transform.position.y + (FloorManager.sqrSize * 1.75f) + (coord.y * FloorManager.sqrSize * ORTHO_OFFSET.y/1.1f) - (ORTHO_OFFSET.y*2.25f * FloorManager.sqrSize * coord.x),             
            0);
    }

    public int SortOrderFromCoord(Vector2 coord) {
        return 8 + (int)coord.x - (int)coord.y;
    }
}