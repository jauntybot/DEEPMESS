using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Grid : MonoBehaviour {
    
   
//[SerializedFields] will be able to be refactored into level design classes
//static fields stay with the grid script indefinetely
    [SerializeField] GameObject sqrPrefab;
    [SerializeField] int _gridSize;
    public static int gridSize;
    [SerializeField] float _sqrSize;
    public static float sqrSize;
    public List<GridSquare> sqrs = new List<GridSquare>();
    public List<GridElement> gridElements = new List<GridElement>();

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

//loop through grid x,y, generate sqr grid elements, update them and add to list
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
    }

    public void DisplayValidMoves(List<Vector2> coords) {
        foreach(GridSquare sqr in sqrs)
            sqr.ToggleValidMove(false);
        if (coords != null) {
            foreach (Vector2 coord in coords) {
                if (sqrs.Find(sqr => sqr.coord == coord))
                    sqrs.Find(sqr => sqr.coord == coord).ToggleValidMove(true);
            }
        }
    }

    public static Vector2 CoordFromPos(Vector3 pos) {
        return new Vector2((pos.x - sqrSize/2 + sqrSize*gridSize/2)/sqrSize, (pos.y - sqrSize/2 + sqrSize*gridSize/2)/sqrSize);
    }
     public static Vector3 PosFromCoord(Vector2 coord) {
        return new Vector3(coord.x*sqrSize + sqrSize/2 - sqrSize*gridSize/2, coord.y*sqrSize + sqrSize/2 - sqrSize*gridSize/2, 0);
    }
}