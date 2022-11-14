using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Deck))]
[RequireComponent(typeof(PlayerController))]
public class BlueManager : TokenManager {
    
    PlayerController pc;
    public GameObject gridCursor;

    protected override void Start() {
        base.Start();
        pc = GetComponent<PlayerController>();
    }

    public void StartEndTurn(bool start) {
        for (int i = 0; i <= tokens.Count - 1; i++) 
            tokens[i].EnableSelection(start);
        
        ToggleHandSelect(start);

        if (start) {
            StartCoroutine(StateMachine(State.Hand));
            DrawToHandLimit();

            StartCoroutine(pc.GridInput());
            StartCoroutine(UpdateHandDisplay());
        } else {
            DeselectCard();
            DeselectToken();
        }
    }

    public override Token SpawnToken(Vector2 coord) {
        Token t = base.SpawnToken(coord);
        t.owner = Token.Owner.Blue;
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
        var width = height*Camera.main.aspect;
        int i=1;
        foreach (Card card in hand) {
            yield return new WaitForSeconds(Util.initD);

            card.gameObject.SetActive(true);
            card.EnableInput(true);

            float w = Util.cardSize*hand.Count;
            card.transform.position = new Vector2(-w/2+Util.cardSize*i-Util.cardSize/2, -8);
            i++;
        }
    }

    public override void UpdatePlayedCardDisplay() {
        playedCard.EnableInput(false);
        playedCard.transform.position = new Vector2(-1.5f, -4.75f);
    }

    public override void SelectCard(Card c) {
        if (selectedCard) 
            DeselectCard();

        selectedCard = c;
        if (selectedToken)
            selectedToken.UpdateValidMoves(selectedCard);

        c.EnableInput(false, true);
    }

    public override void DeselectCard() {
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
            selectedToken.UpdateValidMoves(selectedCard);
              
        ToggleGridCursor(true, t.coord);
    }
 
    public override void DeselectToken() {
        selectedToken = null;
        grid.DisplayValidMoves(null);
        ToggleGridCursor(false, Vector2.zero);
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
        scenario.TurnAction();
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
