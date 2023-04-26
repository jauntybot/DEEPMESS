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

    Material originalMaterial;

    [Header("UI/UX")]
    public List<SpriteRenderer> gfx;
    new public string name;


    public delegate void OnElementUpdate(GridElement ge);
    public virtual event OnElementUpdate ElementDestroyed;

    public int hpMax, hpCurrent, defense;
    public bool shell;
    [SerializeField] GameObject shellGFX;
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

// Update grid position and coordinate
    public virtual void UpdateElement(Vector2 c) 
    {
        transform.position = grid.PosFromCoord(c);
        UpdateSortOrder(c);
        coord=c;
    }  

    public virtual void UpdateSortOrder(Vector2 c) {
        int sort = grid.SortOrderFromCoord(c);
        foreach (SpriteRenderer sr in gfx)
            sr.sortingOrder = sort;
        if (shellGFX) {
            if (shellGFX.GetComponent<LineRenderer>()) shellGFX.GetComponent<LineRenderer>().sortingOrder = sort;
            else if (shellGFX.GetComponent<SpriteRenderer>()) shellGFX.GetComponent<SpriteRenderer>().sortingOrder = sort;
        }
    }

    public virtual void EnableSelection(bool state) {
        selectable = state;
        hitbox.enabled = state;
    }

  
    public virtual IEnumerator TakeDamage(int dmg, GridElement source = null) 
    {
        if (!shell) {
            hpCurrent -= dmg;
            
            if (elementCanvas) {
                yield return StartCoroutine(elementCanvas.DisplayDamageNumber(dmg));
                elementCanvas.UpdateStatsDisplay();
            }
        } else {
            RemoveShell();
        }
        yield return new WaitForSecondsRealtime(.4f);
        TargetElement(false);
        if (hpCurrent <= 0) {
            StartCoroutine(DestroyElement());
        }
    }

    public virtual IEnumerator DestroyElement() 
    {
        ElementDestroyed?.Invoke(this);
        AudioManager.PlaySound(destroyed, transform.position);
        yield return new WaitForSecondsRealtime(.25f);
        if (this.gameObject != null)
            Destroy(this.gameObject);
        StopAllCoroutines();
    }

    public virtual void TargetElement(bool state) 
    {
        targeted = state;
        if (elementCanvas) {
            elementCanvas.ToggleStatsDisplay(state);
        }
    }

    public virtual IEnumerator CollideFromBelow(GridElement above) {
        RemoveShell();
        yield return StartCoroutine(TakeDamage(hpCurrent));
    }

    public virtual void OnSharedSpace(GridElement sharedWith) {
        
    }

    public virtual void ApplyShell() {
        if (!shell) {
            shellGFX.SetActive(true);
            shell = true;
        }
    }

    public virtual void RemoveShell() {
        if (shell) {
            shellGFX.SetActive(false);
            shell = false;
        }
    }
    
}

