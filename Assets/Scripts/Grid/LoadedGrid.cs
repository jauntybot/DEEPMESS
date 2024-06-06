using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadedGrid : Grid {

    public override void GenerateGrid(int i) {
        List<Vector2> altTiles = new();

        foreach (FloorDefinition.Spawn spawn in lvlDef.initSpawns) {
            if (spawn.asset.prefab.GetComponent<GridElement>() is Tile) {
                altTiles.Add(spawn.coord);
            }
        }
        //bool bloodFallsL = false, bloodFallsR = false;
        for (int y = FloorManager.gridSize - 1; y >= 0; y--) {
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

        LoadSavedFloorState(null);

        LockGrid(true);
    }

    void LoadSavedFloorState(RunData runData) {

        enemy = Instantiate(enemyPrefab, transform).GetComponent<EnemyManager>(); 
        enemy.transform.SetSiblingIndex(2);
        enemy.StartCoroutine(enemy.Initialize(this));

        foreach (KeyValuePair<Vector2, string[]> entry in runData.floorDict) {
            Tile tile = null;
            if (entry.Value[0] == "BONE") tile = Instantiate(tilePrefab, transform).GetComponent<Tile>();
            //else if (entry.Value[0] == "BLOOD") tile = Instantiate()

            // tile.white=false;
            // if (x%2==0) { if (y%2==0) tile.white=true; } 
            // else { if (y%2!=0) tile.white=true; }

            // tile.StoreInGrid(this);
            // tile.UpdateElement(new Vector2(x,y));

            tiles.Add(tile);
            tile.transform.parent = gridContainer.transform;
        }
        

    }

}
