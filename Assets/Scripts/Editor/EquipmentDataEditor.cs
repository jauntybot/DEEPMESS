using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EquipmentData))]
[CanEditMultipleObjects]
public class EquipmentDataEditor : Editor {
    
    EquipmentData arg;
    
    void OnEnable() {
        arg = target as EquipmentData;
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        
    }

}
