using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Custom data class, stores card type enum, sprite, and other things to come

[CreateAssetMenu(menuName = "Card")]
[System.Serializable]
public class CardData : ScriptableObject {
    public int energyCost;
    public enum Action { Move, Attack, Defend };
    public Action action;


// The following variables are dependent on the card Action

    public enum AdjacencyType { Diamond, Orthogonal, Diagonal, Star, Box };
    [HideInInspector] public AdjacencyType adjacency;

    [HideInInspector] public int range; 
    [HideInInspector] public int shield;
    [HideInInspector] public int dmg;
}