using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Equipment/Slag/Shield")]
[System.Serializable]   
public class ShieldData : SlagEquipmentData {

    [SerializeField] GameObject shieldPrefab;
    public List<Shield> activeShields = new();

    public override void EquipEquipment(Unit user) {
        base.EquipEquipment(user);
        activeShields = new();
    }

    public override List<Vector2> TargetEquipment(GridElement user, int mod) {
        List<Vector2> validCoords = new();
        if (upgrades[UpgradePath.Shunt] == 3)
            validCoords = EquipmentAdjacency.OfTypeOnBoardAdjacency(targetTypes, user.grid);
        else 
            validCoords = EquipmentAdjacency.GetAdjacent(user.coord, range + mod, this, targetTypes, user.grid);
            
        validCoords.Add(user.coord);
        user.grid.DisplayValidCoords(validCoords, gridColor);

        if (user is PlayerUnit u) {
          u.ui.ToggleEquipmentButtons();
          u.inRangeCoords = validCoords;  
        } 
        for (int i = validCoords.Count - 1; i >= 0; i--) {
            if (user.grid.CoordContents(validCoords[i]).Count != 0) {
                bool remove = true;
                foreach(GridElement ge in user.grid.CoordContents(validCoords[i])) {
                    foreach (GridElement tar in targetTypes)
                        if (ge.GetType() == tar.GetType()) remove = false;
                    
                }
                if (remove) validCoords.RemoveAt(i);
            } else
                validCoords.RemoveAt(i);
        }
        return validCoords;
    }

    public override IEnumerator UseEquipment(GridElement user, GridElement target = null) {
        yield return base.UseEquipment(user, target);

        PlayerUnit pu = (PlayerUnit)user;
// SPECIAL TIER I - Increase shield limit
        int shieldLimit = upgrades[UpgradePath.Scab] >= 1 ? 1 : 0;
// Destory instances exceeding shield limit
        if (activeShields.Count > shieldLimit) {
            Shield s = activeShields[0];
            s.unit.RemoveShield();
            activeShields.Remove(s);
        } 
// Instantiate new shield obj
        Shield shield = Instantiate(shieldPrefab, target.transform).GetComponent<Shield>();
        if (target is Unit unit && user is Unit pony)
            shield.Init(unit, pony);

        activeShields.Add(shield);
        target.ApplyShield(shield);
    }

    public override void UpgradeEquipment(UpgradePath targetPath) {
        base.UpgradeEquipment(targetPath);
        if (targetPath == UpgradePath.Shunt) {
            if (upgrades[targetPath] == 0) {
                adjacency = AdjacencyType.Diamond;
                range = 1;
            }
            if (upgrades[targetPath] == 1) {
                range = 2;
            }
            if (upgrades[targetPath] == 2) {
                range = 3;
                slag.moveMod += 1;
            }
        } else if (targetPath ==  UpgradePath.Sludge) {
            if (upgrades[targetPath] == 1) {
                slag.hpMax += 1;
                slag.elementCanvas.InstantiateMaxPips();
                //slag.ui.overview.hPPips.UpdatePips();
                slag.StartCoroutine(slag.TakeDamage(-1, GridElement.DamageType.Heal));
                foreach(Shield shield in activeShields)
                    shield.buoyant = true;
            } else if (upgrades[targetPath] == 2) {
                foreach(Shield shield in activeShields)
                    shield.healing = true;
            }
        } else if (targetPath == UpgradePath.Scab && upgrades[targetPath] == 2) {
            foreach(Shield shield in activeShields)
                    shield.thorns = true;
        }
    }
}
