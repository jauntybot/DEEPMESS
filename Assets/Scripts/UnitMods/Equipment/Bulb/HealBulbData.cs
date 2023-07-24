using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Bulb/Heal")]
public class HealBulbData : BulbEquipmentData
{

    public override List<Vector2> TargetEquipment(GridElement user, int mod = 0)
    {
        List<Vector2> validCoords = EquipmentAdjacency.GetAdjacent(user, range + mod, this);
        Debug.Log("first: " + validCoords.Count);
        if (user is PlayerUnit u) {
            u.ui.ToggleEquipmentButtons();
            u.inRangeCoords = validCoords;
        }
        user.grid.DisplayValidCoords(validCoords, gridColor);
        for (int i = validCoords.Count - 1; i >= 0; i--) {
            if (user.grid.CoordContents(validCoords[i]).Count != 0) {
                foreach(GridElement ge in user.grid.CoordContents(validCoords[i])) {
                    Debug.Log(ge.name);
                    if (ge is not PlayerUnit)
                        validCoords.Remove(validCoords[i]);
                }
            } else 
                validCoords.Remove(validCoords[i]);
        }
        Debug.Log("last: " + validCoords.Count);
        return validCoords;
    }


    public override IEnumerator UseEquipment(GridElement user, GridElement target = null)
    {
        return base.UseEquipment(user, target);
//     int missing = sharedWith.hpMax - sharedWith.hpCurrent;
            //     if (missing > 0) {
            //         usedValue = value - missing;
            //         usedValue = usedValue <= 0 ? value : usedValue;
            //     }                        
            //     StartCoroutine(sharedWith.TakeDamage(-value));
            //     if (sharedWith is PlayerUnit u && ScenarioManager.instance.currentTurn == ScenarioManager.Turn.Player)
            //         StartCoroutine(WaitForUndoClear(u.pManager));
            //     else
            //         StartCoroutine(DestroyElement());
    }
}
