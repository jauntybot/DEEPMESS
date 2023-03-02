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
    public UnitManager enemy;
    [SerializeField] GameObject enemyPrefab;

    static Vector2 ORTHO_OFFSET = new Vector2(0.75f, 0.5f);
    [SerializeField] GameObject sqrPrefab, gridCursor;
    [SerializeField] static float fadeInDur = 0.25f;
    public LevelDefinition lvlDef;
    [SerializeField] Color offWhite;

    public List<GridSquare> sqrs = new List<GridSquare>();
    public List<GridElement> gridElements = new List<GridElement>();


    void Start() {
        if (FloorManager.instance) floorManager = FloorManager.instance;
        if (PlayerManager.instance) player = PlayerManager.instance;
    }

// loop through grid x,y, generate sqr grid elements, update them and add to list
    public IEnumerator GenerateGrid(int i) {
        for (int y = 0; y < FloorManager.gridSize; y++) {
            for (int x = 0; x < FloorManager.gridSize; x++) {
//store bool for white sqrs
                GridSquare sqr = Instantiate(sqrPrefab, this.transform).GetComponent<GridSquare>();
                sqr.white=false;
                if (x%2==0) { if (y%2==0) sqr.white=true; } 
                else { if (y%2!=0) sqr.white=true; }
                if (!sqr.white) {
                    foreach (SpriteRenderer sr in sqr.gfx)
                        sr.color = offWhite;
                }
                sqr.StoreInGrid(this);
                sqr.UpdateElement(new Vector2(x,y));

                sqrs.Add(sqr);
                sqr.transform.parent = gridContainer.transform;
            }
        }
        gridCursor.transform.localScale = Vector3.one * FloorManager.sqrSize;
        index = i;

        yield return StartCoroutine(SpawnLevelDefinition());
       
        NestedFadeGroup.NestedFadeGroup fade = GetComponent<NestedFadeGroup.NestedFadeGroup>();

        float timer = 0;
        while (timer <= fadeInDur) {
            fade.AlphaSelf = Mathf.Lerp(0, 1, timer/fadeInDur);

            timer += Time.deltaTime;
            yield return null;
        }
        fade.AlphaSelf = 1;
    }

    IEnumerator SpawnLevelDefinition() {
        enemy = Instantiate(enemyPrefab, this.transform).GetComponent<EnemyManager>(); 
        yield return StartCoroutine(enemy.Initialize());
        foreach (Content c in lvlDef.initSpawns) {
            if (c.prefabToSpawn is Unit u) 
            {
                if (u is EnemyUnit e)
                    enemy.SpawnUnit(c.coord, e);
            } else {
                GridElement ge = Instantiate(c.prefabToSpawn.gameObject, this.transform).GetComponent<GridElement>();
                ge.transform.parent = neutralGEContainer.transform;

                ge.StoreInGrid(this);
                ge.UpdateElement(c.coord);
            }
        }
    }

    public void ToggleChessNotation() {
        chessNotation.SetActive(!chessNotation.activeSelf);
    }

    public void AddElement(GridElement ge) {
        gridElements.Add(ge);
    }

    public void RemoveElement(GridElement ge) 
    {
        gridElements.Remove(ge);
    }

    public void DisplayGridCursor(bool state, Vector2 coord) {
        gridCursor.SetActive(state);
        gridCursor.transform.position = PosFromCoord(coord);
        gridCursor.GetComponent<SpriteRenderer>().sortingOrder = SortOrderFromCoord(coord);
    }

// Toggle GridSquare highlights, apply color by index
    public void DisplayValidCoords(List<Vector2> coords, int? index = null) {
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

    public GridElement CoordContents(Vector2 coord) {
        return gridElements.Find(ge => ge.coord == coord);
    }

     public Vector3 PosFromCoord(Vector2 coord) {
        return new Vector3(
// offset from scene origin + coord to pos conversion + ortho offset + center measure
            transform.position.x - (FloorManager.sqrSize * FloorManager.gridSize * ORTHO_OFFSET.x) + (coord.x * FloorManager.sqrSize * ORTHO_OFFSET.x) + (ORTHO_OFFSET.x * FloorManager.sqrSize * coord.y) + (FloorManager.sqrSize * ORTHO_OFFSET.x), 
            transform.position.y + (coord.y * FloorManager.sqrSize * ORTHO_OFFSET.y) - (ORTHO_OFFSET.y * FloorManager.sqrSize * coord.x), 
            0);
    }

    public int SortOrderFromCoord(Vector2 coord) {
        return 8 + (int)coord.x - (int)coord.y;
    }
}