using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class to control individual squares of a grid, linked to GridSquare prefab

[System.Serializable]
public class GridSquare : GridElement {

    [SerializeField] SpriteRenderer[] spriteRenderers;
    [SerializeField] Sprite[] sprites;
    [HideInInspector] public bool white;
    [SerializeField] Color blackColor;
    [SerializeField] GameObject highlight;

// Initialize refs
    protected override void Start()
    {
// Don't inherit base class initialization to avoid adding GridElement to wrong list
        if (Grid.instance) 
        {
            grid = Grid.instance;
            transform.localScale = Vector3.one * Grid.sqrSize;
        }
        hitbox = GetComponent<PolygonCollider2D>();
        hitbox.enabled = false;
// Temporary checkerboard, color sprite renderers
        if (!white) 
        {
            foreach (SpriteRenderer sr in spriteRenderers) 
                sr.color = blackColor;
        }
    }

// Toggle highlight gameobject active and update it's color
    public void ToggleValidCoord(bool state, Color? color = null) 
    {
        selectable = state;
        hitbox.enabled = state;

        highlight.SetActive(state);
        if (color is Color c) 
            UpdateHighlight(c);
    }

// Made a function for 3 lines of code ig
    public virtual void UpdateHighlight(Color color) 
    {
        highlight.GetComponent<SpriteRenderer>().color = color;
        foreach (SpriteRenderer sr in highlight.GetComponentsInChildren<SpriteRenderer>())
            sr.color = color;
    }
}
