using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script that generates and maintains the grid
// Stores all Grid Elements that are created

public class Grid : MonoBehaviour {
    
   
//[SerializedFields] will be able to be refactored into level design classes
//static fields stay with the grid script indefinetely

    static Vector2 ORTHO_OFFSET = new Vector2(0.75f, 0.5f);
    [SerializeField] GameObject sqrPrefab, gridCursor;
    

    [SerializeField] int _gridSize;
    public static int gridSize;
    [SerializeField] float _sqrSize;
    public static float sqrSize;
    public List<GridSquare> sqrs = new List<GridSquare>();
    public List<GridElement> gridElements = new List<GridElement>();

    public Color moveColor, attackColor, defendColor;

 #region Singleton (and Awake)
    public static Grid instance;
    private void Awake() {
        if (Grid.instance) {
            Debug.Log("Warning! More than one instance of Grid found!");
            return;
        }
        Grid.instance = this;
        gridSize=_gridSize; sqrSize = _sqrSize;
    }
    #endregion

// loop through grid x,y, generate sqr grid elements, update them and add to list
    public IEnumerator GenerateGrid() {
        for (int y = 0; y < gridSize; y++) {
            for (int x = 0; x < gridSize; x++) {
                yield return new WaitForSecondsRealtime(Util.initD/2);
//store bool for white sqrs
                bool _white=false;
                if (x%2==0) { if (y%2==0) _white=true; } 
                else { if (y%2!=0) _white=true; }

                GridSquare sqr = Instantiate(sqrPrefab, this.transform).GetComponent<GridSquare>();
                sqr.white = _white;
                sqr.UpdateElement(new Vector2(x,y));

                sqrs.Add(sqr);
            }
        }
        gridCursor.transform.localScale = Vector3.one * sqrSize;
    }

    public void RemoveElement(GridElement ge) 
    {
        gridElements.Remove(ge);
    }

// Toggle GridSquare highlights, apply color by index
    public void DisplayValidCoords(List<Vector2> coords, int? index = null) {
        DisableGridHighlight();

        Color c = moveColor;
        if (index is int i) {
            switch (i) {
             case 0:
                c = moveColor;
             break;
             case 1:
                c = attackColor;
             break;
             case 2:
                c = defendColor;
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

     public static Vector3 PosFromCoord(Vector2 coord) {
        return new Vector3(
// offset from scene origing + coord to pos conversion + ortho offset + center measure
            -(sqrSize * gridSize * ORTHO_OFFSET.x) + (coord.x * sqrSize * ORTHO_OFFSET.x) + (ORTHO_OFFSET.x * sqrSize * coord.y) + (sqrSize * ORTHO_OFFSET.x), 
            (coord.y * sqrSize * ORTHO_OFFSET.y) - (ORTHO_OFFSET.y * sqrSize * coord.x), 
            0);
    }
}