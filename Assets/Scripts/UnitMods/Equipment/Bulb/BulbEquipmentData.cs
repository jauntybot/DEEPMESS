using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BulbEquipmentData : EquipmentData
{

    [SerializeField] GameObject bulbPrefab;

    public override void EquipEquipment(GridElement user)
    {
        base.EquipEquipment(user);
        PlayerUnit pu = (PlayerUnit)user;
        pu.ui.UpdateEquipmentButtons();
    }

    public override IEnumerator UseEquipment(GridElement user, GridElement target = null)
    {
        yield return base.UseEquipment(user, target);
        PlayerUnit pu = (PlayerUnit)user;


        pu.ui.UpdateEquipmentButtons();
    }

}
