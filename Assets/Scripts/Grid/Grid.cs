using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script that generates and maintains the grid
// Stores all Grid Elements that are created

public class Grid : MonoBehaviour {
    
    FloorManager floorManager;
    PlayerManager player;
    public int index;
    [SerializeField] GameObject grid, neutralGridElements;
    public UnitManager enemy;
    [SerializeField] GameObject enemyPrefab;

    static Vector2 ORTHO_OFFSET = new Vector2(0.75f, 0.5f);
    [SerializeField] GameObject sqrPrefab, gridCursor;
    public static float spawnDur = 10;
    public LevelDefinition lvlDef;

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
                sqr.white = true;
                sqr.StoreInGrid(this);
                sqr.UpdateElement(new Vector2(x,y));

                sqrs.Add(sqr);
                sqr.transform.parent = grid.transform;
            }
        }
        gridCursor.transform.localScale = Vector3.one * FloorManager.sqrSize;
        index = i;
        yield return StartCoroutine(SpawnLevelDefinition());
    }

    IEnumerator SpawnLevelDefinition() 
    {
        enemy = Instantiate(enemyPrefab, this.transform).GetComponent<EnemyManager>(); 
        yield return StartCoroutine(enemy.Initialize());
        foreach (Content c in lvlDef.initSpawns) 
        {
            if (c.gridElement is Unit u) 
            {
                if (u is EnemyUnit e)
                    enemy.SpawnUnit(c.coord, e);
            } else {
                GridElement ge = Instantiate(c.gridElement.gameObject, this.transform).GetComponent<GridElement>();
                gridElements.Add(ge);
                ge.transform.parent = neutralGridElements.transform;

                ge.ElementDestroyed += RemoveElement;
                ge.StoreInGrid(this);
                ge.UpdateElement(c.coord);
            }
        }
        
    }

    public IEnumerator DropDrill() {
        yield return null;
        Content drillSpawn = lvlDef.initSpawns.Find(ge => ge.gridElement is Drill);
        float xOffset = PosFromCoord(drillSpawn.coord).x;
        player.drill.transform.position = new Vector3(xOffset, player.drill.transform.position.y, 0);
        yield return StartCoroutine(player.UpdateDrill(drillSpawn.coord));
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
            -(FloorManager.sqrSize * FloorManager.gridSize * ORTHO_OFFSET.x) + (coord.x * FloorManager.sqrSize * ORTHO_OFFSET.x) + (ORTHO_OFFSET.x * FloorManager.sqrSize * coord.y) + (FloorManager.sqrSize * ORTHO_OFFSET.x), 
            (coord.y * FloorManager.sqrSize * ORTHO_OFFSET.y) - (ORTHO_OFFSET.y * FloorManager.sqrSize * coord.x), 
            0);
    }
}