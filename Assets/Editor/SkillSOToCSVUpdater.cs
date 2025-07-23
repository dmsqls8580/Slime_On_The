#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SkillSOToCSVUpdater
{
    private static string skillCsvPath = "Assets/11_Data/CSV/SkillData.csv";

    public static void UpdateCSV(PlayerSkillSO _skill)
    {
        if (!File.Exists(skillCsvPath))
        {
            return;
        }
        var lines = new List<string>(File.ReadAllLines(skillCsvPath));
        if(lines.Count < 1) return;
        
        var header = lines[0].Split(',');
        Dictionary<string, int> colMap = new();

        for (int i = 0; i < header.Length; i++)
        {
            colMap[header[i].Trim()] = i;
        }
        
        for (int i = 1; i < lines.Count; i++)
        {
            var cols=  lines[i].Split(',');
            if(cols.Length < 1)continue;

            if (int.TryParse(cols[colMap["skillIndex"]], out int rowIdx) && rowIdx == _skill.skillIndex)
            {
                cols[colMap["skillName"]] = _skill.skillName;
                cols[colMap["damage"]] = _skill.damage.ToString();
                cols[colMap["speed"]] = _skill.speed.ToString();
                cols[colMap["range"]] = _skill.range.ToString();
                cols[colMap["cooldown"]] = _skill.cooldown.ToString();
                cols[colMap["actionDuration"]] = _skill.actionDuration.ToString();
                cols[colMap["useSlimeGauge"]] = _skill.useSlimeGauge.ToString();
                
                lines[i]= string.Join(",", cols);
                break;
            }
        }
        
        File.WriteAllLines(skillCsvPath, lines);
        Logger.Log($"[SkillCSV] {_skill.skillName} 수정반영 완료");
    }
}

#endif