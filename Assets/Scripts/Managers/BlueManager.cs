using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Deck))]
[RequireComponent(typeof(PlayerController))]
public class BlueManager : TokenManager {
    
    PlayerController pc;

    protected override void Start() {
        base.Start();
        pc = GetComponent<PlayerController>();
    }

    public override void Initialize() {
        base.Initialize();
        UpdateHandDisplay();
        UpdatePlayedCardDisplay();
    }

    public override void StateMachine(State targetState) {
        base.StateMachine(targetState);
        pc.ChangeStates(targetState);

        switch (targetState) {
            case State.Hand:

            break;
            case State.Grid:

            break;
            case State.Idle:

            break;
        }
    }

    public void UpdateHandDisplay() {
        var height = 2*Camera.main.orthographicSize;
        var width = height*Camera.main.aspect;
        Debug.Log(hand.Count-1);
        int i=1;
        foreach (Card card in hand) {
            card.gameObject.SetActive(true);
            card.hitbox.enabled = true;
            float w = Util.cardSize*hand.Count;
            card.transform.position = new Vector2(-w/2+Util.cardSize*i-Util.cardSize/2, -8);
            i++;
        }
    }

    public override void UpdatePlayedCardDisplay() {
        playedCard.gameObject.SetActive(true);
        playedCard.hitbox.enabled = false;
        playedCard.transform.position = new Vector2(-1.5f, -4.75f);
    }

    public void SelectCard(Card c) {
        selectedCard = c;
        StateMachine(State.Grid);
    }

    public void DeselectCard() {
        selectedCard = null;
        StateMachine(State.Hand);
    }

    public void SelectToken(Token t) {
        selectedToken = t;
    }
 
    public void DeselectToken() {
        selectedToken = null;
    }

    public void PlayCard(Card c) {

    }

    public void MoveToken() {

    }


}
