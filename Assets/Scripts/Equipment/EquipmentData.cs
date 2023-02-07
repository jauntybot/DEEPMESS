using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Custom data class, stores card type enum, sprite, and other things to come

[CreateAssetMenu(menuName = "Equipment")]
[System.Serializable]
public class EquipmentData : ScriptableObject {
    new public string name;
    public int energyCost;
    public enum Action { Move, Attack, PassHammer, StrikeHammer, None };
    public Action action;


// The following variables are dependent on the card Action, hidden with custom editor

    public enum AdjacencyType { Diamond, Orthogonal, Diagonal, Star, Box };
    public AdjacencyType adjacency;
    public int range; 
    public int shield;
    public int dmg;
}