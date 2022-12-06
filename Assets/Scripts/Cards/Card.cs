using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// Container class for card data, serialized and instanced in Unity scenes

[RequireComponent(typeof(OffsetOnHover))]
[RequireComponent(typeof(BoxCollider2D))]
public class Card : MonoBehaviour {

// Serialized refs
    [SerializeField] SpriteRenderer cardBG;
    [SerializeField] Sprite[] bgSprites;
    [SerializeField] GameObject selectedBox;
    [SerializeField] Image actionIcon;
    [SerializeField] List<Sprite> actionSprites;

    [SerializeField] TMPro.TMP_Text valueText;
    [SerializeField] TMPro.TMP_Text costText;

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
        switch (data.action) 
        {
            case CardData.Action.Move:
                cardBG.sprite = bgSprites[0];
                SwitchIcon();
                valueText.text = data.range.ToString();
            break;
            case CardData.Action.Attack:
                cardBG.sprite = bgSprites[1];
                SwitchIcon();
                valueText.text = data.range.ToString();

            break;
            case CardData.Action.Defend:
                cardBG.sprite = bgSprites[2];
                actionIcon.sprite = actionSprites[4];
                valueText.text = data.shield.ToString();
            break;
        }
        costText.text = data.energyCost.ToString();
    }

    void SwitchIcon() 
    {
        switch (data.adjacency) 
        {
            case CardData.AdjacencyType.Orthogonal:
                actionIcon.sprite = actionSprites[0];
            break;
            case CardData.AdjacencyType.Diagonal:
                actionIcon.sprite = actionSprites[1];
            break;
            case CardData.AdjacencyType.Diamond:
                actionIcon.sprite = actionSprites[2];
            break;
            case CardData.AdjacencyType.Box:
                actionIcon.sprite = actionSprites[3];
            break;

        }
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