using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]   
public class ConsumableEquipmentData : EquipmentData
{

    [SerializeField] private int startCount;

    public override void EquipEquipment(GridElement user)
    {
        base.EquipEquipment(user);
        PlayerUnit pu = (PlayerUnit)user;
        pu.consumableCount = startCount;
        pu.ui.UpdateEquipmentButtons();
    }

    public override IEnumerator UseEquipment(GridElement user, GridElement target = null)
    {
        yield return base.UseEquipment(user, target);
        PlayerUnit pu = (PlayerUnit)user;
        pu.consumableCount--;
        pu.ui.UpdateEquipmentButtons();
    }

}
