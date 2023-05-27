using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentPickup : GroundElement
{
    enum Pickup { Hammer, Consumable };
    [SerializeField] Pickup pickup;
    public List<EquipmentData> equipment;
    [SerializeField] List<EquipmentData> consumableTable;
    [SerializeField] SpriteRenderer equipIcon;

    protected override void Start() {
        base.Start();
        if (pickup is Pickup.Consumable) {
            int rndI = Random.Range(0, consumableTable.Count-1);
            equipment[0] = consumableTable[rndI];
            equipIcon.sprite = equipment[0].icon;
        }
    }
    public override void OnSharedSpace(GridElement sharedWith)
    {
        base.OnSharedSpace(sharedWith);
        if (sharedWith is PlayerUnit pu) {
            switch (pickup) {
                default: break;
// Spawn new hammer and assign it to equipment data
                case Pickup.Hammer:
                    PlayerManager manager = (PlayerManager)pu.manager;
                    List<Hammer> hData = new List<Hammer>();
                    foreach (EquipmentData equip in equipment)
                        hData.Add((Hammer)equip);
                    manager.SpawnHammer(pu, hData);
                    StartCoroutine(DestroyElement());
                break;
                case Pickup.Consumable:
                    for (int i = pu.equipment.Count - 1; i >= 0; i--) {
                        if (pu.equipment[i] is ConsumableEquipmentData c)
                            pu.equipment.Remove(c);
                    }
                    pu.equipment.Insert(1, equipment[0]);
                    pu.equipment[1].EquipEquipment(pu);
        
                    pu.ui.UpdateEquipmentButtons();
                    StartCoroutine(DestroyElement());
                break;
            }
            
        }
    }

}
