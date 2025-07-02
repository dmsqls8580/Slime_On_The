#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class ItemSOPostprocessor : AssetPostprocessor
{
    static void OnPostprocessAllAssets(
        string[] importedAssets, string[] deletedAssets,
        string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (var path in importedAssets)
        {
            if (path.EndsWith(".asset") && path.Contains("/08_ScriptableObjects/"))
            {
                ItemSO item = AssetDatabase.LoadAssetAtPath<ItemSO>(path);
                if (item != null)
                {
                    ItemSOToCSVUpdater.UpdateCSV(item);
                }
            }
        }
    }
}
#endif
