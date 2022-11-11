using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TokenManager : MonoBehaviour {

// Global refs
    Grid grid;
    [HideInInspector] public Deck deck;
// State machine
    public enum State { Hand, Grid, Idle }
    public State state;
    public delegate void OnPlayerAction(State targetState);
    public virtual event OnPlayerAction stateChange;

// Card vars
    public List<Card> hand;
    public Card playedCard, selectedCard;

// Token vars
    [SerializeField] GameObject[] tokenPrefabs;
    public List<Token> tokens = new List<Token>();    
    public Token selectedToken;
    public List<Vector2> startingCoords;



    protected virtual void Start() {
// Grab global refs
        if (Grid.instance) grid=Grid.instance;
        deck=GetComponent<Deck>();
    }

    public virtual void StateMachine(State targetState) {
        state=targetState;
    }

// Called from scenario manager when game starts
    public virtual void Initialize() {
        deck.InitializeDeck();
        foreach(Vector2 coord in startingCoords) {
            SpawnToken(coord);
        }
        DrawCard();
        DrawCard();
        DrawCard();
        Debug.Log("initialized");
    }

    public void SpawnToken(Vector2 coord) {
        Token token = Instantiate(tokenPrefabs[0], this.transform).GetComponent<Token>();
        token.UpdateElement(token.gameObject, coord);

        tokens.Add(token);
    }

    protected virtual void DrawCard() {
        hand.Add(deck.DrawCard());
    }

    public virtual void UpdatePlayedCardDisplay() {
        playedCard.gameObject.SetActive(true);
        playedCard.hitbox.enabled = false;
        playedCard.transform.position = new Vector2(-1.5f, -4.75f);
    }
}

