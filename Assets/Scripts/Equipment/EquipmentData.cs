using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Custom data class, stores card type enum, sprite, and other things to come

[CreateAssetMenu(menuName = "Equipment")]
[System.Serializable]
public class EquipmentData : ScriptableObject {

    new public string name;
    public Image icon;
    public int gridColor;
    public int energyCost;
    public AdjacencyType adjacency;
    public int range;
    public List<GridElement> filters; 
// When false, filters out listed elements from adjacenecy checks, when true, only allows listed elements in adjacency checks
    public bool filterValid;
    protected float animDur = 0.5f;

// The following variables are dependent on the card Action, hidden with custom editor

    public enum AdjacencyType { Diamond, Orthogonal, Diagonal, Star, Box, OfType };

    public virtual List<Vector2> TargetEquipment(GridElement user) {
        List<Vector2> validCoords = new List<Vector2>();

        return validCoords;
    }

    public virtual IEnumerator UseEquipment(GridElement user, GridElement target = null) {
        user.energyCurrent -= energyCost;
        yield return null;
    }

    public virtual void EquipEquipment(GridElement user) {

    }

    public virtual void UnequipEquipment(GridElement user) {

    }
}