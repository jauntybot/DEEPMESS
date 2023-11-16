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
    [SerializeField] new public string name;
    public Vector2 coord;
    public bool selectable, targeted;
    [HideInInspector] public PolygonCollider2D hitbox;
     public ElementCanvas elementCanvas;
    public enum DamageType { Unspecified, Melee, Gravity, Bile, Slots };

    bool takingDmg;

    [Header("UI/UX")]
    public List<SpriteRenderer> gfx;



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
    protected virtual void Start()  {
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
        }
    }

// Update grid position and coordinate
    public virtual void UpdateElement(Vector2 c) {
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

  
    public virtual IEnumerator TakeDamage(int dmg, DamageType dmgType = DamageType.Unspecified, GridElement source = null) {
        takingDmg = true;
        if (!shell || Mathf.Sign(dmg) == -1) {
            if (Mathf.Sign(dmg) == 1) 
                PlaySound(dmgdSFX);
             else if (dmgType != DamageType.Slots)
                PlaySound(healedSFX);
            
            if (elementCanvas) {
                yield return StartCoroutine(elementCanvas.DisplayDamageNumber(dmg));
            }

            hpCurrent -= dmg;
            
            if (hpCurrent < 0) hpCurrent = 0;
            if (hpCurrent > hpMax) hpCurrent = hpMax;
            

        } else {
            RemoveShell();
        }
        if (hpCurrent <= 0) {
            yield return StartCoroutine(DestroySequence(dmgType));
        }
        //yield return new WaitForSecondsRealtime(.4f);
        TargetElement(false);
    }

    public virtual void DestroyElement(DamageType dmgType = DamageType.Unspecified) {
        StopAllCoroutines();
        StartCoroutine(DestroySequence(dmgType));
    }

    public virtual IEnumerator DestroySequence(DamageType dmgType = DamageType.Unspecified) {
        ElementDestroyed?.Invoke(this);
               
        PlaySound(destroyedSFX);
        
        if (elementCanvas)
            elementCanvas.ToggleStatsDisplay(false);
        yield return new WaitForSecondsRealtime(.5f);
        foreach (SpriteRenderer sr in gfx) 
            sr.enabled = false;
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
        yield return StartCoroutine(DestroySequence(DamageType.Gravity));
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
        if (sfx) {
            if (sfx.outputMixerGroup) 
                audioSource.outputAudioMixerGroup = sfx.outputMixerGroup;   

            audioSource.PlayOneShot(sfx.Get());
        }
    }
    
}

