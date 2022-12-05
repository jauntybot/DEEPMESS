using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Universal data class for any instance that occupies grid space

[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(HPDisplay))]
public class GridElement : MonoBehaviour{

    protected Grid grid;

    [Header("Grid Element")]
    public Vector2 coord;
    public bool selectable;
    public PolygonCollider2D hitbox;
    [HideInInspector] public HPDisplay hpDisplay;


    public delegate void OnElementUpdate(GridElement ge);
    public event OnElementUpdate ElementDestroyed;

    public int hpMax, hpCurrent, defense;

// Initialize references, scale to grid, subscribe onDeath event
    protected virtual void Start() {
        if (Grid.instance) {
            grid=Grid.instance;
            grid.gridElements.Add(this);
            ElementDestroyed += grid.RemoveElement;
            transform.localScale = Vector3.one * Grid.sqrSize;
        }
        hitbox = GetComponent<PolygonCollider2D>();
        hitbox.enabled = false;

        hpCurrent = hpMax;

        hpDisplay = GetComponent<HPDisplay>();
    }

// Update grid position and coordinate
    public virtual void UpdateElement(Vector2 c) {
        transform.position = Grid.PosFromCoord(c);
        coord=c;
    }  

// Apply shield to this element
    public virtual IEnumerator Defend(int value) 
    {
        yield return new WaitForEndOfFrame();
        defense += value;
        hpDisplay.UpdateHPDisplay();
    }
    
    public virtual void TakeDamage(int dmg) {
        defense -= dmg;
        if (Mathf.Sign(defense) == -1) {
            hpCurrent += defense;
            if (hpCurrent <= 0)
                StartCoroutine(DestroyElement());
            defense = 0;
        }
        hpDisplay.UpdateHPDisplay();
    }

    public virtual IEnumerator DestroyElement() {
        ElementDestroyed?.Invoke(this);
        yield return new WaitForSecondsRealtime(.5f);
        DestroyImmediate(this.gameObject);
    }

    
}

