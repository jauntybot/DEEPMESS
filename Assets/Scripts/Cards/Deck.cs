using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Container class of Cards, functions to shuffle and draw

public class Deck : MonoBehaviour {
    
    PlayerManager manager;

    [SerializeField] GameObject cardPrefab;
    public List<CardData> atlas;
    public ShuffleBag<Card> deck;
    public List<Card> discard;
    // Card vars
    public List<Card> hand;
    public Card selectedCard;
    [SerializeField] protected int handLimit;

    void Start() {
        manager = GetComponent<PlayerManager>();
    }

// Instantiate and Initialize Card instances from atlas
    public virtual void InitializeDeck() {
        deck = new ShuffleBag<Card>();
        for (int i=0; i<atlas.Count; i++) {
            Card card = Instantiate(cardPrefab, this.transform).GetComponent<Card>();
            card.Initialize(atlas[i]);            
            deck.Add(card);
        }
    }

// Remove and return random card from deck, shuffle if deck empty
    public Card DrawCard() {
        if (deck.Count<=0) ShuffleDeck();
        if (deck.Count>0) {
            Card nextCard = deck.Next();
            deck.Remove(nextCard);

            return nextCard;
        }
        return null;
    }

// Return all cards from discard to deck
    public void ShuffleDeck() {
        int toShuffle = discard.Count - 1;
        for (int i = toShuffle; i >= 0; i--) {
            deck.Add(discard[i]);
        }
        discard = new List<Card>();
    }

    public void ToggleHandSelect(bool state) {
        foreach (Card card in hand) {
            card.hover.active = false;
            card.selectable = state;
        }
        if (selectedCard) selectedCard.hover.active = true;
        
        if (state) {
            StartCoroutine(manager.pc.HandInput());
        } else {
            DeselectCard();
            manager.pc.StopAllCoroutines();
        }
    }

// Hand mgmt
    public IEnumerator UpdateHandDisplay() {
        var height = 2*Camera.main.orthographicSize;
        var width = height*Camera.main.aspect/2;
        int i=0;
        foreach (Card card in hand) {
            yield return new WaitForSeconds(Util.initD);

            card.gameObject.SetActive(true);
            card.EnableInput(true);

            float scale = Mathf.Clamp(width/hand.Count/3f, 0.25f, 1.5f);
            card.transform.localScale = Vector3.one * scale;
            
            float w = Util.cardSize * card.transform.localScale.x * hand.Count;
            card.transform.position = new Vector2(
                -w + Camera.main.transform.position.x + (Util.cardSize * card.transform.localScale.x * i) , 
                -9);
            card.hover.UpdateOrigins();

            i++;
        }
    }

    public virtual void DrawToHandLimit() {
        int toDraw = handLimit - hand.Count;
        for (int i = 0; i < toDraw; i++) {
            hand.Add(DrawCard());
        }
    }

    public virtual void DiscardHand() {
        for (int i = hand.Count - 1; i >= 0; i--) {
            hand[i].gameObject.SetActive(false);
            hand[i].EnableInput(false);
            discard.Add(hand[i]);
            hand.Remove(hand[i]);
        }
    }

    public virtual void SelectCard(Card c) {
        if (selectedCard) 
            DeselectCard();

        selectedCard = c;
        selectedCard.SelectCard();

        if (manager.selectedUnit)
            manager.selectedUnit.UpdateAction(selectedCard);

        c.EnableInput(false, true);
    }

    public virtual void DeselectCard() {
        if (manager.selectedUnit)
            manager.selectedUnit.UpdateAction();
        if (selectedCard) {        
            selectedCard.EnableInput(true);
            selectedCard = null;
        }
    }
}

