using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base class for enemy and player managers

public class TokenManager : MonoBehaviour {

// Global refs
    protected Grid grid;
    [HideInInspector] public ScenarioManager scenario;

// Token vars
    [SerializeField] GameObject[] tokenPrefabs;
    public List<Token> tokens = new List<Token>();    
    public Token selectedToken;
    public List<Vector2> startingCoords;



    protected virtual void Start() 
    {
// Grab global refs
        if (ScenarioManager.instance) scenario = ScenarioManager.instance;
        if (Grid.instance) grid = Grid.instance;

    }

// Called from scenario manager when game starts
    public virtual IEnumerator Initialize() 
    {
        for (int i = 0; i <= startingCoords.Count - 1; i++) {
            SpawnToken(startingCoords[i], i);
            yield return new WaitForSeconds(Util.initD);
        }
    }

// Create a new token from prefab index, update its GridElement
    public virtual Token SpawnToken(Vector2 coord, int index) 
    {
        Token token = Instantiate(tokenPrefabs[index], this.transform).GetComponent<Token>();
        token.UpdateElement(coord);

        tokens.Add(token);
        return token;
    }

// Inherited functionality dependent on inherited classes
    public virtual void SelectToken(Token t) {}
    public virtual void DeselectToken() {}
    public virtual void PlayCard() {}
    public virtual void MoveToken(Vector2 moveTo) {}
}

