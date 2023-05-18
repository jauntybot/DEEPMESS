using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Universal data class derrived by any instance that occupies grid space

//[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(AudioSource))]
public class GridElement : MonoBehaviour{

    public Grid grid;

    [Header("Grid Element")]
    public Vector2 coord;
    public bool selectable, targeted;
    public PolygonCollider2D hitbox;
    public ElementCanvas elementCanvas;
    public enum DamageType { Unspecified, Melee, Gravity, Bile };
    Material originalMaterial;
    bool takingDmg;

    [Header("UI/UX")]
    public List<SpriteRenderer> gfx;
    new public string name;


    public delegate void OnElementUpdate(GridElement ge);
    public virtual event OnElementUpdate ElementUpdated;
    public virtual event OnElementUpdate ElementDestroyed;

    public int hpMax, hpCurrent, defense;
    public bool shell;
    [SerializeField] GameObject shellGFX;
    public int energyCurrent, energyMax;
    
    [Header("Audio")]
    [HideInInspector] public AudioSource audioSource;
    public SFX selectedSFX;
    public SFX dmgdSFX, healedSFX, destroyedSFX;

// Initialize references, scale to grid, subscribe onDeath event
    protected virtual void Start() 
    {
        audioSource = GetComponent<AudioSource>();
        hitbox = GetComponent<PolygonCollider2D>();
        hitbox.enabled = false;

        hpCurrent = hpMax;
        energyCurrent = energyMax;
        elementCanvas = GetComponentInChildren<ElementCanvas>();
        if (elementCanvas) elementCanvas.Initialize(this);
    }

    public virtual void StoreInGrid(Grid owner) {
        grid = owner;
        if (!grid.gridElements.Contains(this)) {
            grid.gridElements.Add(this);
            ElementDestroyed += grid.RemoveElement;
            transform.localScale = Vector3.one * FloorManager.sqrSize;
            UpdateElement(coord);
        }
    }

// Update grid position and coordinate
    public virtual void UpdateElement(Vector2 c) 
    {
        ElementUpdated?.Invoke(this);
        transform.position = grid.PosFromCoord(c);
        UpdateSortOrder(c);
        coord=c;
        foreach (GridElement ge in grid.CoordContents(c)) {
            if (ge != this)
                ge.OnSharedSpace(this);
        }
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

  
    public virtual IEnumerator TakeDamage(int dmg, DamageType dmgType = DamageType.Unspecified, GridElement source = null) 
    {
        takingDmg = true;
        if (!shell || Mathf.Sign(dmg) == -1) {
            if (Mathf.Sign(dmg) == 1) 
                PlaySound(dmgdSFX);
             else 
                PlaySound(healedSFX);
            
            if (elementCanvas) {
                yield return StartCoroutine(elementCanvas.DisplayDamageNumber(dmg));
            }

            hpCurrent -= dmg;
            hpCurrent = hpCurrent < 0 ? 0 : hpCurrent;
            hpCurrent = hpCurrent > hpMax ? hpMax : hpCurrent;


        } else {
            RemoveShell();
        }
        if (hpCurrent <= 0) {
            StartCoroutine(DestroyElement(dmgType));
        }
        yield return new WaitForSecondsRealtime(.4f);
        TargetElement(false);
    }

    public virtual IEnumerator DestroyElement(DamageType dmgType = DamageType.Unspecified) 
    {
        ElementDestroyed?.Invoke(this);
               
        PlaySound(destroyedSFX);
        
        yield return new WaitForSecondsRealtime(.5f);
        foreach (SpriteRenderer sr in gfx) 
            sr.enabled = false;
        if (elementCanvas)
            elementCanvas.ToggleStatsDisplay(false);
        if (this.gameObject != null)
            Destroy(this.gameObject);
        
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
        yield return StartCoroutine(DestroyElement(DamageType.Gravity));
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

    public virtual void PlaySound(SFX sfx = null) {
        if (sfx)
            audioSource.PlayOneShot(sfx.Get());
    }
    
}

