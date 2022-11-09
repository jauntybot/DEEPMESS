using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioManager : MonoBehaviour
{
    Grid grid;
    [SerializeField] TokenManager[] tknMngrs;

    public enum Turn { Player, Opponent, Environment }
    public Turn currentTurn;

    public void Start() {
        if (Grid.instance) {
            grid=Grid.instance;
            grid.GenerateGrid();
        }
        foreach(TokenManager manager in tknMngrs) {
            manager.Initialize();
        }
    }

    public void SwitchTurns(Turn turn) {
        switch(currentTurn) {
            case Turn.Player:

            break;
            case Turn.Opponent:

            break;
            case Turn.Environment:

            break;
        }
    }

    public void TurnAction() {
        switch(currentTurn) {
            case Turn.Player:

            break;
            case Turn.Opponent:

            break;
            case Turn.Environment:

            break;
        }
    }
}
