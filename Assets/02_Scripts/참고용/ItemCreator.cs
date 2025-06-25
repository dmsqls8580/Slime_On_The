using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


// 아이템 데이터 ( 참고용 )
public enum ItemType
{
    Consumable,
    Equipable
}

public enum StatType
{
    // Consumable
    Healing,
    SpeedUp,
    
    // Equipable
    Health,
    AttackPower,
    Speed
}

[System.Serializable]
public class StatEntry
{
    public StatType type;
    public int value;
}

[System.Serializable]
public class ItemData : ScriptableObject
{
    public int ItemID;
    public string ItemName;
    public string ItemDescription;
    public Sprite ItemIcon;
    public ItemType ItemType;
    public List<StatEntry> stats;
}



// 아이템 에디터 (참고용)
public class ItemCreator : EditorWindow
{
    private int itemID;
    private string itemName = "New Item";
    private string itemDescription = "New Description";
    private Sprite itemIcon;
    private ItemType itemType = ItemType.Consumable;
    private List<StatEntry> statEntries = new List<StatEntry>();

    [MenuItem("window/Item Creator")]
    private static void ShowWindow()
    {
        GetWindow<ItemCreator>("아이템 생성");
    }

    private void OnGUI()
    {
        // 아이템 ID 입력
        itemID  = EditorGUILayout.IntField("ID", itemID);
        
        // 아이템 이름 입력
        itemName = EditorGUILayout.TextField("아이템 이름", itemName);
        
        // 아이템 설명
        itemDescription = EditorGUILayout.TextField("아이템 설명", itemDescription);
        
        // 아이템 이미지
        itemIcon = EditorGUILayout.ObjectField("아이템 이미지", itemIcon, typeof(Sprite), true) as Sprite;
        
        // 아이템 타입 선택
        itemType = (ItemType)EditorGUILayout.EnumPopup("아이템 타입", itemType);

        // 스탯 리스트를 보여주는 UI
        EditorGUILayout.LabelField("아이템 스탯");

        // 스탯 타입, 밸류를 리스트로 관리해 추가, 삭제가 용이
        for (int i = 0; i < statEntries.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            statEntries[i].type = (StatType)EditorGUILayout.EnumPopup("스탯 타입", statEntries[i].type);
            statEntries[i].value = EditorGUILayout.IntField("스탯 값", statEntries[i].value);

            if (GUILayout.Button("삭제",GUILayout.Width(50)))
            {
                statEntries.RemoveAt(i);
            }

            EditorGUILayout.EndHorizontal();
        }

        // 새로운 스탯 추가 버튼
        if (GUILayout.Button("스탯 추가"))
        {
            statEntries.Add(new StatEntry()); // 새로운 빈 StatEntry 객체 추가
        }

        // 아이템 저장 버튼 (아이템을 생성하고 저장하는 로직은 추가 필요)
        if (GUILayout.Button("아이템 생성"))
        {
            CreateItem();
        }
    }

    private void CreateItem()
    {
        ItemData itemData = new ItemData();
        itemData.ItemID = itemID;
        itemData.ItemName = itemName;
        itemData.ItemDescription = itemDescription;
        itemData.ItemType = itemType;
        itemData.stats = statEntries; 
        itemData.ItemIcon = itemIcon;
        
        AssetDatabase.CreateAsset(itemData, $"Assets/Resources/Items/{itemName}.asset" );
        AssetDatabase.SaveAssets();
    }
}
