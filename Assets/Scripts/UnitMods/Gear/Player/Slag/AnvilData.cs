using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Gear/Slag/Anvil")]
[System.Serializable]   
public class AnvilData : SlagGearData {
    
    [Header("ANVIL PARAMS")]
    [SerializeField] GameObject prefab;
    public int anvilHP;
    public int anvilLimit;
    public bool reinforcedBottom, explode, liveWire, crystalize;


    public override void EquipGear(Unit user) {
        base.EquipGear(user);
        reinforcedBottom = false;
        explode = false;
        liveWire = false;
        crystalize = false;
        range = 1;
        anvilHP = 1;
        anvilLimit = 1;
    }

    public override IEnumerator UseGear(GridElement user, GridElement target = null) {
        yield return base.UseGear(user, target);
        PlayerUnit pu = (PlayerUnit)user;

        OnEquipmentUse evt = ObjectiveEvents.OnEquipmentUse;
        evt.data = this; evt.user = user; evt.target = target;
        ObjectiveEventManager.Broadcast(evt);

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
        
// Destroy active anvils over anvil limit
        if (activeAnvils >= anvilLimit) {
            anvils[0].StartCoroutine(anvils[0].DestroySequence());
        } 
            
// Spawn new Anvil and Initialize
        Anvil anvil = (Anvil)pu.manager.SpawnUnit(prefab.GetComponent<Anvil>(), target.coord);
        anvil.GetComponent<NestedFadeGroup.NestedFadeGroup>().AlphaSelf = 1;
        anvil.UpdateElement(pu.coord);      
        anvil.Init(anvilHP, this);
              
// Move to coord
        yield return user.StartCoroutine(MoveToCoord(pu, target.coord));
    }

    public virtual IEnumerator MoveToCoord(Unit unit, Vector2 moveTo, bool undo = false) {  
        // if (upgrades[UpgradePath.Sludge] < 2) {
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
                unit.UpdateSortOrder(fromTo[current]);
                current = fromTo[current];
                yield return null;
            }        
        //} else {
        //     if (unit.grid.SortOrderFromCoord(moveTo) > unit.grid.SortOrderFromCoord(unit.coord))
        //         unit.UpdateSortOrder(moveTo);
        //     float timer = 0;
        //     float throwDur = animDur * Vector2.Distance(unit.coord, moveTo);
        //     Vector3 startPos = unit.transform.position;
        //     Vector3 endPos = unit.grid.PosFromCoord(moveTo);
        //     float h = 0.25f + Vector2.Distance(unit.coord, moveTo) / 2;
        //     while (timer < throwDur) {
        //         unit.transform.position = Util.SampleParabola(startPos, endPos, h, timer/throwDur);
        //         yield return null;
        //         timer += Time.deltaTime;
        //     }
        //     unit.transform.position = endPos;
        // }

        if (unit is PlayerUnit pu && unit.grid.tiles.Find(sqr => sqr.coord == unit.coord) is TileBulb tb && pu.pManager.overrideEquipment == null) {
            if (!tb.harvested && unit.equipment.Find(e => e is BulbEquipmentData) == null)
                tb.HarvestBulb(pu);
        }
    }
}
