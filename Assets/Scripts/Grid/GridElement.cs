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
    public enum DamageType { Unspecified, Heal, Melee, Gravity, Bile, Slots };
    public int hpMax, hpCurrent;
    public Shield shield;
    public int energyCurrent, energyMax;

    [Header("UI/UX")]
    public List<SpriteRenderer> gfx;

    public delegate void OnElementUpdate(GridElement ge);
    public virtual event OnElementUpdate ElementUpdated;
    public virtual event OnElementUpdate ElementDestroyed;
    public virtual event OnElementUpdate ElementShielded;

    
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
        if (shield) 
            shield.gfx.sortingOrder = sort;
        
    }

    public virtual void EnableSelection(bool state) {
        selectable = state;
        hitbox.enabled = state;
    }

  
    public virtual IEnumerator TakeDamage(int dmg, DamageType dmgType = DamageType.Unspecified, GridElement source = null) {
        if (shield == null || Mathf.Sign(dmg) == -1) {
            if (Mathf.Sign(dmg) == 1) 
                PlaySound(dmgdSFX);
                         
            if (elementCanvas) {
                yield return StartCoroutine(elementCanvas.DisplayDamageNumber(dmg));
            }

            hpCurrent -= dmg;
            
            if (hpCurrent < 0) hpCurrent = 0;
            if (hpCurrent > hpMax) hpCurrent = hpMax;
            

        } else {
            RemoveShield();
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
        enabled = false;
        if (gameObject != null)
            Destroy(gameObject);
        
    }

    public virtual void TargetElement(bool state) 
    {
        targeted = state;
        if (elementCanvas) {
            elementCanvas.ToggleStatsDisplay(state);
        }
    }

    public virtual IEnumerator CollideFromBelow(GridElement above) {
        RemoveShield();
        yield return StartCoroutine(DestroySequence(DamageType.Gravity));
    }

    public virtual void OnSharedSpace(GridElement sharedWith) {
        
    }

    public virtual IEnumerator OnDamaged() {
        yield return null;
    }

    public virtual void ApplyShield(Shield _shield) {
        if (shield == null) {
            shield = _shield;
        }
    }

    public virtual void RemoveShield() {
        if (shield) {
// SHIELD UNIT TIER II - Heal unit on breaking
            if (shield.healing) {
                StartCoroutine(TakeDamage(-1));
            }
            Destroy(shield.gameObject);
            shield = null;
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

