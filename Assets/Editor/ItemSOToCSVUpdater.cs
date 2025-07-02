#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public static class ItemSOToCSVUpdater
{
    static string csvPath = "Assets/11_Data/CSV/ItemData_fix.csv"; // 경로 수정 가능

    public static void UpdateCSV(ItemSO item)
    {
        if (!File.Exists(csvPath))
        {
            Debug.LogError("[CSV] 파일이 존재하지 않습니다: " + csvPath);
            return;
        }

        var lines = new List<string>(File.ReadAllLines(csvPath));

        if (lines.Count < 1) return;

        // 헤더에서 컬럼 인덱스 매핑
        var header = lines[0].Split(',');
        Dictionary<string, int> colMap = new();
        for (int i = 0; i < header.Length; i++)
            colMap[header[i].Trim()] = i;

        // 해당 item.idx에 해당하는 행 찾기
        for (int i = 1; i < lines.Count; i++)
        {
            var cols = lines[i].Split(',');
            if (cols.Length < 1) continue;

            if (cols[colMap["idx"]].Trim() == item.idx)
            {
                // 기존 데이터를 수정
                cols[colMap["itemName"]] = item.itemName;
                cols[colMap["description"]] = item.description.Replace(",", " ");
                cols[colMap["ItemTypes"]] = item.itemTypes.ToString().Replace(", ", "|");
                cols[colMap["stackable"]] = item.stackable ? "y" : "n";
                cols[colMap["maxStack"]] = item.maxStack.ToString();

                // 필요 시 ToolData, EquipableData 등도 추가 반영
                if (item.itemTypes.HasFlag(ItemType.Tool) && item.toolData != null)
                {
                    if (colMap.TryGetValue("toolType", out int idx)) cols[idx] = item.toolData.toolType.ToString();
                    if (colMap.TryGetValue("getAmount", out idx)) cols[idx] = item.toolData.getAmount.ToString();
                    if (colMap.TryGetValue("luck", out idx)) cols[idx] = item.toolData.luck.ToString();
                    if (colMap.TryGetValue("durability", out idx)) cols[idx] = item.toolData.durability.ToString();
                    if (colMap.TryGetValue("atkSpd", out idx)) cols[idx] = item.toolData.atkSpd.ToString();
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

                // 수정한 line으로 다시 합치기
                lines[i] = string.Join(",", cols);
                break;
            }
        }

        // 다시 쓰기
        File.WriteAllLines(csvPath, lines);
        AssetDatabase.Refresh();
        Debug.Log($"[CSV] {item.itemName} 수정 반영 완료");
    }
}
#endif
