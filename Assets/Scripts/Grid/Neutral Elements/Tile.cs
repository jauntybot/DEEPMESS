using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;


// Class to control individual squares of a grid, linked to Tile prefab

[System.Serializable]
public class Tile : GridElement {

    [SerializeField] SpriteRenderer[] spriteRenderers;
    public enum TileType { Bone, Blood, Bile };
    public TileType tileType;
    public bool white;
    [SerializeField] Color blackColor;
    [SerializeField] GameObject highlight;
    [SerializeField] List<Sprite> rndSprite;

    public Animator anim;

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
    public void ToggleValidCoord(bool state, Color? color = null, bool fill = true) {
        selectable = state;

        highlight.SetActive(state);
        if (color is Color c) {
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
            ssr.color = new Color(color.r, color.g, color.b, fill ? color.a : 0);
            ssr.sortingOrder = grid.SortOrderFromCoord(coord);
        }
        LineRenderer lr = highlight.GetComponentInChildren<LineRenderer>();
        if (lr) {
            lr.sortingOrder = grid.SortOrderFromCoord(coord);
            lr.startColor = new Color(color.r, color.g, color.b, 0.75f); lr.endColor = new Color(color.r, color.g, color.b, 0.75f);
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
// Offset tile animation to break up the grid
        if (anim != null) {
            string name = anim.GetCurrentAnimatorClipInfo(0)[0].clip.name;
            anim.Play(name, 0, Util.Remap(grid.SortOrderFromCoord(c), 0, 159, 0, 23));
        }
    }
}
