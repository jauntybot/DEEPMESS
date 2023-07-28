using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Equipment/PerFloor/Shell")]
[System.Serializable]   
public class ShellData : PerFloorEquipmentData
{

    public override List<Vector2> TargetEquipment(GridElement user, int mod) {
        List<Vector2> validCoords = EquipmentAdjacency.GetAdjacent(user.coord, range + mod, this, null, user.grid);
        
        if (user is PlayerUnit u) {
          u.ui.ToggleEquipmentButtons();
          u.inRangeCoords = validCoords;  
        } 
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
