using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.U2D;

// Script that generates and maintains the grid
// Stores all Grid Elements that are created

[RequireComponent(typeof(AudioSource))]
public class Grid : MonoBehaviour {
    
    FloorManager floorManager;
    [Header("References")]
    [SerializeField] PlayerManager player;
    public UnitManager enemy;
    public GameObject gridContainer, neutralGEContainer;
    [SerializeField] GameObject chessNotation;
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] GameObject tilePrefab, selectedCursor;
    public bool overrideHighlight;
    

    [Header("Grid Data")]
    public int index = 0;
    public FloorDefinition lvlDef;
    [HideInInspector] public List<Vector2> slagSpawns, nailSpawns;
    private bool notation = false;
    [SerializeField] public Vector2 ORTHO_OFFSET = new(1.15f, 0.35f);
    [SerializeField] float shockwaveDur = .25f, collapseDur = 0.25f;
    [SerializeField] AnimationCurve shockwaveCurve;

    public List<Tile> tiles = new();
    public List<GridElement> gridElements = new();

    [SerializeField] SFX collapseSFX;
    AudioSource audioSource;


    void Awake() {
        if (FloorManager.instance) floorManager = FloorManager.instance;
        if (PlayerManager.instance) player = PlayerManager.instance;
        audioSource = GetComponent<AudioSource>();
    }

// loop through grid x,y, generate Tile grid elements, update them and add to list
    public void GenerateGrid(int i) {
        List<Vector2> altTiles = new();

        foreach (FloorDefinition.Spawn spawn in lvlDef.initSpawns) {
            if (spawn.asset.prefab.GetComponent<GridElement>() is Tile) {
                altTiles.Add(spawn.coord);
            }
        }
        for (int y = 0; y < FloorManager.gridSize; y++) {
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

        SpawnLevelDefinition();
       

        LockGrid(true);
    }

// Function that instantiates and houses FloorAssets that are defined by the currently loaded Floor Definition
    void SpawnLevelDefinition() {
// Create new EnemyManager if this floor is not overriden by Tutorial
        enemy = Instantiate(enemyPrefab, transform).GetComponent<EnemyManager>(); 
        enemy.transform.SetSiblingIndex(2);
        enemy.StartCoroutine(enemy.Initialize(this));
        
        slagSpawns = new(); nailSpawns = new();
        foreach (FloorDefinition.Spawn spawn in lvlDef.initSpawns) {
            GridElement ge = spawn.asset.prefab.GetComponent<GridElement>();
// Spawn a Unit
            if (ge is Unit u) {
                if (u is EnemyUnit e && e is not BossUnit) {
                    enemy.SpawnUnit(spawn.asset.prefab.GetComponent<Unit>(), spawn.coord);
                } else if (u is PlayerUnit) {
                    slagSpawns.Add(spawn.coord);
                }
                else if (u is Nail) {
                    nailSpawns.Add(spawn.coord);
                }
// Spawn a Neutral Grid Element
            } else if (ge is not Tile) {
                GridElement neutralGE = Instantiate(spawn.asset.prefab, this.transform).GetComponent<GridElement>();
                neutralGE.transform.parent = neutralGEContainer.transform;

                neutralGE.StoreInGrid(this);
                neutralGE.UpdateElement(spawn.coord);
            }
        }
    }

    public IEnumerator ShockwaveCollapse(Vector2 origin) {
        StartCoroutine(CollapseTile(origin));
        
        CameraController.instance.StartCoroutine(CameraController.instance.ScreenShake(0.75f, 0.75f));
        
        int range;
        if (origin.x >= 4) {
            if (origin.y >= 4) {
                range = (int)origin.x + (int)origin.y;
            } else {
                range = (int)origin.x + 7 - (int)origin.y;
            }
        } else {
            if (origin.y >= 4) {
                range = 7 - (int)origin.x + (int)origin.y;
            } else {
                range = 7 - (int)origin.x + 7 - (int)origin.y;
            }
        }

        float timer = 0;
        List<Coroutine> collapseCo = new();
// Box adjacency
        // for (int r = 1; r <= 9; r++) {
        //     List<Vector2> activeCoords = new();
        //     for (int i = r; i >= 0; i--) {
        //         activeCoords.Add(new Vector2(origin.x + r - i, origin.y + r));
        //         activeCoords.Add(new Vector2(origin.x + r - i, origin.y - r));
        //         activeCoords.Add(new Vector2(origin.x - r + i, origin.y + r));
        //         activeCoords.Add(new Vector2(origin.x - r + i, origin.y - r));
        //         activeCoords.Add(new Vector2(origin.x + r, origin.y + r - i - 1));
        //         activeCoords.Add(new Vector2(origin.x + r, origin.y - r + i + 1));
        //         activeCoords.Add(new Vector2(origin.x - r, origin.y + r - i - 1));
        //         activeCoords.Add(new Vector2(origin.x - r, origin.y - r + i + 1));

        //     }
// Diamond adjacency
        PlaySound(collapseSFX);
        for (int r = 1; r <= range; r++) {
            List<Vector2> activeCoords = new();
            for (int i = r; i >= 0; i--) {
                activeCoords.Add(new Vector2(origin.x + r - i, origin.y + i)); 
                activeCoords.Add(new Vector2(origin.x - i, origin.y + r - i)); 
                activeCoords.Add(new Vector2(origin.x - r + i, origin.y - i));
                activeCoords.Add(new Vector2(origin.x + i, origin.y - r + i));
            }
            activeCoords = EquipmentAdjacency.RemoveOffGridCoords(activeCoords);
            if (activeCoords.Count == 0) break;

            float dur = shockwaveDur*((float)(range - (r-1))/(float)range);              
            collapseCo.Add(StartCoroutine(CollapseTiles(activeCoords, dur)));

            timer = 0;
            while (timer < dur) {
                yield return null;
                timer += Time.deltaTime;
            }
        }    
        for (int i = collapseCo.Count - 1; i >= 0; i--) {
            if (collapseCo[i] != null) 
                yield return collapseCo[i];
            else
                collapseCo.RemoveAt(i);
        }
    }

    IEnumerator CollapseScale() {
        float timer = 0;
        while (timer <= shockwaveDur*8f) {
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.75f, shockwaveCurve.Evaluate(timer/(shockwaveDur/8)));
            
            timer += Time.deltaTime;
            yield return null;
        }

    }

