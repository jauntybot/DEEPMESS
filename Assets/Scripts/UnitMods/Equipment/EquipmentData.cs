using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Custom data class, stores card type enum, sprite, and other things to come

[CreateAssetMenu(menuName = "Equipment")]
[System.Serializable]
public class EquipmentData : ScriptableObject {

    new public string name;
    public Sprite icon;

    [Header("GRID DISPLAY")]
    public GameObject contextualAnimGO;
    public GridContextuals.ContextDisplay contextDisplay;
    public GridContextuals.ContextDisplay multiContext;
    public int gridColor;
    
    [Header("MODIFIERS")]
    public bool multiselect;
    public AdjacencyType adjacency;
    public enum AdjacencyType { Diamond, Orthogonal, Diagonal, Star, Box, OfType, OfTypeInRange };
    public GridElement firstTarget;
    public int energyCost;
    public int range;
    [SerializeField] protected float animDur = 0.5f;

    [Header("FILTERS")]
// When false, filters out listed elements from adjacenecy checks, when true, only allows listed elements in adjacency checks
    public bool filterValid;
    public List<GridElement> filters; 
    public List<GridElement> targetTypes;

    [Header("AUDIO")]
    public SFX selectSFX;
    public SFX useSFX;



    public virtual List<Vector2> TargetEquipment(GridElement user, int mod = 0) {
        List<Vector2> validCoords = EquipmentAdjacency.GetAdjacent(user, range + mod, this);
        user.grid.DisplayValidCoords(validCoords, gridColor);
        if (user is PlayerUnit u) u.ui.ToggleEquipmentButtons();
        if (selectSFX)
            user.PlaySound(selectSFX.Get());

        return validCoords;
    }

    public virtual void UntargetEquipment(GridElement user) {
        
    }

    public virtual IEnumerator UseEquipment(GridElement user, GridElement target = null) {
        user.energyCurrent -= energyCost;
        if (user is PlayerUnit pu) {
            PlayerManager manager = (PlayerManager)pu.manager;
            manager.undoableMoves = new Dictionary<Unit, Vector2>();
            manager.undoOrder = new List<Unit>();
        }
        user.elementCanvas.UpdateStatsDisplay();

        if (useSFX)
            user.PlaySound(useSFX.Get());

        yield return null;
    }

    public virtual void EquipEquipment(GridElement user) {

    }

    public virtual void UnequipEquipment(GridElement user) {

    }
}