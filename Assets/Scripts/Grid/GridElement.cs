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
    public enum DamageType { Unspecified, Heal, Melee, Fall, Crush, Bile, Explosion };
    public int hpMax, hpCurrent;
    public Shield shield;
    public int energyCurrent, energyMax;
    public bool destroyed;

    [Header("UI/UX")]
    public List<SpriteRenderer> gfx;

    public delegate void OnElementUpdate(GridElement ge);
    public virtual event OnElementUpdate ElementUpdated;
    public virtual event OnElementUpdate ElementDestroyed;

    
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

  
    public virtual IEnumerator TakeDamage(int dmg, DamageType dmgType = DamageType.Unspecified, GridElement source = null, EquipmentData sourceEquip = null) {
        if (!destroyed) {
            ObjectiveEventManager.Broadcast(GenerateDamageEvent(dmgType, dmg, source, sourceEquip));
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
            //yield return new WaitForSecondsRealtime(.4f);
            TargetElement(false);
            if (hpCurrent <= 0) {
                yield return StartCoroutine(DestroySequence(dmgType, source, sourceEquip));
            }
        }
    }

    protected virtual GridElementDamagedEvent GenerateDamageEvent(DamageType dmgType, int dmg, GridElement source = null, EquipmentData sourceEquip = null) {
        GridElementDamagedEvent evt = ObjectiveEvents.GridElementDamagedEvent;
        evt.element = this;
        evt.damageType = dmgType;
        evt.dmg = dmg;
        evt.source = source;
        evt.sourceEquip = sourceEquip;
        return evt;
    }

    public virtual IEnumerator DestroySequence(DamageType dmgType = DamageType.Unspecified, GridElement source = null, EquipmentData sourceEquip = null) {
        if (!destroyed) {
            destroyed = true;
            ElementDestroyed?.Invoke(this);
            ObjectiveEventManager.Broadcast(GenerateDestroyEvent(dmgType, source, sourceEquip));        

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
    }

    protected virtual GridElementDestroyedEvent GenerateDestroyEvent(DamageType dmgType = DamageType.Unspecified, GridElement source = null, EquipmentData sourceEquip = null) {
        GridElementDestroyedEvent evt = ObjectiveEvents.GridElementDestroyedEvent;
        evt.element = this;
        evt.damageType = dmgType;
        evt.source = source;
        evt.sourceEquip = sourceEquip;
        return evt;
    }

    public virtual void TargetElement(bool state) {
        targeted = state;
        if (elementCanvas) {
            elementCanvas.ToggleStatsDisplay(state);
        }
    }

    public virtual IEnumerator CollideFromBelow(GridElement above) {
        RemoveShield();
        yield return StartCoroutine(DestroySequence(DamageType.Crush, above));
    }

// For when a Slag is acting on a Unit to move it, such as BigGrab or any push mechanics
    public virtual IEnumerator CollideFromBelow(GridElement above, GridElement source, EquipmentData sourceEquip) {
        RemoveShield();
        yield return StartCoroutine(DestroySequence(DamageType.Crush, source, sourceEquip));
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

