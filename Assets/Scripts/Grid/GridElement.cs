using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Universal data class derrived by any instance that occupies grid space

//[RequireComponent(typeof(PolygonCollider2D))]
public class GridElement : MonoBehaviour{

    public Grid grid;

    [Header("Grid Element")]
    public Vector2 coord;
    public bool selectable, targeted;
    public PolygonCollider2D hitbox;
    public ElementCanvas elementCanvas;

    [Header("UI/UX")]
    public List<SpriteRenderer> gfx;
    new public string name;


    public delegate void OnElementUpdate(GridElement ge);
    public event OnElementUpdate ElementDestroyed;

    public int hpMax, hpCurrent, defense;
    public int energyCurrent, energyMax;
    
    [Header("Audio")]
    public AudioAtlas.Sound destroyed;

// Initialize references, scale to grid, subscribe onDeath event
    protected virtual void Start() 
    {
        hitbox = GetComponent<PolygonCollider2D>();
        hitbox.enabled = false;

        hpCurrent = hpMax;
        energyCurrent = energyMax;
        elementCanvas = GetComponentInChildren<ElementCanvas>();
        if (elementCanvas) elementCanvas.Initialize(this);
    }

    public virtual void StoreInGrid(Grid owner) {
        grid = owner;
        grid.gridElements.Add(this);
        ElementDestroyed += grid.RemoveElement;
        transform.localScale = Vector3.one * FloorManager.sqrSize;
    }

    protected virtual IEnumerator SpawnElement() {
       float timer = 0;

       while (timer < Grid.spawnDur) {
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * FloorManager.sqrSize, timer/Grid.spawnDur);

        yield return null;
        timer += Time.deltaTime;
       }

       yield return null;
    }

// Update grid position and coordinate
    public virtual void UpdateElement(Vector2 c) 
    {
        transform.position = grid.PosFromCoord(c);
        UpdateSortOrder(c);
        coord=c;
    }  

    public virtual void UpdateSortOrder(Vector2 c) {
        foreach (SpriteRenderer sr in gfx)
            sr.sortingOrder = grid.SortOrderFromCoord(c);
    }

    public virtual void EnableSelection(bool state) {}

// Apply shield to this element
    public virtual IEnumerator Defend(int value) 
    {
        yield return null;
        defense += value;
        elementCanvas.UpdateStatsDisplay();
    }
    
    public virtual IEnumerator TakeDamage(int dmg, GridElement source = null) 
    {

        hpCurrent -= dmg;
        
        if (elementCanvas) {
            elementCanvas.UpdateStatsDisplay();
            StartCoroutine(elementCanvas.DisplayDamageNumber(dmg));
        }
        yield return new WaitForSecondsRealtime(.25f);
        TargetElement(false);
        if (hpCurrent <= 0) {
            StartCoroutine(DestroyElement());
            yield break;
        }
    }

    public virtual IEnumerator DestroyElement() 
    {
        ElementDestroyed?.Invoke(this);
        AudioManager.PlaySound(destroyed, transform.position);
        yield return new WaitForSecondsRealtime(.25f);
        DestroyImmediate(this.gameObject);
    }

    public virtual void TargetElement(bool state) 
    {
        targeted = state;
        if (elementCanvas) {
            elementCanvas.ToggleStatsDisplay(state);
        }
    }

    public virtual IEnumerator CollideFromBelow(GridElement above) {
        
        yield return StartCoroutine(TakeDamage(hpCurrent));
    }
    
}

