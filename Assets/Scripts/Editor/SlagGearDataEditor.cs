using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Codice.CM.Client.Differences.Graphic;
using TMPro;

[CustomEditor(typeof(SlagGearData), true)]
[CanEditMultipleObjects]
public class SlagGearDataEditor : Editor {
    
    protected static bool UpgradePanel, EquipmentPanel;
    protected static SlagGearData arg;
    int upgradeIndex = 0;
    string[] options;
    Dictionary<string, GearUpgrade> upgrades;
    GearUpgrade selectedGear;
    int slotIndex;

    void OnEnable() {
        arg = target as SlagGearData;
        options = new string[arg.upgrades.Count];
        upgrades = new();
        int i = 0;
        foreach (GearUpgrade upgrade in arg.upgrades) {
            options[i] = upgrade.name;
            upgrades.Add(options[i], upgrade);
            i++;
        }
    }

    public override void OnInspectorGUI() {

        UpgradePanel = EditorGUILayout.Foldout(UpgradePanel, "UPGRADES");
        if (UpgradePanel) {
            EditorGUI.BeginChangeCheck();
            upgradeIndex = EditorGUILayout.Popup("Upgrade", upgradeIndex, options);
            if (EditorGUI.EndChangeCheck())
            {
                selectedGear = upgrades[options[upgradeIndex]];
                Debug.Log(options[upgradeIndex]);
            }
            GUILayout.BeginHorizontal();
            for (int x = 0; x <= 2; x++) {
                string slot;
                if (arg.slottedUpgrades[x] != null) {
                    GearUpgrade slotted = arg.slottedUpgrades[x];
                    slot = slotted.name;
                    if (x == slotIndex) slot += "\n" + "[SELECTED]";
                }
                else if (x == slotIndex) slot = "[EMPTY]" + "\n" + "[SELECTED]";
                else slot = "[EMPTY]";

                GUILayout.BeginVertical();
                Rect r = new(x * (EditorGUIUtility.currentViewWidth-25)/3, 0, (EditorGUIUtility.currentViewWidth-25)/3, (EditorGUIUtility.currentViewWidth-25)/3);
                
                GUIStyle style = new(GUI.skin.button);
                style.wordWrap = true;
                if (GUILayout.Button(slot, style, GUILayout.Width((EditorGUIUtility.currentViewWidth-25)/3), GUILayout.Height((EditorGUIUtility.currentViewWidth-25)/3))) {
                    slotIndex = x;

                }

                GUILayout.EndVertical();
            }

            GUILayout.EndHorizontal();
            if (GUILayout.Button("Slot Upgrade")) {
                arg.UpgradeGear(selectedGear, slotIndex);
            }
            if (GUILayout.Button("Clear Slot")) {
                arg.UpgradeGear(null, slotIndex);
            }
        }
        EquipmentPanel = EditorGUILayout.Foldout(EquipmentPanel, "EQUIPMENT VARS");
        if (EquipmentPanel) {
            base.OnInspectorGUI();
        }

    }

}
