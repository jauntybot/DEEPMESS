using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentPickup : GroundElement
{
    enum Pickup { Hammer, Consumable };
    [SerializeField] Pickup pickup;
    public EquipmentData equipment;
    [SerializeField] List<EquipmentData> consumableTable;
    [SerializeField] SpriteRenderer equipIcon;

    protected override void Start() {
        base.Start();
        if (pickup is Pickup.Consumable) {
            int rndI = Random.Range(0, consumableTable.Count-1);
            equipment = consumableTable[rndI];
            equipIcon.sprite = equipment.icon;
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
                    manager.SpawnHammer(pu, (HammerData)equipment);
                    StartCoroutine(DestroyElement());
                break;
                case Pickup.Consumable:
                    for (int i = pu.equipment.Count - 1; i >= 0; i--) {
                        if (pu.equipment[i] is ConsumableEquipmentData c)
                            pu.equipment.Remove(c);
                    }
                    pu.equipment.Insert(1, equipment);
                    pu.equipment[2].EquipEquipment(pu);
        
                    pu.ui.UpdateEquipmentButtons();
                    StartCoroutine(DestroyElement());
                break;
            }
            
        }
    }

}
