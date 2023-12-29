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
// SPECIAL TIER II -- increase anvil limit
        int anvilLimit = upgrades[UpgradePath.Special] >= 2 ? 1 : 0;
// Destroy active anvils over anvil limit
        if (activeAnvils > anvilLimit) {
            int destroyCount = activeAnvils - anvilLimit;
            for (int i = 0; i < destroyCount; i++) {
                anvils[0].StartCoroutine(anvils[0].DestroySequence());
                anvils.RemoveAt(0);
            }
        } 
            
// Spawn new Anvil and Initialize
        Anvil anvil = (Anvil)pu.manager.SpawnUnit(prefab.GetComponent<Anvil>(), target.coord);
        anvil.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = 1;
        anvil.UpdateElement(pu.coord);      
        anvil.Init(this);
              
// Move to coord
        yield return user.StartCoroutine(MoveToCoord(pu, target.coord));
    }

    public virtual IEnumerator MoveToCoord(Unit unit, Vector2 moveTo, bool undo = false) {  
        if (upgrades[UpgradePath.Unit] < 2) {
// Build frontier dictionary for stepped lerp
            Dictionary<Vector2, Vector2> fromTo = new();
            fromTo = EquipmentAdjacency.SteppedCoordAdjacency(unit.coord, moveTo, this);

            Vector2 current = unit.coord;
            unit.coord = moveTo;
            
// Lerp units position to target
            while (!Vector2.Equals(current, moveTo)) {
// exposed UpdateElement() functionality to selectively update sort order
                if (unit.grid.SortOrderFromCoord(fromTo[current]) > unit.grid.SortOrderFromCoord(current))
                    unit.UpdateSortOrder(fromTo[current]);
                Vector3 toPos = FloorManager.instance.currentFloor.PosFromCoord(fromTo[current]);
                float timer = 0;
                while (timer < animDur) {
                    yield return null;
                    unit.transform.position = Vector3.Lerp(unit.transform.position, toPos, timer/animDur);
                    timer += Time.deltaTime;
                }
                current = fromTo[current];
                yield return null;
            }        
        } else {
            if (unit.grid.SortOrderFromCoord(moveTo) > unit.grid.SortOrderFromCoord(unit.coord))
                unit.UpdateSortOrder(moveTo);
            float timer = 0;
            float throwDur = animDur * Vector2.Distance(unit.coord, moveTo);
            Vector3 startPos = unit.transform.position;
            Vector3 endPos = unit.grid.PosFromCoord(moveTo);
            float h = 0.25f + Vector2.Distance(unit.coord, moveTo) / 2;
            while (timer < throwDur) {
                unit.transform.position = Util.SampleParabola(startPos, endPos, h, timer/throwDur);
                yield return null;
                timer += Time.deltaTime;
            }
            unit.transform.position = endPos;
        }

        unit.UpdateElement(moveTo);
    }

    public override void UpgradeEquipment(UpgradePath targetPath) {
        base.UpgradeEquipment(targetPath);
        if (targetPath ==  UpgradePath.Unit) {
// UNIT TIER I - Upgrade move distance after placing
            if (upgrades[targetPath] == 1)
                range = 3;
// UNIT TIER II - Upgrade unit base movement
            else if (upgrades[targetPath] == 2) {
                slag.moveMod += 1;
                adjacency = AdjacencyType.OfTypeInRange;
                contextDisplay = GridContextuals.ContextDisplay.Parabolic;
            }
            else {
                range = 1;
                adjacency = AdjacencyType.Diamond;
                contextDisplay = GridContextuals.ContextDisplay.Stepped;
            }
        } else if (targetPath == UpgradePath.Special) {
// SPECIAL TIER I - Upgrade unit HP
            if (upgrades[targetPath] == 1) {
                slag.hpMax += 1;
                slag.hpCurrent += 1;
                if (slag.hpCurrent > slag.hpMax) slag.hpCurrent = slag.hpMax;
                slag.elementCanvas.InstantiateMaxPips();
                slag.ui.overview.InstantiateMaxPips();
            }
        } else if (targetPath == UpgradePath.Power) {
// POWER TIER II - Upgrade unit HP
            if (upgrades[targetPath] == 2) {
                slag.hpMax += 1;
                slag.hpCurrent += 1;
                if (slag.hpCurrent > slag.hpMax) slag.hpCurrent = slag.hpMax;
                slag.elementCanvas.InstantiateMaxPips();
                slag.ui.overview.InstantiateMaxPips();
            }
        }
    }

// Set unit max HP
                // slag.hpMax = 3;
                // if (slag.hpCurrent > slag.hpMax) slag.hpCurrent = slag.hpMax;
                // slag.elementCanvas.InstantiateMaxPips();
                // slag.ui.overview.InstantiateMaxPips();
}
