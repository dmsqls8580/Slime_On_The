using UnityEngine;

[System.Flags]
public enum ItemType
{
    None = 0,
    Material = 1 << 0,
    Tool = 1 << 1,
    Equipable = 1 << 2,
    Eatable = 1 << 3,
    Placeable = 1 << 4,
    BuffItem = 1 << 5
}

public enum PlaceableType
{
    None,
    Building,
    Seed,
    Furniture
}

public enum ToolType 
{ 
    None, 
    Axe, 
    Pickaxe,
    Shovel,
    FishingRod,
    Hoe, 
    WateringCan 
}

public enum EquipableType 
{ 
    Armor, 
    Pants, 
    Shoes, 
    Necklace, 
    Ring 
}

[CreateAssetMenu(fileName = "New Item", menuName = "Item/Item")]
public class ItemSO : ScriptableObject
{
    [Header("기본 정보")]
    public string idx;
    public string itemName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("공통")]
    public ItemType itemTypes; // 다중 타입 가능
    public bool stackable;
    public int maxStack;

    [Header("타입별 데이터")]
    public MaterialData materialData;
    public ToolData toolData;
    public EquipableData equipableData;
    public EatableData eatableData;
    public PlaceableData placeableData;
}

[System.Serializable]
public class MaterialData
{
    public bool rottenable;
}

[System.Serializable]
public class ToolData
{
    public ToolType toolType;
    public float getAmount;
    public float luck;
    public float durability;
    public float atkSpd;
}

[System.Serializable]
public class EquipableData
{
    public EquipableType equipableType;
    public float maxHealth;
    public float atk;
    public float def;
    public float spd;
    public float crt;
}

[System.Serializable]
public class EatableData
{
    public float recoverHP;
    public float fullness;
    public float slimeGauge;
    public float duration;
    public bool rottenable;
    public bool permanent;
}

[System.Serializable]
public class PlaceableData
{
    public PlaceableType placeableType;
}