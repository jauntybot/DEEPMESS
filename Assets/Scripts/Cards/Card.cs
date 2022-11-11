using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(OffsetOnHover))]
[RequireComponent(typeof(BoxCollider2D))]
public class Card : MonoBehaviour {
    
    OffsetOnHover hover;
    public CardData data;
    public BoxCollider2D hitbox;

    protected virtual void Start() {
        hover = GetComponent<OffsetOnHover>();
        hitbox = GetComponent<BoxCollider2D>();
    }

    public void Initialize(CardData cd) {
        this.data = cd;
        GetComponent<SpriteRenderer>().sprite = data.graphic;
    }

    public void OnMouseOver() {
        hover.active=true;
    }

    public void OnMouseExit() {
        hover.active=false;
    }
}
