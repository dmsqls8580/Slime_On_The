using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class ItemSOGenerator : EditorWindow
{
    TextAsset itemCsvFile;
    TextAsset recipeCsvFile;

    // idx를 키로, 재료들의 리스트를 값으로 하는 맵
    Dictionary<string, List<(string ingredientIdx, int amount)>> recipeMap;

    [MenuItem("Tools/Item SO Generator")]
    public static void ShowWindow()
    {
        GetWindow<ItemSOGenerator>("Item SO Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("CSV to ScriptableObject Generator", EditorStyles.boldLabel);
        itemCsvFile = (TextAsset)EditorGUILayout.ObjectField("Item CSV File", itemCsvFile, typeof(TextAsset), false);
        recipeCsvFile = (TextAsset)EditorGUILayout.ObjectField("Recipe CSV File", recipeCsvFile, typeof(TextAsset), false);

        if (GUILayout.Button("Generate SOs") && itemCsvFile != null)
        {
            LoadRecipeData();
            GenerateItemSOs();
        }
    }

    void LoadRecipeData()
    {
        recipeMap = new();

        if (recipeCsvFile == null)
        {
            Debug.LogWarning("Recipe CSV 파일이 지정되지 않았습니다.");
            return;
        }

        string[] lines = recipeCsvFile.text.Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] cols = line.Split(',');
            if (cols.Length < 3) continue;

            string resultIdx = cols[0].Trim();
            string ingredientIdx = cols[1].Trim();
            int.TryParse(cols[2].Trim(), out int amount);

            if (!recipeMap.ContainsKey(resultIdx))
                recipeMap[resultIdx] = new();

            recipeMap[resultIdx].Add((ingredientIdx, amount));
        }
    }

    void GenerateItemSOs()
    {
        string[] lines = itemCsvFile.text.Split('\n');
        if (lines.Length < 2) return;

        string outputPath = "Assets/08_ScriptableObjects/Items/";
        if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

        // 헤더 매핑
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
            if (cols.Length < 5 || string.IsNullOrWhiteSpace(cols[columnIndex["itemName"]])) continue;

            ItemSO asset = ScriptableObject.CreateInstance<ItemSO>();

            asset.idx = GetSafe(cols, columnIndex, "idx");
            asset.itemName = GetSafe(cols, columnIndex, "itemName");
            asset.description = GetSafe(cols, columnIndex, "description");
            asset.itemTypes = ParseItemTypes(GetSafe(cols, columnIndex, "ItemTypes"));
            asset.stackable = GetSafe(cols, columnIndex, "stackable").ToLower() == "y";
            int.TryParse(GetSafe(cols, columnIndex, "maxStack"), out asset.maxStack);

            // === Tool ===
            if (asset.itemTypes.HasFlag(ItemType.Tool))
            {
                asset.toolData = new ToolData();
                asset.toolData.toolType = ParseToolType(GetSafe(cols, columnIndex, "toolType"));
                TryParseFloat(cols, columnIndex, "getAmount", out asset.toolData.getAmount);
                TryParseFloat(cols, columnIndex, "luck", out asset.toolData.luck);
                TryParseFloat(cols, columnIndex, "durability", out asset.toolData.durability);
                TryParseFloat(cols, columnIndex, "atkSpd", out asset.toolData.atkSpd);
            }

            // === Equipable ===
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

            // === Eatable ===
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

            // === Recipe 연결 ===
            if (recipeMap.TryGetValue(asset.idx, out var recipes))
            {
                asset.recipe = new List<RecipeIngredient>();

                foreach (var (ingredientIdx, amount) in recipes)
                {
                    string[] guids = AssetDatabase.FindAssets("t:ItemSO", new[] { "Assets/08_ScriptableObjects/Items" });
                    bool found = false;

                    foreach (string guid in guids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        ItemSO so = AssetDatabase.LoadAssetAtPath<ItemSO>(path);
                        if (so != null && so.idx == ingredientIdx)
                        {
                            asset.recipe.Add(new RecipeIngredient { item = so, amount = amount });
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        Debug.LogWarning($"[Recipe] 재료 idx={ingredientIdx} 를 가진 ItemSO를 찾지 못했습니다.");
                    }
                }
            }

            // === 저장 ===
            string assetPath = $"{outputPath}{asset.itemName}.asset";
            AssetDatabase.CreateAsset(asset, assetPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("아이템 SO 자동 생성 완료!");
    }

    // enum 다중 파싱 (Material|Eatable 등)
    ItemType ParseItemTypes(string input)
    {
        ItemType result = ItemType.None;
        if (string.IsNullOrWhiteSpace(input)) return result;
        string[] tokens = input.Split('|');
        foreach (string token in tokens)
        {
            if (System.Enum.TryParse<ItemType>(token.Trim(), true, out var parsed))
                result |= parsed;
            else
                Debug.LogWarning($"[ItemType 파싱 실패] '{token}'");
        }
        return result;
    }

    ToolType ParseToolType(string input)
    {
        return System.Enum.TryParse(input, true, out ToolType result) ? result : ToolType.None;
    }

    EquipableType ParseEquipableType(string input)
    {
        return System.Enum.TryParse(input, true, out EquipableType result) ? result : EquipableType.Armor;
    }

    string GetSafe(string[] cols, Dictionary<string, int> map, string key)
    {
        return map.ContainsKey(key) && map[key] < cols.Length ? cols[map[key]].Trim() : "";
    }

    bool TryParseFloat(string[] cols, Dictionary<string, int> map, string key, out float value)
    {
        value = 0f;
        return map.ContainsKey(key) && map[key] < cols.Length && float.TryParse(cols[map[key]], out value);
    }
}
