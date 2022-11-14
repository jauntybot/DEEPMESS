using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GridSquare : GridElement {

    [SerializeField] Sprite[] sprites;
    [HideInInspector] public bool white;
    [SerializeField] GameObject validMove;

    protected override void Start() {

        if (Grid.instance) 
            grid=Grid.instance;       
        hitbox = GetComponent<BoxCollider2D>();
        hitbox.enabled = false;
        
    }

    public override void UpdateElement(Vector2 c) {
        base.UpdateElement(c);
        if (white) GetComponent<SpriteRenderer>().sprite = sprites[1];
    }

    public void ToggleValidMove(bool state) {
        selectable = state;
        hitbox.enabled = state;

        validMove.SetActive(state);
    }


}