// Shuffle list, offset collapse delay
    public IEnumerator CollapseTiles(List<Vector2> activeCoords, float window) {
        Vector2[] array = activeCoords.ToArray();
        ShuffleBag<Vector2> shuffle = new(array);

        List<Coroutine> collapseCo = new();
        int count = shuffle.Count;
        for (int i = count - 1; i >= 0; i--) {
            collapseCo.Add(StartCoroutine(CollapseTile(shuffle.Next())));
            float timer = 0;
            while (timer < window/count) {
                yield return null;
                timer += Time.deltaTime;
            }
        }
        for (int x = collapseCo.Count - 1; x >= 0; x--) {
            if (collapseCo[x] != null) 
                yield return collapseCo[x];
            else
                collapseCo.RemoveAt(x);
        }
    }

    public IEnumerator CollapseTile(Vector2 coord) {
        Dictionary<GridElement, Vector2> affected = new();
        Tile tile = tiles.Find(t => t.coord == coord);
        affected.Add(tile, tile.transform.position);
        if (tile.anim) tile.anim.SetTrigger("Collapse");
        foreach (GridElement ge in CoordContents(coord)) {
            affected.Add(ge, ge.transform.position);
            //if (ge is Wall w) w.StartCoroutine(w.DestroyElement());
        }

        float timer = 0;
        while (timer <= collapseDur/2) {
            foreach (KeyValuePair<GridElement, Vector2> entry in affected) {
                if (entry.Key is Unit u) {
                    u.transform.position = new Vector3(entry.Value.x, Mathf.Clamp(entry.Value.y + Mathf.Sin(shockwaveCurve.Evaluate(timer/(collapseDur/2))*3.14f), entry.Value.y, entry.Value.y + 0.5f));
                    //u.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = 1 - shockwaveCurve.Evaluate(timer/(collapseDur/2));
                }
                else {
                    entry.Key.transform.position = new Vector3(entry.Value.x, Mathf.Clamp(entry.Value.y + Mathf.Sin(shockwaveCurve.Evaluate(timer/(collapseDur/2))*3.14f), entry.Value.y, entry.Value.y + 0.5f));
                }
            }
            yield return null;
            timer += Time.deltaTime;
        }
        foreach (KeyValuePair<GridElement, Vector2> snap in affected) 
            snap.Key.transform.position = snap.Value;

        timer = 0;
        while (timer <= collapseDur/4) {
            yield return null;
            timer += Time.deltaTime;
        }

        timer = 0;
        while (timer <= collapseDur) {
            foreach (KeyValuePair<GridElement, Vector2> entry in affected) {
                if (timer >= collapseDur*0.875f && entry.Key is not Unit) {
                    entry.Key.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.75f, Mathf.Clamp(4 - shockwaveCurve.Evaluate((timer - collapseDur*0.875f)/(collapseDur*0.875f)) * 4, 0, 1));
                }
                entry.Key.transform.position = new Vector3(entry.Value.x, entry.Value.y - shockwaveCurve.Evaluate(timer/collapseDur)*3);
                entry.Key.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = Mathf.Clamp(1 - shockwaveCurve.Evaluate((timer - collapseDur/2)/(collapseDur/2)), 0, 1);
            }
            yield return null;
            timer += Time.deltaTime;
        }
        foreach (KeyValuePair<GridElement, Vector2> entry in affected)
            entry.Key.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = 0;

    }

    public void ToggleChessNotation(bool state) {
        chessNotation.SetActive(state);
    }

    public void AddElement(GridElement ge) {
        gridElements.Add(ge);
    }

    public void RemoveElement(GridElement ge) {
        gridElements.Remove(ge);
    }

    public void UpdateSelectedCursor(bool state, Vector2 coord) {
        selectedCursor.SetActive(state);
        selectedCursor.transform.position = PosFromCoord(coord);
        selectedCursor.GetComponentInChildren<LineRenderer>().sortingOrder = SortOrderFromCoord(coord);
        if (overrideHighlight)
            selectedCursor.SetActive(false);
    }

