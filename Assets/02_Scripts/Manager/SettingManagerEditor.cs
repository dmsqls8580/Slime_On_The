#if UNITY_EDITOR
using _02_Scripts.Manager;
using UnityEditor;
using UnityEngine;
using System;

[CustomEditor(typeof(SettingManager))]
public class SettingManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        SettingManager manager = (SettingManager)target;

        if (Application.isPlaying)
            Repaint();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField(" [UIDict]", EditorStyles.boldLabel);

        if (manager.DebugUIDict != null && manager.DebugUIDict.Count > 0)
        {
            foreach (var kvp in manager.DebugUIDict)
            {
                EditorGUILayout.LabelField($"Key: {kvp.Key}, Value: {kvp.Value?.name ?? "null"}");
            }
        }
        else
        {
            EditorGUILayout.LabelField("UIDict is empty.");
        }
    }
}
#endif