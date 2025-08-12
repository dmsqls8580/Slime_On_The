using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class SkillSOGenerator : EditorWindow
{
    TextAsset skillCsvFile;

    [MenuItem("Tools/Skill SO Generator")]
    public static void ShowWindow()
    {
        GetWindow<SkillSOGenerator>("Skill SO Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("CSV TO Skill SO Generator", EditorStyles.boldLabel);
        skillCsvFile = (TextAsset)EditorGUILayout.ObjectField("Skill Csv File", skillCsvFile, typeof(TextAsset), false);

        if (GUILayout.Button("Generate SOs") && !skillCsvFile.IsUnityNull())
        {
            GeneratorSkillSOs();
        }
    }

    void GeneratorSkillSOs()
    {
        string[] lines = skillCsvFile.text.Split('\n');
        if (lines.Length < 2) return;

        string outputPath = "Assets/08_ScriptableObjects/Skill/";
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        string[] headers = lines[0].Trim().Split(',');
        Dictionary<string, int> columnIndex = new();
        for (int i = 0; i < headers.Length; i++)
        {
            columnIndex[headers[i].Trim()] = i;
        }

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line))
                continue;
            string[] cols = line.Split(',');
            if (cols.Length < 2||string.IsNullOrWhiteSpace(cols[columnIndex["skillName"]])) 
                continue;
            
            string typeName = GetSafe(cols, columnIndex, "scriptName");
            Logger.Log($"[SkillSOGenerator] typeName from CSV = >{typeName}< (length={typeName.Length})");
            PlayerSkillSO asset = ScriptableObject.CreateInstance(typeName)as PlayerSkillSO;
            if (asset == null)
            {
                Logger.Log($"[SkillSOGenerator] SO 생성 실패. typeName={typeName}");
                // 여기에 타입 리스트 찍기
                foreach(var asm in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach(var t in asm.GetTypes())
                    {
                        if (typeof(PlayerSkillSO).IsAssignableFrom(t))
                            Logger.Log($"가능 타입: {t.FullName}");
                    }
                }
                continue;
            }
            int.TryParse(GetSafe(cols, columnIndex, "skillIndex"), out asset.skillIndex);
            asset.skillName = GetSafe(cols, columnIndex, "skillName");
            string activeTypeStr = GetSafe(cols, columnIndex, "skillActiveType");
            if (!string.IsNullOrEmpty(activeTypeStr))
            {
                if (Enum.TryParse(typeof(SkillActiveType), activeTypeStr, true, out var at))
                    asset.skillActiveType = (SkillActiveType)at;
            }
            float.TryParse(GetSafe(cols, columnIndex, "damage"), out asset.damage);
            float.TryParse(GetSafe(cols, columnIndex, "speed"), out asset.speed);
            float.TryParse(GetSafe(cols, columnIndex, "range"), out asset.range);
            float.TryParse(GetSafe(cols, columnIndex, "cooldown"), out asset.cooldown);
            float.TryParse(GetSafe(cols, columnIndex, "actionDuration"), out asset.actionDuration);
            float.TryParse(GetSafe(cols, columnIndex, "useSlimeGauge"), out asset.useSlimeGauge);

            string assetPath = $"{outputPath}{asset.skillName}.asset";
            AssetDatabase.CreateAsset(asset, assetPath);
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Logger.Log("스킬 SO 생성");
    }

    string GetSafe(string[] _cols, Dictionary<string, int> _columnIndex, string _key)
    {
        return _columnIndex.ContainsKey(_key) &&  _columnIndex[_key] <_cols.Length? _cols[_columnIndex[_key]].Trim() : "";
    }
}