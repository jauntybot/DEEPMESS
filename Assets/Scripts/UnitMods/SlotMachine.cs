using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotMachine : MonoBehaviour
{
    LoadoutManager manager;
    [SerializeField] public List<EquipmentData> equipmentTable;
    public EquipmentData selectedReward;
    UnitLoadoutUI selectedUI;
    [SerializeField] Transform slotsContainer, orText;
    List<SlotMachineSlot> slots;
    List<int> rolledEquipment = new();
    [SerializeField] Button spinButton, backButton, healButton;
    [SerializeField] SFX slotSpin, healSFX;
    AudioSource audioSource;


    public void Initialize(List<EquipmentData> table) {
        
        // foreach (UnitLoadoutUI ui in ScenarioManager.instance.player.loadout.unitLoadoutUIs) {
        //     Button b = ui.slotsLoadoutButton.GetComponent<Button>();
        //     b.onClick.RemoveAllListeners();
        //     b.onClick.AddListener(ui.SwapEquipmentFromSlots);
        //     b.onClick.AddListener(ClaimSelectedReward);
        //     b.interactable = false;
        // }

        audioSource = GetComponent<AudioSource>();

        spinButton.interactable = true;
        slots = new List<SlotMachineSlot>();
        foreach(Transform child in slotsContainer) {
            slots.Add(child.GetComponent<SlotMachineSlot>());
        }
        foreach(SlotMachineSlot slot in slots) {
            slot.Initialize(this, table, Random.Range(0, equipmentTable.Count - 1));
        }
        spinButton.gameObject.SetActive(true);
        spinButton.interactable = true;
        backButton.gameObject.SetActive(false);
        
        
        foreach (SlotMachineSlot slot in slots) 
            slot.gameObject.SetActive(true);

        HealAllUnits();
    }

    public void SpinSlots() {
        PlaySound(slotSpin);
        spinButton.interactable = false;

        rolledEquipment = new List<int>();
        for (int i = 0; i <= 2; i++) {
            int newEquip = Random.Range(0, equipmentTable.Count - 1);
            while (rolledEquipment.Contains(newEquip)) {
                newEquip = Random.Range(0, equipmentTable.Count - 1);
            }
            rolledEquipment.Insert(i, newEquip);
            slots[i].Spin(rolledEquipment[i]);
        }
        spinButton.gameObject.SetActive(false);

    }

    public virtual void PlaySound(SFX sfx = null) {
        if (sfx) {
            if (sfx.outputMixerGroup) 
                audioSource.outputAudioMixerGroup = sfx.outputMixerGroup;   

            audioSource.PlayOneShot(sfx.Get());
        }
    }

    public void SelectReward(int index, SlotMachineSlot selected) {
        selectedReward = equipmentTable[index];
        foreach (SlotMachineSlot slot in slots) 
            slot.gameObject.SetActive(slot == selected);
        backButton.gameObject.SetActive(true);
        spinButton.gameObject.SetActive(false);
        orText.gameObject.SetActive(true);
        orText.GetComponent<TMPro.TMP_Text>().text = "SELECT UNIT TO SWAP EQUIPMENT";
        // foreach (UnitLoadoutUI ui in ScenarioManager.instance.player.loadout.unitLoadoutUIs) {
        //     Button b = ui.slotsLoadoutButton.GetComponent<Button>();
        //     b.interactable = true;
        // }
    }

    public void DeselectReward() {
        selectedReward = null;
        foreach (SlotMachineSlot slot in slots) 
            slot.gameObject.SetActive(true);
        backButton.gameObject.SetActive(false);
        orText.gameObject.SetActive(true);
        orText.GetComponent<TMPro.TMP_Text>().text = "SELECT AN EQUIPMENT";
        // foreach (UnitLoadoutUI ui in ScenarioManager.instance.player.loadout.unitLoadoutUIs) {
        //     Button b = ui.slotsLoadoutButton.GetComponent<Button>();
        //     b.interactable = false;
        // }
    }

    void ClaimSelectedReward() {
        selectedReward = null;
        foreach (SlotMachineSlot slot in slots)
            slot.GetComponent<Button>().interactable = false;
        Invoke("SkipSlots", 1f);
    }
    public void HealAllUnits() {
        PlaySound(healSFX);
        foreach (Unit u in ScenarioManager.instance.player.units) {
            if (u is Nail) 
                u.StartCoroutine(u.TakeDamage(-3));
            else if (u is PlayerUnit pu) {
                if (pu.conditions.Contains(Unit.Status.Disabled))
                    pu.Stabilize();
                else
                    pu.StartCoroutine(u.TakeDamage(-1, GridElement.DamageType.Slots));
            }
        }
    }

    public void SkipSlots() {
        ScenarioManager.instance.currentTurn = ScenarioManager.Turn.Descent;
        gameObject.SetActive(false);
    }


}
