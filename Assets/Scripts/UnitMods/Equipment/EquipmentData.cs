using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Custom data class, stores card type enum, sprite, and other things to come

[System.Serializable]
public class EquipmentData : ScriptableObject {

    new public string name;
    public Sprite icon;

    [Header("GRID DISPLAY")]
    public GameObject contextualAnimGO;
    public GridContextuals.ContextDisplay contextDisplay;
    public GridContextuals.ContextDisplay multiContext;
    public int gridColor;
    [SerializeField] protected GameObject vfx;
    
    [Header("MODIFIERS")]
    public bool multiselect;
    public GridElement firstTarget;
    public enum AdjacencyType { Diamond, Orthogonal, Diagonal, Star, Box, OfType, OfTypeInRange };
    public AdjacencyType adjacency;
    public int range;
    public AdjacencyType aoeAdjacency;
    public int aoeRange;
    public int energyCost;
    
    [SerializeField] protected float animDur = 0.5f;

    [Header("FILTERS")]
    public List<GridElement> filters;
    public List<Component> _filters; 
    public List<GridElement> targetTypes;

    [Header("AUDIO")]
    public SFX selectSFX;
    public SFX useSFX;

    public virtual List<Vector2> TargetEquipment(GridElement user, int mod = 0) {
        List<Vector2> validCoords = EquipmentAdjacency.GetAdjacent(user.coord, range + mod, this);

        user.grid.DisplayValidCoords(validCoords, gridColor);
        if (user is PlayerUnit u) {
            u.inRangeCoords = validCoords;  
            u.ui.ToggleEquipmentButtons();
        } 

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

        
        user.PlaySound(useSFX);

        yield return null;
    }

    public virtual void EquipEquipment(Unit user) {
        user.ui.UpdateEquipmentButtons();
        
    }

    public virtual void UnequipEquipment(Unit user) {
        if (user.equipment.Contains(this)) user.equipment.Remove(this);
        user.ui.UpdateEquipmentButtons();
    }

    public virtual IEnumerator UntargetDelay(Unit unit) {
        yield return new WaitForSecondsRealtime(0.25f);
        if (!unit.targeted) unit.TargetElement(false);
    }
}