using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;


// Class to control individual squares of a grid, linked to GridSquare prefab

[System.Serializable]
public class GridSquare : GridElement {

    [SerializeField] SpriteRenderer[] spriteRenderers;
    public enum TileType { Bone, Blood, Bile };
    public TileType tileType;
    public bool white;
    [SerializeField] Color blackColor;
    [SerializeField] GameObject highlight;
    [SerializeField] List<Sprite> rndSprite;

    [SerializeField] Animator anim;

// Initialize refs
    protected virtual void Awake() {
        hitbox = GetComponent<PolygonCollider2D>();
        if (rndSprite.Count > 0)
            gfx[0].sprite = rndSprite[Random.Range(0,rndSprite.Count)];
        if (gfx[0].GetComponent<Animator>())
            anim = gfx[0].GetComponent<Animator>();

    }

// Don't inherit base class initialization to avoid adding GridElement to wrong list
    public override void StoreInGrid(Grid owner) {
        grid = owner;
        transform.localScale = Vector3.one * FloorManager.sqrSize;
    }

// Toggle highlight gameobject active and update it's color
    public void ToggleValidCoord(bool state, Color? color = null, bool fill = true) 
    {
        selectable = state;

        highlight.SetActive(state);
        if (color is Color c) {
            c.a = 50;
            UpdateHighlight(c, fill);
        }
    }
    public void ToggleHitBox(bool state) {
        hitbox.enabled = state;
    }

    public virtual void UpdateHighlight(Color color, bool fill) 
    {
        SpriteShapeRenderer ssr = highlight.GetComponent<SpriteShapeRenderer>();
        if (ssr) {
            ssr.color = new Color(color.r, color.g, color.b, fill ? 0.25f : 0);
            ssr.sortingOrder = grid.SortOrderFromCoord(coord);
        } else {
            highlight.GetComponent<SpriteRenderer>().color = color;
            highlight.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = .25f;
        }
        if (highlight.GetComponentInChildren<LineRenderer>()) {
            LineRenderer lr = highlight.GetComponentInChildren<LineRenderer>();
            lr.sortingOrder = grid.SortOrderFromCoord(coord);
            lr.startColor = color; lr.endColor = color;
        }
    }

    public override void UpdateElement(Vector2 c)
    {
        base.UpdateElement(c);
        if (!white) {
// Apply color variant to GFX sprite renderer
            foreach (SpriteRenderer sr in spriteRenderers) 
                sr.color = blackColor;
        }
        if (anim != null) {
            Debug.Log("remapped to " + Util.Remap(grid.SortOrderFromCoord(c), 0, 16, 0, 1));
            string name = anim.GetCurrentAnimatorClipInfo(0)[0].clip.name;
            anim.Play(name, 0, Util.Remap(grid.SortOrderFromCoord(c), 0, 16, 0, 4)%4);
        }
    }
}
