#if UNITY_EDITOR

using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class SkillSOPostprocessor : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        foreach (string path in importedAssets)
        {
            if (path.EndsWith(".asset") && path.Contains("08_ScriptableObjects/Skill"))
            {
                PlayerSkillSO skill = AssetDatabase.LoadAssetAtPath<PlayerSkillSO>(path);
                if (!skill.IsUnityNull())
                {
                    SkillSOToCSVUpdater.UpdateCSV(skill);
                }
            }
        }
    }
}
#endif