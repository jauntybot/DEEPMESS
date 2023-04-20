using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Equipment/Consumable/Shell")]
[System.Serializable]   
public class ShellData : ConsumableEquipmentData
{

    public override List<Vector2> TargetEquipment(GridElement user, int mod) {
        List<Vector2> validCoords = EquipmentAdjacency.GetAdjacent(user, range + mod, this);
        if (user is PlayerUnit u) u.ui.ToggleEquipmentButtons();
        for (int i = validCoords.Count - 1; i >= 0; i--) {
            foreach(GridElement ge in user.grid.CoordContents(validCoords[i])) {
                if (ge.shell) {
                    validCoords.RemoveAt(i);

                }
            }
        }
        user.grid.DisplayValidCoords(validCoords, gridColor);
        return validCoords;
    }

    public override IEnumerator UseEquipment(GridElement user, GridElement target = null)
    {
        yield return base.UseEquipment(user, target);
        target.ApplyShell();

    }

}
