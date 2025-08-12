#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public static class ItemSOToCSVUpdater
{
    static string itemCsvPath = "Assets/11_Data/CSV/ItemData.csv";
    static string recipeCsvPath = "Assets/11_Data/CSV/RecipeData.csv";

    public static void UpdateCSV(ItemSO item)
    {
        UpdateItemCSV(item);
        UpdateRecipeCSV(item);
        AssetDatabase.Refresh();
    }

    static void UpdateItemCSV(ItemSO item)
    {
        if (!File.Exists(itemCsvPath))
        {
            Debug.LogError("[ItemCSV] 파일이 존재하지 않습니다: " + itemCsvPath);
            return;
        }

        var lines = new List<string>(File.ReadAllLines(itemCsvPath));
        if (lines.Count < 1) return;

        var header = lines[0].Split(',');
        Dictionary<string, int> colMap = new();
        for (int i = 0; i < header.Length; i++)
            colMap[header[i].Trim()] = i;

        for (int i = 1; i < lines.Count; i++)
        {
            var cols = lines[i].Split(',');
            if (cols.Length < 1) continue;

            if (int.TryParse(cols[colMap["idx"]], out int rowIdx) && rowIdx == item.idx)
            {
                cols[colMap["itemName"]] = item.itemName;
                cols[colMap["description"]] = item.description.Replace(",", " ");
                cols[colMap["ItemTypes"]] = item.itemTypes.ToString().Replace(", ", "|");
                cols[colMap["stackable"]] = item.stackable ? "y" : "n";
                cols[colMap["maxStack"]] = item.maxStack.ToString();

                if (item.itemTypes.HasFlag(ItemType.Tool) && item.toolData != null)
                {
                    if (colMap.TryGetValue("toolType", out int idx)) cols[idx] = item.toolData.toolType.ToString();
                    if (colMap.TryGetValue("power", out idx)) cols[idx] = item.toolData.power.ToString();
                    if (colMap.TryGetValue("luck", out idx)) cols[idx] = item.toolData.luck.ToString();
                    if (colMap.TryGetValue("durability", out idx)) cols[idx] = item.toolData.durability.ToString();
                    if (colMap.TryGetValue("actSpd", out idx)) cols[idx] = item.toolData.actSpd.ToString();
                }

                if (item.itemTypes.HasFlag(ItemType.Equipable) && item.equipableData != null)
                {
                    if (colMap.TryGetValue("equipableType", out int idx)) cols[idx] = item.equipableData.equipableType.ToString();
                    if (colMap.TryGetValue("maxHealth", out idx)) cols[idx] = item.equipableData.maxHealth.ToString();
                    if (colMap.TryGetValue("atk", out idx)) cols[idx] = item.equipableData.atk.ToString();
                    if (colMap.TryGetValue("def", out idx)) cols[idx] = item.equipableData.def.ToString();
                    if (colMap.TryGetValue("spd", out idx)) cols[idx] = item.equipableData.spd.ToString();
                    if (colMap.TryGetValue("crt", out idx)) cols[idx] = item.equipableData.crt.ToString();
                }

                if (item.itemTypes.HasFlag(ItemType.Eatable) && item.eatableData != null)
                {
                    if (colMap.TryGetValue("recoverHP", out int idx)) cols[idx] = item.eatableData.recoverHP.ToString();
                    if (colMap.TryGetValue("fullness", out idx)) cols[idx] = item.eatableData.fullness.ToString();
                    if (colMap.TryGetValue("slimeGauge", out idx)) cols[idx] = item.eatableData.slimeGauge.ToString();
                    if (colMap.TryGetValue("duration", out idx)) cols[idx] = item.eatableData.duration.ToString();
                    if (colMap.TryGetValue("eatableRottable", out idx)) cols[idx] = item.eatableData.rottenable ? "y" : "n";
                }

                lines[i] = string.Join(",", cols);
                break;
            }
        }

        File.WriteAllLines(itemCsvPath, lines);
        Debug.Log($"[ItemCSV] {item.itemName} 수정 반영 완료");
    }

    static void UpdateRecipeCSV(ItemSO item)
    {
        if (item.recipe == null) return;

        var lines = new List<string>();
        if (File.Exists(recipeCsvPath))
        {
            lines.AddRange(File.ReadAllLines(recipeCsvPath));
        }
        else
        {
            lines.Add("resultIdx,ingredientIdx,amount");
        }

        string header = lines[0];
        List<string> newLines = new() { header };

        // 기존 줄 중 현재 item.idx에 해당하지 않는 줄만 유지
        // 삽입 위치 저장
        int insertIndex = lines.Count;
        for (int i = 1; i < lines.Count; i++)
        {
            var line = lines[i];
            var cols = line.Split(',');
            if (cols.Length < 3) continue;

            if (int.TryParse(cols[0].Trim(), out int rowIdx) 
                            && rowIdx == item.idx)
            {
                if (insertIndex == lines.Count)
                    insertIndex = newLines.Count;
                continue;
            }

            newLines.Add(line);
        }

        // 새 레시피 줄 생성
        List<string> recipeLines = new();
        foreach (var ri in item.recipe)
        {
            if (ri.item == null) continue;
            recipeLines.Add($"{item.idx},{ri.item.idx},{ri.amount}");
        }

        // 기존 위치에 삽입
        newLines.InsertRange(insertIndex, recipeLines);

        File.WriteAllLines(recipeCsvPath, newLines);
        Debug.Log($"[RecipeCSV] {item.itemName} 레시피 수정 반영 완료");
    }
}
#endif