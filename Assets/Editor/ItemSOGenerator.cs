using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class ItemSOGenerator : EditorWindow
{
    TextAsset csvFile;

    [MenuItem("Tools/Item SO Generator")]
    public static void ShowWindow()
    {
        GetWindow<ItemSOGenerator>("Item SO Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("CSV to ScriptableObject Generator", EditorStyles.boldLabel);
        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile, typeof(TextAsset), false);

        if (GUILayout.Button("Generate SOs") && csvFile != null)
        {
            GenerateItemSOs();
        }
    }

    void GenerateItemSOs()
    {
        string[] lines = csvFile.text.Split('\n');
        if (lines.Length < 2) return;

        string outputPath = "Assets/08_ScriptableObjects/Items/";
        if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

        // 헤더 읽기
        string[] headers = lines[0].Trim().Split(',');
        Dictionary<string, int> columnIndex = new();
        for (int h = 0; h < headers.Length; h++)
        {
            columnIndex[headers[h].Trim()] = h;
        }

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] cols = line.Split(',');

            // 최소 데이터 검증
            if (cols.Length < 5 || string.IsNullOrWhiteSpace(cols[columnIndex["itemName"]])) continue;

            ItemSO asset = ScriptableObject.CreateInstance<ItemSO>();

            // 기본 정보
            asset.idx = cols[columnIndex["idx"]];
            asset.itemName = cols[columnIndex["itemName"]];
            asset.description = cols[columnIndex["description"]];
            asset.itemTypes = ParseItemTypes(cols[columnIndex["ItemTypes"]]);
            asset.stackable = cols[columnIndex["stackable"]].Trim().ToLower() == "y";
            int.TryParse(cols[columnIndex["maxStack"]], out asset.maxStack);

            // === ToolData ===
            if (asset.itemTypes.HasFlag(ItemType.Tool))
            {
                asset.toolData = new ToolData();
                asset.toolData.toolType = ParseToolType(GetSafe(cols, columnIndex, "toolType"));
                TryParseFloat(cols, columnIndex, "getAmount", out asset.toolData.getAmount);
                TryParseFloat(cols, columnIndex, "luck", out asset.toolData.luck);
                TryParseFloat(cols, columnIndex, "durability", out asset.toolData.durability);
                TryParseFloat(cols, columnIndex, "atkSpd", out asset.toolData.atkSpd);
            }

            // === EquipableData ===
            if (asset.itemTypes.HasFlag(ItemType.Equipable))
            {
                asset.equipableData = new EquipableData();
                asset.equipableData.equipableType = ParseEquipableType(GetSafe(cols, columnIndex, "equipableType"));
                TryParseFloat(cols, columnIndex, "maxHealth", out asset.equipableData.maxHealth);
                TryParseFloat(cols, columnIndex, "atk", out asset.equipableData.atk);
                TryParseFloat(cols, columnIndex, "def", out asset.equipableData.def);
                TryParseFloat(cols, columnIndex, "spd", out asset.equipableData.spd);
                TryParseFloat(cols, columnIndex, "crt", out asset.equipableData.crt);
            }

            // === EatableData ===
            if (asset.itemTypes.HasFlag(ItemType.Eatable))
            {
                asset.eatableData = new EatableData();
                TryParseFloat(cols, columnIndex, "recoverHP", out asset.eatableData.recoverHP);
                TryParseFloat(cols, columnIndex, "fullness", out asset.eatableData.fullness);
                TryParseFloat(cols, columnIndex, "slimeGauge", out asset.eatableData.slimeGauge);
                TryParseFloat(cols, columnIndex, "duration", out asset.eatableData.duration);
                asset.eatableData.permanent = asset.eatableData.duration == 0;
                asset.eatableData.rottenable = GetSafe(cols, columnIndex, "eatableRottable").ToLower() == "y";
            }

            string assetPath = $"{outputPath}{asset.itemName}.asset";
            AssetDatabase.CreateAsset(asset, assetPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("아이템 SO 자동 생성 완료!");
    }

    // Helper: Enum 파싱 (복수 타입 지원)
    ItemType ParseItemTypes(string input)
    {
        ItemType result = ItemType.None;
        if (string.IsNullOrWhiteSpace(input)) return result;

        string[] tokens = input.Split('|');
        foreach (string token in tokens)
        {
            if (System.Enum.TryParse<ItemType>(token.Trim(), true, out var parsed))
            {
                result |= parsed;
            }
            else
            {
                Debug.LogWarning($"[ItemType 파싱 실패] '{token}'");
            }
        }
        return result;
    }

    // Helper: 안전하게 컬럼 가져오기
    string GetSafe(string[] cols, Dictionary<string, int> map, string key)
    {
        return map.ContainsKey(key) && map[key] < cols.Length ? cols[map[key]].Trim() : "";
    }

    // Helper: float 파싱
    bool TryParseFloat(string[] cols, Dictionary<string, int> map, string key, out float value)
    {
        value = 0f;
        if (!map.ContainsKey(key) || map[key] >= cols.Length) return false;
        return float.TryParse(cols[map[key]], out value);
    }

    ToolType ParseToolType(string input)
    {
        if (System.Enum.TryParse(input, true, out ToolType type))
            return type;
        Debug.LogWarning($"[ToolType 파싱 실패] '{input}'");
        return ToolType.None;
    }

    EquipableType ParseEquipableType(string input)
    {
        if (System.Enum.TryParse(input, true, out EquipableType type))
            return type;
        Debug.LogWarning($"[EquipableType 파싱 실패] '{input}'");
        return EquipableType.Armor;
    }
}
