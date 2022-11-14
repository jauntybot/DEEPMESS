using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TokenManager : MonoBehaviour {

// Global refs
    protected Grid grid;
    [HideInInspector] public ScenarioManager scenario;
    [HideInInspector] public Deck deck;
// State machine
    public enum State { Hand, Grid, Idle }
    public State state;
    public delegate void OnPlayerAction(State targetState);
    public virtual event OnPlayerAction stateChange;

// Card vars
    public List<Card> hand;
    public Card playedCard, selectedCard;
    [SerializeField] protected int handLimit;
// Token vars
    [SerializeField] GameObject[] tokenPrefabs;
    public List<Token> tokens = new List<Token>();    
    public Token selectedToken;
    public List<Vector2> startingCoords;



    protected virtual void Start() {
// Grab global refs
        if (ScenarioManager.instance) scenario = ScenarioManager.instance;
        if (Grid.instance) grid=Grid.instance;
        deck=GetComponent<Deck>();
    }

    public virtual IEnumerator StateMachine(State targetState) {
        yield return new WaitForSeconds(1/Util.fps);
        state=targetState;
    }

// Called from scenario manager when game starts
    public virtual IEnumerator Initialize() {
        deck.InitializeDeck();
        for (int i = 0; i <= startingCoords.Count - 1; i++) {
            SpawnToken(startingCoords[i]);
            yield return new WaitForSeconds(Util.initD);
        }
        DrawToHandLimit();
    }

    public virtual Token SpawnToken(Vector2 coord) {
        Token token = Instantiate(tokenPrefabs[0], this.transform).GetComponent<Token>();
        token.UpdateElement(coord);

        tokens.Add(token);
        return token;
    }

    protected virtual void DrawToHandLimit() {
        int toDraw = handLimit - hand.Count;
        for (int i = 0; i < toDraw; i++) {
            hand.Add(deck.DrawCard());
        }
    }

    public virtual void UpdatePlayedCardDisplay() {
        playedCard.gameObject.SetActive(true);
        playedCard.hitbox.enabled = false;
        playedCard.transform.position = new Vector2(-1.5f, -4.75f);
    }

    public virtual void SelectCard(Card c) {
        selectedCard = c;
        StateMachine(State.Grid);
    }

    public virtual void DeselectCard() {
        selectedCard = null;
        StateMachine(State.Hand);
    }

    public virtual void SelectToken(Token t) {
        DeselectToken();
        selectedToken = t;
    }
 
    public virtual void DeselectToken() {
        selectedToken = null;
    }

    public virtual void PlayCard() {

    }

    public virtual void MoveToken(Vector2 moveTo) {

    }
}

