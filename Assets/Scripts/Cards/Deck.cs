using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Container class of Cards, functions to shuffle and draw

public class Deck : MonoBehaviour {
    

    [SerializeField] GameObject cardPrefab;
    public List<CardData> atlas;
    public ShuffleBag<Card> deck;
    public List<Card> discard;

// Instantiate and Initialize Card instances from atlas
    public virtual void InitializeDeck() {
        deck = new ShuffleBag<Card>();
        Debug.Log(atlas.Count);
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
}

