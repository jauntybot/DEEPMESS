using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Slag/Anvil")]
[System.Serializable]   
public class AnvilData : SlagEquipmentData {
    [SerializeField] GameObject prefab;

    public override IEnumerator UseEquipment(GridElement user, GridElement target = null) {
        yield return base.UseEquipment(user, target);
        PlayerUnit pu = (PlayerUnit)user;

// Destroy instances exceeding anvil limit
        int activeAnvils = 0;
        List<Anvil> anvils = new();
        for (int i = 0; i <= pu.manager.units.Count - 1; i++) {
            if (pu.manager.units[i] is Anvil a) {
                anvils.Add(a);
                activeAnvils++;
            }
        }
        int anvilLimit = upgrades[UpgradePath.Unit] == 3 ? 1 : 0;
        if (activeAnvils > anvilLimit) {
            int destroyCount = activeAnvils - anvilLimit;
            for (int i = 0; i < destroyCount; i++) {
                anvils[0].StartCoroutine(anvils[0].DestroySequence());
                anvils.RemoveAt(0);
            }
        } 
            
// Spawn new Anvil and Initialize
        Anvil anvil = (Anvil)pu.manager.SpawnUnit(target.coord, prefab.GetComponent<Anvil>());
        anvil.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = 1;
        anvil.UpdateElement(pu.coord);      
        anvil.Init(this);
              
// Move to coord
        yield return user.StartCoroutine(MoveToCoord(pu, target.coord));
    }

public IEnumerator MoveToCoord(Unit unit, Vector2 moveTo) {
        float timer = 0;
        
// exposed UpdateElement() functionality to selectively update sort order
        if (unit.grid.SortOrderFromCoord(moveTo) > unit.grid.SortOrderFromCoord(unit.coord))
            unit.UpdateSortOrder(moveTo);
        unit.coord = moveTo;

// Lerp units position to target
        Vector3 toPos = FloorManager.instance.currentFloor.PosFromCoord(moveTo);
        while (timer < animDur) {
            yield return null;
            unit.transform.position = Vector3.Lerp(unit.transform.position, toPos, timer/animDur);
            timer += Time.deltaTime;
        }
        
        unit.UpdateElement(moveTo);
        yield return new WaitForSecondsRealtime(0.25f);
        if (!unit.targeted) unit.TargetElement(false);
    }

    public override void UpgradeEquipment(Unit user, UpgradePath targetPath) {
        base.UpgradeEquipment(user, targetPath);
        if (targetPath ==  UpgradePath.Unit) {
            if (upgrades[targetPath] == 0) {
                slag.hpMax = 3;
                if (slag.hpCurrent > slag.hpMax) slag.hpCurrent = slag.hpMax;
                slag.elementCanvas.InstantiateMaxPips();
                slag.ui.overview.InstantiateMaxPips();
            } else if (upgrades[targetPath] == 1) {
                slag.hpMax = 4;
                slag.hpCurrent ++;
                slag.elementCanvas.InstantiateMaxPips();
                slag.ui.overview.InstantiateMaxPips();
            }

            if (upgrades[targetPath] >= 2)
                range = 2;
            else range = 1;
        }
    }

}
