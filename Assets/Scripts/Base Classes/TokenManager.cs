using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TokenManager : MonoBehaviour {

    Grid grid;
    public Deck deck;
    public List<Card> hand;
    [SerializeField] GameObject[] tokenPrefabs;

    public List<Token> tokens = new List<Token>();
    public int startTknCount;
    public List<Vector2> startingCoords;

    void Start() {
        if (Grid.instance) grid=Grid.instance;
        deck=GetComponent<Deck>();
    }

    public void Initialize() {
        deck.InitializeDeck();
        foreach(Vector2 coord in startingCoords) {
                SpawnToken(coord);
        }
    }

    public void SpawnToken(Vector2 coord) {
        Token token = Instantiate(tokenPrefabs[0], Grid.PosFromCoord(coord), Quaternion.identity, this.transform).GetComponent<Token>();
        token.Initialize(token.gameObject, Grid.PosFromCoord(coord), coord);

        tokens.Add(token);
    }
        
    void DrawToFour() {
        while (hand.Count < 4) {
            hand.Add(deck.DrawCard());
        }
    }
}

