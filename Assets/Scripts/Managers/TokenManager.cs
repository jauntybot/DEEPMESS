using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TokenManager : MonoBehaviour {

// Global refs
    protected Grid grid;
    [HideInInspector] public ScenarioManager scenario;

// Token vars
    [SerializeField] GameObject[] tokenPrefabs;
    public List<Token> tokens = new List<Token>();    
    public Token selectedToken;
    public List<Vector2> startingCoords;



    protected virtual void Start() {
// Grab global refs
        if (ScenarioManager.instance) scenario = ScenarioManager.instance;
        if (Grid.instance) grid=Grid.instance;

    }

// Called from scenario manager when game starts
    public virtual IEnumerator Initialize() {
        for (int i = 0; i <= startingCoords.Count - 1; i++) {
            SpawnToken(startingCoords[i]);
            yield return new WaitForSeconds(Util.initD);
        }
    }

    public virtual Token SpawnToken(Vector2 coord) {
        Token token = Instantiate(tokenPrefabs[0], this.transform).GetComponent<Token>();
        token.UpdateElement(coord);

        tokens.Add(token);
        return token;
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