// Toggle Tile highlights, apply color by index
    public void DisplayValidCoords(List<Vector2> coords, int? index = null, bool stack = false, bool fill = true) {
        if (!stack)
            DisableGridHighlight();

        Color c = floorManager.playerColor;
        if (index is int i)
            c = floorManager.GetFloorColor(i);

        if (coords != null) {
            foreach (Vector2 coord in coords) {
                if (tiles.Find(tile => tile.coord == coord))
                    tiles.Find(tile => tile.coord == coord).ToggleValidCoord(true, c, fill);
            }
        }

        if (overrideHighlight) DisableGridHighlight();
    }

// Disable Tile highlights
    public void DisableGridHighlight() {
        foreach(Tile tile in tiles)
            tile.ToggleValidCoord(false);
    }

    public void LockGrid(bool state) {
        foreach (Tile tile in tiles)
            tile.ToggleHitBox(!state);
    }

    public List<GridElement> CoordContents(Vector2 coord) {
        return gridElements.FindAll(ge => ge.coord == coord && ge is not GodParticleGE);
    }

     public Vector3 PosFromCoord(Vector2 coord) {
        return new Vector3(
// offset from scene origin + coord to pos conversion + ortho offset + center measure
            transform.position.x - (FloorManager.sqrSize * FloorManager.gridSize * ORTHO_OFFSET.x/1.355f) + (coord.x * FloorManager.sqrSize * ORTHO_OFFSET.x/2.5f) + (ORTHO_OFFSET.x * FloorManager.sqrSize * coord.y) + (FloorManager.sqrSize * ORTHO_OFFSET.x), 
            transform.position.y + (FloorManager.sqrSize * FloorManager.gridSize * ORTHO_OFFSET.y/1.75f) + (coord.y * FloorManager.sqrSize * ORTHO_OFFSET.y/1.1f) - (ORTHO_OFFSET.y*2.1f * FloorManager.sqrSize * coord.x),             
            0);
    }

    public int SortOrderFromCoord(Vector2 coord, int coordSort = 0) {
        return (8 + (int)coord.x - (int)coord.y) * 10 + coordSort;
    }

    public virtual void PlaySound(SFX sfx = null) {
        if (sfx) {
            if (sfx.outputMixerGroup) 
                audioSource.outputAudioMixerGroup = sfx.outputMixerGroup;   

            audioSource.PlayOneShot(sfx.Get());
        }
    }
}