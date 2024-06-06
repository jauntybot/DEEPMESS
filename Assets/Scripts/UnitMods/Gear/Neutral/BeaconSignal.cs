using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Gear/Neutral/Beacon")]
[System.Serializable]
public class BeaconSignal : GearData {

    UpgradeManager upgradeManager;

    

    public override List<Vector2> TargetEquipment(GridElement user, int mod = 0) {
        user.StartCoroutine(UseGear(user));
        return null;
    }


    public override IEnumerator UseGear(GridElement user, GridElement target = null) {
        Beacon b = (Beacon)user;
        b.manager.DeselectUnit();
        
        yield return base.UseGear(user, target);

        yield return new WaitForSecondsRealtime(1.25f);
        b.StartCoroutine(b.SelectBeacon());

        
    }

}
