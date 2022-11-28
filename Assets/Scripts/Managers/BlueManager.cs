using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class that controls the player's functionality, recieves input from PlayerController and ScenarioManager
// tbh I'm not commenting this one bc it's the most heavily edited and refactors are incoming

[RequireComponent(typeof(Deck))]
[RequireComponent(typeof(PlayerController))]
public class BlueManager : TokenManager {
    
    PlayerController pc;
    public GameObject gridCursor;

// Card vars
    public List<Card> hand;
    [HideInInspector] public Deck deck;
    public Card playedCard, selectedCard;
    [SerializeField] protected int handLimit;


    protected override void Start() {
        base.Start();
        pc = GetComponent<PlayerController>();
        deck=GetComponent<Deck>();
    }

    public override IEnumerator Initialize()
    {
        deck.InitializeDeck();
        yield return base.Initialize();
    }

    public void StartEndTurn(bool start) {
        for (int i = 0; i <= tokens.Count - 1; i++) 
            tokens[i].EnableSelection(start);
        
        ToggleHandSelect(start);

        if (start) {
            DrawToHandLimit();

            StartCoroutine(pc.GridInput());
            StartCoroutine(UpdateHandDisplay());
        } else {
            DeselectCard();
            DeselectToken();
        }
    }

    public override Token SpawnToken(Vector2 coord, int index) {
        Token t = base.SpawnToken(coord, index);
        t.owner = Token.Owner.Player;
        return t;
    }

    public void ToggleHandSelect(bool state) {
        foreach (Card card in hand) {
            card.hover.active = false;
            card.selectable = state;
        }
        if (selectedCard) selectedCard.hover.active = true;
        
        if (state) {
            StartCoroutine(pc.HandInput());
        } else 
            DeselectCard();
    }

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
                -w + (Util.cardSize * card.transform.localScale.x * i) - (Util.cardSize*card.transform.localScale.x/2), 
                -8);
            card.hover.UpdateOrigins();

            i++;
        }
    }

    protected virtual void DrawToHandLimit() {
        int toDraw = handLimit - hand.Count;
        for (int i = 0; i < toDraw; i++) {
            hand.Add(deck.DrawCard());
        }
    }

    public virtual void UpdatePlayedCardDisplay() {
        playedCard.EnableInput(false);
        playedCard.transform.position = new Vector2(-1.5f, -4.75f);
    }

    public virtual void SelectCard(Card c) {
        if (selectedCard) 
            DeselectCard();

        selectedCard = c;
        selectedCard.SelectCard();

        if (selectedToken)
            selectedToken.UpdateAction(selectedCard);

        c.EnableInput(false, true);
    }

    public virtual void DeselectCard() {
        if (selectedToken)
            selectedToken.UpdateAction();
        if (selectedCard) {        
            selectedCard.EnableInput(true);
            selectedCard = null;
        }
    }

    public override void SelectToken(Token t) {
        if (selectedToken)
            DeselectToken();

        selectedToken = t;
        if (selectedCard) 
            selectedToken.UpdateAction(selectedCard);
              
        ToggleGridCursor(true, t.coord);
    }
 
    public override void DeselectToken() {
        if (selectedToken) {
            selectedToken.UpdateAction();
            selectedToken = null;
            grid.DisableGridHighlight();
            ToggleGridCursor(false, Vector2.zero);
        }
    }

    public override void PlayCard() {
        hand.Remove(selectedCard);
        deck.discard.Add(selectedCard);

        selectedCard.gameObject.SetActive(false);
        selectedCard.EnableInput(false);
        
        playedCard = selectedCard;
        UpdatePlayedCardDisplay();
        
        DeselectCard();
        
        StartCoroutine(UpdateHandDisplay());
    }

    public override void MoveToken(Vector2 moveTo) {
        StartCoroutine(selectedToken.JumpToCoord(moveTo));
        PlayCard();
        DeselectToken();
    }

    void ToggleGridCursor(bool state, Vector2 coord) {
        gridCursor.SetActive(state);
        gridCursor.transform.position = Grid.PosFromCoord(coord);

    }


}
