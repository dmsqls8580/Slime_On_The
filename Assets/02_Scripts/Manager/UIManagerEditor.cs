#if UNITY_EDITOR
using _02_Scripts.Manager;
using UnityEditor;
using UnityEngine;
using System;

[CustomEditor(typeof(UIManager))]
public class UIManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        UIManager manager = (UIManager)target;
        
        if (Application.isPlaying)
            Repaint();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField(" [UIDict]", EditorStyles.boldLabel);

        if (manager.DebugUIDict != null && manager.DebugUIDict.Count > 0)
        {
            foreach (var kvp in manager.DebugUIDict)
            {
                EditorGUILayout.LabelField($"Key: {kvp.Key.Name}, Value: {kvp.Value?.name ?? "null"}");
            }
        }
        else
        {
            EditorGUILayout.LabelField("UIDict is empty.");
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField(" [Opened UI List]", EditorStyles.boldLabel);

        if (manager.DebugOpenedUIList != null && manager.DebugOpenedUIList.Count > 0)
        {
            foreach (var ui in manager.DebugOpenedUIList)
            {
                EditorGUILayout.LabelField(ui?.name ?? "null");
            }
        }
        else
        {
            EditorGUILayout.LabelField("No opened UIs.");
        }
    }
}
#endif