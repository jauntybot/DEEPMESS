using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class to control individual squares of a grid, linked to GridSquare prefab

[System.Serializable]
public class GridSquare : GroundElement {

    [SerializeField] SpriteRenderer[] spriteRenderers;
    public enum TileType { Bone, Blood, Bile };
    public TileType tileType;
    public bool white;
    [SerializeField] Color blackColor;
    [SerializeField] GameObject highlight;
    [SerializeField] List<Sprite> rndSprite;

// Initialize refs
    protected override void Start() {
        hitbox = GetComponent<PolygonCollider2D>();
        if (rndSprite.Count > 0)
            gfx[0].sprite = rndSprite[Random.Range(0,rndSprite.Count)];
    }

// Don't inherit base class initialization to avoid adding GridElement to wrong list
    public override void StoreInGrid(Grid owner) {
            grid = owner;
            transform.localScale = Vector3.one * FloorManager.sqrSize;
    }

// Toggle highlight gameobject active and update it's color
    public void ToggleValidCoord(bool state, Color? color = null) 
    {
        selectable = state;
        

        highlight.SetActive(state);
        if (color is Color c) {
            c.a = 50;
            UpdateHighlight(c);
        }
    }
    public void ToggleHitBox(bool state) {
        hitbox.enabled = state;
    }

// Made a function for 3 lines of code ig
    public virtual void UpdateHighlight(Color color) 
    {
        highlight.GetComponent<SpriteRenderer>().color = color;
        highlight.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = .5f;
    }

    public override void UpdateElement(Vector2 c)
    {
        base.UpdateElement(c);
        if (!white) {
// Apply color variant to GFX sprite renderer
            foreach (SpriteRenderer sr in spriteRenderers) 
                sr.color = blackColor;
        }
    }
}
