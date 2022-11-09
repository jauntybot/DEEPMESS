using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour {
    
   

    [SerializeField] GameObject sqrPrefab;

    [SerializeField] int _gridSize;
    public static int gridSize;
    [SerializeField] float _sqrSize;
    public static float sqrSize;
    public List<GridSquare> sqrs = new List<GridSquare>();

 #region Awake and Singleton
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

    public void GenerateGrid() {
        for (int x = 0; x < gridSize; x++) {
            for (int y =0; y < gridSize; y++) {
                bool _white=false;
                if (x%2==0) { if (y%2==0) _white=true; } 
                else { if (y%2!=0) _white=true; }

                Vector3 pos = PosFromCoord(new Vector2(x,y));
                GridSquare sqr = Instantiate(sqrPrefab, pos, Quaternion.identity, this.transform).GetComponent<GridSquare>();
                sqr.Initialize(sqr.gameObject, pos, new Vector2(x,y), _white);

                sqrs.Add(sqr);
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
