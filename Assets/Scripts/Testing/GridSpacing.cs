using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GridSpacing : MonoBehaviour
{
    [SerializeField] bool generate;
    [SerializeField] Vector2 calcIn;
    [SerializeField] bool calc;
    [SerializeField] Transform gridContainer;
    [SerializeField] GameObject sqrPrefab, wallPrefab;
    
    [SerializeField] public Vector2 ORTHO_OFFSET = new(1.12f, 0.35f);

    [SerializeField] Vector2 gridSize;
    [SerializeField] float sqrSize;
    [SerializeField] float belowFlooroffset;

    [SerializeField] float orthoXFactor1 = 1.355f, orthoXFactor2 = 2.5f, orthoYFactor1 = 2.1f, orthoYFactor2 = 1.1f, ySqrScale = 1.75f;


    void Update()
    {
        if (generate) {
            for (int i = gridContainer.childCount - 1; i >= 0; i--) {
                DestroyImmediate(gridContainer.GetChild(i).gameObject);
            }
            
            for (int y = 0; y < gridSize.y; y++) {
                for (int x = 0; x < gridSize.x; x++) {
                    Tile sqr = Instantiate(sqrPrefab, this.transform).GetComponent<Tile>();
                    sqr.transform.position = PosFromCoord(new Vector2(x,y));
                    sqr.transform.localScale = Vector3.one * sqrSize;
                    
                    sqr.gfx[0].sortingOrder = SortOrderFromCoord(new Vector2(x,y));
                    if (sqr.GetComponentInChildren<LineRenderer>()) {
                        LineRenderer lr = sqr.GetComponentInChildren<LineRenderer>();
                        lr.sortingOrder = SortOrderFromCoord(new Vector2(x,y));
                        
                    }
                    if (x >= gridSize.x/2 - 4 && x <= gridSize.x/2 + 3 && y >= gridSize.y/2 - 4 && y <= gridSize.y/2 + 3) {

                    } else {
                        sqr.transform.position -= new Vector3(0, belowFlooroffset, 0);
                        sqr.gfx[0].color = new Color(.25f, .25f, .25f, 1);
                        if (x < gridSize.x/2 - 4 || y > gridSize.y/2+3)
                            sqr.gfx[0].sortingLayerName = "BG";
                        
                        if (Random.Range(0,100) >= 50) {
                            GridElement wall = Instantiate(wallPrefab, this.transform).GetComponent<GridElement>();
                            wall.transform.position = PosFromCoord(new Vector2(x,y)) - new Vector3(0, belowFlooroffset, 0);
                            wall.transform.localScale = Vector3.one * sqrSize;

                            wall.gfx[0].sortingOrder = SortOrderFromCoord(new Vector2(x,y));
                            if (x < gridSize.x/2 - 4 || y > gridSize.y/2+3)
                                wall.gfx[0].sortingLayerName = "BG";
                            wall.gfx[0].color = new Color(.25f, .25f, .25f, 1);
                        }
                    }
                }
            }
            generate = false;
        }
        if (calc) {
            Vector3 pos = PosFromCoord(calcIn);
            Vector3 offset = new(pos.x + (sqrSize * gridSize.x * ORTHO_OFFSET.x/orthoXFactor1), pos.y - (sqrSize * gridSize.y * ORTHO_OFFSET.y/ySqrScale));
            Debug.Log(calcIn + ", " +  offset);
            calc = false;
        }
    }


    public Vector3 PosFromCoord(Vector2 coord) {
        return new Vector3(
// offset from scene origin + coord to pos conversion + ortho offset + center measure
            transform.position.x - (sqrSize * gridSize.x * ORTHO_OFFSET.x/orthoXFactor1) + (coord.x * sqrSize * ORTHO_OFFSET.x/orthoXFactor2) + (ORTHO_OFFSET.x * sqrSize * coord.y) + (sqrSize * ORTHO_OFFSET.x), 
            transform.position.y + (sqrSize * gridSize.y * ORTHO_OFFSET.y/ySqrScale) + (coord.y * sqrSize * ORTHO_OFFSET.y/orthoYFactor2) - (ORTHO_OFFSET.y*orthoYFactor1 * sqrSize * coord.x),             
            0);
    }

    public int SortOrderFromCoord(Vector2 coord) {
        return 8 + (int)coord.x - (int)coord.y;
    }
}
