using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(OffsetOnHover))]
[RequireComponent(typeof(BoxCollider2D))]
public class Card : MonoBehaviour {
    
    [HideInInspector] public OffsetOnHover hover;
    public bool selectable;
    public CardData data;
    [HideInInspector] public BoxCollider2D hitbox;
    [SerializeField] GameObject selectedBox;

    protected virtual void Start() {
        hover = GetComponent<OffsetOnHover>();
        hitbox = GetComponent<BoxCollider2D>();
        hitbox.enabled = false;

        selectedBox.SetActive(false);
        gameObject.SetActive(false);
    }

    public void Initialize(CardData cd) {
        this.data = cd;
        GetComponent<SpriteRenderer>().sprite = data.graphic;
    }

    public void OnMouseOver() {
        if (selectable)
            hover.active=true;
    }

    public void OnMouseExit() {
        if (selectable)
            hover.active=false;
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
