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
            GUILayout.Label("SPECIAL: " + arg.upgrades[SlagEquipmentData.UpgradePath.Scab]);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Upgrade"))
                arg.UpgradeEquipment(SlagEquipmentData.UpgradePath.Scab);
            if (GUILayout.Button("Reset")) {
                arg.upgrades[SlagEquipmentData.UpgradePath.Scab] = -1;
                arg.UpgradeEquipment(SlagEquipmentData.UpgradePath.Scab);
            }
            GUILayout.EndHorizontal();
            GUILayout.Label("POWER: " + arg.upgrades[SlagEquipmentData.UpgradePath.Shunt]);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Upgrade"))
                arg.UpgradeEquipment( SlagEquipmentData.UpgradePath.Shunt);
            if (GUILayout.Button("Reset")) {
                arg.upgrades[SlagEquipmentData.UpgradePath.Shunt] = -1;
                arg.UpgradeEquipment(SlagEquipmentData.UpgradePath.Shunt);
            }
            GUILayout.EndHorizontal();
            GUILayout.Label("UNIT: " + arg.upgrades[SlagEquipmentData.UpgradePath.Sludge]);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Upgrade"))
                arg.UpgradeEquipment(SlagEquipmentData.UpgradePath.Sludge);
            if (GUILayout.Button("Reset")) {
                arg.upgrades[SlagEquipmentData.UpgradePath.Sludge] = -1;
                arg.UpgradeEquipment(SlagEquipmentData.UpgradePath.Sludge);
            }
            GUILayout.EndHorizontal();
            
        }
        EquipmentPanel = EditorGUILayout.Foldout(EquipmentPanel, "EQUIPMENT VARS");
        if (EquipmentPanel) {
            base.OnInspectorGUI();
        }

    }

}
