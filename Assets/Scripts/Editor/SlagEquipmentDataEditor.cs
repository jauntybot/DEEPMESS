using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SlagEquipmentData), true)]
[CanEditMultipleObjects]
public class SlagEquipmentDataEditor : Editor {
    
    protected static bool UpgradePanel, EquipmentPanel;
    protected static SlagEquipmentData arg;
    
    void OnEnable() {
        arg = target as SlagEquipmentData;
    }

    public override void OnInspectorGUI() {

        UpgradePanel = EditorGUILayout.Foldout(UpgradePanel, "UPGRADES");
        if (UpgradePanel) { 
            GUILayout.Label("SPECIAL: " + arg.upgrades[SlagEquipmentData.UpgradePath.Special]);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Upgrade"))
                arg.UpgradeEquipment(arg.slag, SlagEquipmentData.UpgradePath.Special);
            if (GUILayout.Button("Reset")) {
                arg.upgrades[SlagEquipmentData.UpgradePath.Special] = -1;
                arg.UpgradeEquipment(arg.slag, SlagEquipmentData.UpgradePath.Special);
            }
            GUILayout.EndHorizontal();
            GUILayout.Label("POWER: " + arg.upgrades[SlagEquipmentData.UpgradePath.Power]);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Upgrade"))
                arg.UpgradeEquipment(arg.slag, SlagEquipmentData.UpgradePath.Power);
            if (GUILayout.Button("Reset")) {
                arg.upgrades[SlagEquipmentData.UpgradePath.Power] = -1;
                arg.UpgradeEquipment(arg.slag, SlagEquipmentData.UpgradePath.Power);
            }
            GUILayout.EndHorizontal();
            GUILayout.Label("UNIT: " + arg.upgrades[SlagEquipmentData.UpgradePath.Unit]);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Upgrade"))
                arg.UpgradeEquipment(arg.slag, SlagEquipmentData.UpgradePath.Unit);
            if (GUILayout.Button("Reset")) {
                arg.upgrades[SlagEquipmentData.UpgradePath.Unit] = -1;
                arg.UpgradeEquipment(arg.slag, SlagEquipmentData.UpgradePath.Unit);
            }
            GUILayout.EndHorizontal();
            
        }
        EquipmentPanel = EditorGUILayout.Foldout(EquipmentPanel, "EQUIPMENT VARS");
        if (EquipmentPanel) {
            base.OnInspectorGUI();
        }

    }

}
