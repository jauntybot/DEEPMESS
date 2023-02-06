using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Universal data class derrived by any instance that occupies grid space

//[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(HPDisplay))]
public class GridElement : MonoBehaviour{

    [SerializeField]
    protected Grid grid;

    [Header("Grid Element")]
    public Vector2 coord;
    public bool selectable, targeted;
    public PolygonCollider2D hitbox;
    [HideInInspector] public HPDisplay hpDisplay;


    public delegate void OnElementUpdate(GridElement ge);
    public event OnElementUpdate ElementDestroyed;

    public int hpMax, hpCurrent, defense;
    public int energyCurrent, energyMax;

// Initialize references, scale to grid, subscribe onDeath event
    protected virtual void Start() 
    {
        hitbox = GetComponent<PolygonCollider2D>();
        hitbox.enabled = false;

        hpCurrent = hpMax;
        energyCurrent = energyMax;
        hpDisplay = GetComponent<HPDisplay>();
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
        coord=c;
    }  

    public virtual void EnableSelection(bool state) {}

// Apply shield to this element
    public virtual IEnumerator Defend(int value) 
    {
        yield return null;
        defense += value;
        hpDisplay.UpdateHPDisplay();
    }
    
    public virtual IEnumerator TakeDamage(int dmg) 
    {
        defense -= dmg;
        if (Mathf.Sign(defense) == -1) {
            hpCurrent += defense;
            if (hpCurrent <= 0) {
                StartCoroutine(DestroyElement());
                yield break;
            }
            defense = 0;
        }
        hpDisplay.UpdateHPDisplay();
        yield return new WaitForSecondsRealtime(.5f);
        TargetElement(false);
    }

    public virtual IEnumerator DestroyElement() 
    {
        ElementDestroyed?.Invoke(this);
        yield return new WaitForSecondsRealtime(.5f);
        DestroyImmediate(this.gameObject);
    }

    public virtual void TargetElement(bool state) 
    {
        targeted = state;
        if (hpDisplay) hpDisplay.ToggleHPDisplay(state);
    }
    
}

