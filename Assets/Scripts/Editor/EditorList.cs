    using UnityEngine;
    using UnityEditor;
    using System;
    using System.Collections;
     
    public static class EditorList {
     
        [Flags]
        public enum EditorListOption {
            None = 0,
            ListSize = 1,
            ListLabel = 2,
            Default = ListSize | ListLabel
        }
     
        public static void Show (SerializedProperty list, EditorListOption options = EditorListOption.Default) {
            bool showListLabel = (options & EditorListOption.ListLabel) != 0, showListSize = (options & EditorListOption.ListSize) != 0;
         
            if (showListLabel) {
                EditorGUILayout.PropertyField(list);
                EditorGUI.indentLevel += 1;
            }
     
            if (!showListLabel || list.isExpanded) {
                if (showListSize)
                    EditorGUILayout.PropertyField(list.FindPropertyRelative("Array.size"));
     
                for (int i = 0; i < list.arraySize; i++)
                    EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i));
            }
     
            if (showListLabel)
                EditorGUI.indentLevel -= 1;
        }
    }
