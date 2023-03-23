using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Swap")]
[System.Serializable]
public class SwapData : EquipmentData
{

    public override IEnumerator UseEquipment(GridElement user, GridElement target = null) {
        yield return base.UseEquipment(user);
        yield return user.StartCoroutine(SwapUnits((Unit)user, (Unit)target));
        
    }

    public IEnumerator SwapUnits(Unit unit1, Unit unit2) {

        Vector2 to1 = unit2.coord; Vector2 to2 = unit1.coord;
        yield return null;
        unit1.UpdateElement(to2); unit2.UpdateElement(to1);

    }

}
