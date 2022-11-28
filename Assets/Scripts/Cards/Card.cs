using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Container class for card data, serialized and instanced in Unity scenes

[RequireComponent(typeof(OffsetOnHover))]
[RequireComponent(typeof(BoxCollider2D))]
public class Card : MonoBehaviour {

// Instanced refs
    [SerializeField] SpriteRenderer cardGFX;
    [SerializeField] GameObject selectedBox;

// Animation
    [HideInInspector] public OffsetOnHover hover;

// Card data
    public bool selectable;
    public CardData data;
    [HideInInspector] public BoxCollider2D hitbox;

// Initialize references, set default state
    protected virtual void Start() {
        hover = GetComponent<OffsetOnHover>();
        hitbox = GetComponent<BoxCollider2D>();
        hitbox.enabled = false;

        selectedBox.SetActive(false);
        gameObject.SetActive(false);
    }

// Update card data
    public void Initialize(CardData cd) {
        this.data = cd;
        cardGFX.sprite = data.graphic;
    }

// Hover animation
    public void OnMouseOver() {
        if (selectable)
            hover.active=true;
    }
    public void OnMouseExit() {
        if (selectable)
            hover.active=false;
    }

// Hand inputs
    public void SelectCard() {
        hover.Selected();
    }
    public void EnableInput(bool state, bool slctd = false) {
        hover.active = !state;

        selectable = state;
        hitbox.enabled = state;
        
        if (slctd)
            selectedBox.SetActive(true);
        else
            selectedBox.SetActive(false);
    }
}
