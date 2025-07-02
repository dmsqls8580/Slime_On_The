#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public static class ItemSOToCSVUpdater
{
    static string csvPath = "Assets/11_Data/CSV/ItemData_fix.csv"; // ��� ���� ����

    public static void UpdateCSV(ItemSO item)
    {
        if (!File.Exists(csvPath))
        {
            Debug.LogError("[CSV] ������ �������� �ʽ��ϴ�: " + csvPath);
            return;
        }

        var lines = new List<string>(File.ReadAllLines(csvPath));

        if (lines.Count < 1) return;

        // ������� �÷� �ε��� ����
        var header = lines[0].Split(',');
        Dictionary<string, int> colMap = new();
        for (int i = 0; i < header.Length; i++)
            colMap[header[i].Trim()] = i;

        // �ش� item.idx�� �ش��ϴ� �� ã��
        for (int i = 1; i < lines.Count; i++)
        {
            var cols = lines[i].Split(',');
            if (cols.Length < 1) continue;

            if (cols[colMap["idx"]].Trim() == item.idx)
            {
                // ���� �����͸� ����
                cols[colMap["itemName"]] = item.itemName;
                cols[colMap["description"]] = item.description.Replace(",", " ");
                cols[colMap["ItemTypes"]] = item.itemTypes.ToString().Replace(", ", "|");
                cols[colMap["stackable"]] = item.stackable ? "y" : "n";
                cols[colMap["maxStack"]] = item.maxStack.ToString();

                // �ʿ� �� ToolData, EquipableData � �߰� �ݿ�
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

                // ������ line���� �ٽ� ��ġ��
                lines[i] = string.Join(",", cols);
                break;
            }
        }

        // �ٽ� ����
        File.WriteAllLines(csvPath, lines);
        AssetDatabase.Refresh();
        Debug.Log($"[CSV] {item.itemName} ���� �ݿ� �Ϸ�");
    }
}
#endif
