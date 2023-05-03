using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GridSpacing : MonoBehaviour
{
    [SerializeField] bool generate;
    [SerializeField] Transform gridContainer;
    [SerializeField] GameObject sqrPrefab, wallPrefab;
    
    [SerializeField] public Vector2 ORTHO_OFFSET = new Vector2(1.15f, 0.35f);

    [SerializeField] int gridSize;
    [SerializeField] float sqrSize;
    [SerializeField] float belowFlooroffset;

    [SerializeField] float orthoXFactor1 = 1.25f, orthoXFactor2 = 2.25f, orthoYFactor1 = 2.25f, orthoYFactor2 = 1.65f, ySqrScale = 1.75f;

    void Update()
    {
        if (generate) {
            for (int i = gridContainer.childCount - 1; i >= 0; i--) {
                DestroyImmediate(gridContainer.GetChild(i).gameObject);
            }
            
            for (int y = 0; y < gridSize; y++) {
                for (int x = 0; x < gridSize; x++) {
                    GridSquare sqr = Instantiate(sqrPrefab, this.transform).GetComponent<GridSquare>();
                    sqr.transform.position = PosFromCoord(new Vector2(x,y));
                    sqr.transform.localScale = Vector3.one * sqrSize;
                    
                    sqr.gfx[0].sortingOrder = SortOrderFromCoord(new Vector2(x,y));
                    if (x >= gridSize/2 - 4 && x <= gridSize/2 + 3 && y >= gridSize/2 - 4 && y <= gridSize/2 + 3) {

                    } else {
                        sqr.transform.position -= new Vector3(0, belowFlooroffset, 0);
                        sqr.gfx[0].color = new Color(.25f, .25f, .25f, 1);
                        if (x <= gridSize/2 - 4 || y >= gridSize/2+3)
                            sqr.gfx[0].sortingLayerName = "BG";
                        
                        if (Random.Range(0,100) >= 50) {
                            GridElement wall = Instantiate(wallPrefab, this.transform).GetComponent<GridElement>();
                            wall.transform.position = PosFromCoord(new Vector2(x,y)) - new Vector3(0, belowFlooroffset, 0);
                            wall.transform.localScale = Vector3.one * sqrSize;

                            wall.gfx[0].sortingOrder = SortOrderFromCoord(new Vector2(x,y));
                            if (x <= gridSize/2 - 4 || y >= gridSize/2+3)
                                wall.gfx[0].sortingLayerName = "BG";
                            wall.gfx[0].color = new Color(.25f, .25f, .25f, 1);
                        }
                    }
                }
            }
            generate = false;
        }
    }


    public Vector3 PosFromCoord(Vector2 coord) {
        return new Vector3(
// offset from scene origin + coord to pos conversion + ortho offset + center measure
            transform.position.x - (sqrSize * gridSize * ORTHO_OFFSET.x/orthoXFactor1) + (coord.x * sqrSize * ORTHO_OFFSET.x/orthoXFactor2) + (ORTHO_OFFSET.x * sqrSize * coord.y) + (sqrSize * ORTHO_OFFSET.x), 
            transform.position.y + (sqrSize * ySqrScale) + (coord.y * sqrSize * ORTHO_OFFSET.y/orthoYFactor2) - (ORTHO_OFFSET.y*orthoYFactor1 * sqrSize * coord.x),             
            0);
    }

    public int SortOrderFromCoord(Vector2 coord) {
        return 8 + (int)coord.x - (int)coord.y;
    }
}
