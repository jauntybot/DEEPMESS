using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour {
    

    [SerializeField] GameObject cardPrefab;
    public List<CardData> atlas;
    public ShuffleBag<Card> deck;
    public List<Card> discard;

    public virtual void InitializeDeck() {
        deck = new ShuffleBag<Card>();
        foreach(CardData card in atlas) {
            Card c = new Card(card,null);
            deck.Add(c);
        }
    }

    
    public Card DrawCard() {
        if (deck.Count>=0) ShuffleDeck();
        if (deck.Count>0) {
            Card nextCard = deck.Next();
            deck.Remove(nextCard);

            Card cardObj = Instantiate(cardPrefab).GetComponent<Card>();
            nextCard.Initialize(cardObj.gameObject);
            cardObj.GetComponent<SpriteRenderer>().sprite = nextCard.cardData.graphic;

            return nextCard;
        }
        return null;
    }

    public void ShuffleDeck() {
        foreach(Card card in discard) {
            discard.Remove(card);
            deck.Add(card);
        }

    }
}

